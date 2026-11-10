using System;
using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatView.Effects;
using CombatView.Views;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CombatView
{
    // Draws one core battle: ships, satellites, worm tail segments, bullets, beams, hit flashes and ship texts at
    // interpolated poses (migration phase 3c, step 4; phase 4, steps 5 and 7f; phase 5, steps 7d and 12c). Events decide
    // when a view exists; snapshots give its poses. Nothing is written back to the simulation. Bullet looks come from
    // the adapter as BulletVisual values, drawn on the shape prefabs (phase 5b, step 5); ship looks as ShipLook values
    // (step 4).
    public sealed class BattleRenderer : IDisposable
    {
        // The root goes into the given scene: the game loads scenes additively and never changes the active
        // scene, so a plain new GameObject would outlive the combat scene.
        // The flash sprite is legacy Flash's glow; without it flashes use the generic sprite. The text font is legacy
        // MessageText's; without it ship texts are not drawn. A null sound output plays nothing.
        public BattleRenderer(Torus torus, Sprite genericSprite, Sprite flashSprite, Font textFont, Scene scene,
            CombatViewSettings settings, ISoundOutput soundOutput)
        {
            _torus = torus;
            _genericSprite = genericSprite;
            _flashSprite = flashSprite ? flashSprite : genericSprite;
            _textFont = textFont;
            Settings = settings;
            _root = new GameObject("CoreBattleViews");
            SceneManager.MoveGameObjectToScene(_root, scene);

            // Phase 5b, step 2: the new prefabs, their effects and the sounds. Steps 3-8 add their users.
            _prefabs = new PrefabPool(_root.transform);
            _shake = new CameraShake();
            _effects = new EffectPool(_prefabs, torus, _root.transform, _shake);
            _sounds = new CombatSounds(soundOutput);

            // A new sprite renderer carries the built-in sprite material; lines and trails share it.
            var probe = _root.AddComponent<SpriteRenderer>();
            _spriteMaterial = probe.sharedMaterial;
            Object.Destroy(probe);

            // Step 4: the ship prefabs and their effects. The HSV material is shared with legacy (owner rule 3).
            _hsv = new HsvMaterials(_spriteMaterial, Resources.Load<Material>(HsvMaterialPath));
            if (settings.Trails)
                _trails = new TrailPool(_prefabs.Load(TrailPrefab)?.GetComponent<TrailRenderer>(), _root.transform);
            _shipAssets = new ShipEffectAssets(_effects, _prefabs, _trails, _sounds);

            // Step 5: the bullet prefabs, one template per look. A bullet without view data draws the generic sprite
            // or a plain beam. Step 6: a bullet without a look gets an empty root for its trigger effects.
            _bulletTemplates = new BulletTemplates(_prefabs);
            _genericBulletLook = new BulletLook(BulletViewKind.Projectile, genericSprite, Color.white, ColorMode.UseMyOwn,
                Color.white, ColorMode.UseMyOwn, ScaleToDiameter(genericSprite, 1f), 0f);
            _genericBeam = new BulletVisual(new BulletLook(BulletViewKind.Laser, null, Color.white, ColorMode.UseMyOwn,
                Color.white, ColorMode.UseMyOwn, GenericBeamWidth, 0f), 1f, Color.white);
            _emptyBullet = new GameObject("Bullet");
            _emptyBullet.SetActive(false);
            _emptyBullet.transform.SetParent(_root.transform, false);
        }

        public CombatViewSettings Settings { get; }

        // The side of the local player (legacy UnitSide.Player): a stealth ship of it stays more visible.
        public int ViewerSide { get; set; }

        // Ships and drones on the ship prefab (phase 5b, step 4), with their engine lights and trails, shield outline and
        // damage text. Legacy rule: the ship's transform scale is its model scale. The colour is the look's colour times
        // the way from white to its tint by ShipState.Tint. side: the ship's side, for the damage text opacity.
        public void AddShip(EntityId id, ShipLook look, int side) => AddShip(id, look, side, false);

        // Legacy Decoy (phase 5, step 10c): the device colour, faded by ShipState.Life as legacy SpriteView with life
        // opacity (1 - (1 - life)^2). It keeps the plain fade at its death until its own prefab (step 7).
        public void AddDecoy(EntityId id, Sprite sprite, float scale, Color color) =>
            AddShip(id, new ShipLook(sprite, scale) { Color = color }, ViewerSide, true);

        // Legacy SatelliteFactory (phase 5, step 12c): a satellite has its own body, so its view is not a child of the
        // ship. Add the ship's satellites in definition order, before its turrets.
        public void AddSatellite(EntityId id, Sprite sprite, float scale)
        {
            if (!_ships.TryGetValue(id, out var ship)) return;
            var view = CreateShipView(SatellitePrefab, _root.transform, sprite, ShipSortingOrder);
            view.gameObject.SetActive(false);
            view.transform.localScale = Vector3.one * scale;
            ship.Satellites.Add(new EntityView(view.gameObject, view.Renderer));
        }

        // The turret rides the interpolated hull or satellite; its local angle comes from the same two snapshots.
        // Legacy ShipFactory.CreatePlatform: the turret prefab with the HSV of the turret colour scheme.
        public void AddTurret(EntityId id, int barrel, BarrelDefinition definition, Sprite sprite, float size, Color color,
            float hue = 0f, float saturation = 0f)
        {
            if (!sprite || !_ships.TryGetValue(id, out var ship)) return;
            if (definition.Satellite >= ship.Satellites.Count) return;
            var carrier = definition.Satellite < 0 ? ship : ship.Satellites[definition.Satellite];
            var view = CreateShipView(TurretPrefab, carrier.Root.transform, sprite, ShipSortingOrder + 1);
            view.ApplyHsv(hue, saturation, _hsv);
            var renderer = view.Renderer;
            var pivot = sprite.pivot / sprite.rect.size;
            var scale = 0.5f / Mathf.Max(Mathf.Max(pivot.x, 1f - pivot.x), Mathf.Max(pivot.y, 1f - pivot.y));
            var carrierScale = carrier.Root.transform.localScale.x;
            renderer.transform.localScale = Vector3.one * (scale * size / carrierScale);
            renderer.color = color;
            ship.Turrets.Add(new TurretView(renderer, barrel, definition, size, carrierScale, color));
        }

        // Legacy EnergyShield unit (phase 5, step 10d): a child of the hull view at the shield's offset, size wide. Its
        // colour goes from transparent to the idle colour (the device colour, its alpha times the default opacity) by 4 x
        // the elapsed time of the distance; for 1 s after a hit it fades from the device colour to the idle one.
        public void AddShield(EntityId id, int deviceIndex, Sprite sprite, Vector2 offset, float size, Color color,
            float defaultOpacity)
        {
            if (!_ships.TryGetValue(id, out var ship)) return;
            var renderer = CreateRenderer("Shield", ship.Root.transform, sprite ? sprite : _genericSprite, ShipSortingOrder + 1);
            var carrierScale = ship.Root.transform.localScale.x;
            renderer.transform.localScale = Vector3.one * (size / carrierScale);
            renderer.transform.localPosition = new Vector3(offset.x, offset.y, 0f) / carrierScale;
            renderer.color = Color.clear;
            var idle = color;
            idle.a *= defaultOpacity;
            ship.Shields.Add(new ShieldView(renderer, deviceIndex, color, idle));
        }

        // Phase 5b, step 4: a death leaves the explosion with its sound and the wreck (legacy
        // ShipFactory.CreateDestructionEffect); a retreat leaves the warp (legacy ShipWarpEffect). The view goes at once,
        // with its turrets and shields.
        public void RemoveShip(EntityId id, RemovalReason reason)
        {
            if (!_ships.TryGetValue(id, out var view))
                return;

            _ships.Remove(id);
            view.Effects?.Dispose();
            view.DamageText?.Dispose();
            if (view.HasPose && !view.IsDecoy && reason == RemovalReason.Death)
                AddDeathEffects(view);
            if (view.HasPose && reason == RemovalReason.Retreat)
                AddWarpEffects(view);

            // Legacy SatelliteController: the ship's death destroys its satellites (an explosion), a retreat makes them vanish.
            foreach (var satellite in view.Satellites)
            {
                if (reason == RemovalReason.Death && satellite.HasPose)
                    AddSatelliteExplosion(satellite, view.Velocity);
                Object.Destroy(satellite.Root);
            }

            if (view.IsDecoy && reason == RemovalReason.Death && view.HasPose)
                _fades.Add(new Fade(view.Renderer));
            else
                Object.Destroy(view.Root);
        }

        // Phase 5b, step 4: the core's Damage event (owner, 2026-11-09) adds to the ship's damage text.
        public void AddDamage(in BattleEvent item)
        {
            if (_ships.TryGetValue(item.EntityId, out var view))
                view.DamageText?.ApplyDamage(item.DeviceIndex, item.Damage);
        }

        // The damage text colours by slot: 0 the shield, 1 + n damage type n (legacy DamageIndicator). Ships added
        // before the call have no damage text.
        public void SetDamageTextColors(params Color[] colors) => _damageColors = colors;

        // Clips of the legacy combat Settings component and of legacy hard-coded effects.
        public SoundClip RetreatSound { get => _shipAssets.RetreatSound; set => _shipAssets.RetreatSound = value; }
        public SoundClip WarpSound { get; set; }
        public SoundClip SatelliteExplosionSound { get; set; }

        // The body sprite of the segments of one worm tail (phase 5, step 7d); without one they draw the generic sprite.
        public void AddRepairBotLook(RepairBotDefinition definition, Sprite sprite, Color laserColor) =>
            _repairBotLooks[definition] = (sprite, laserColor);

        public void AddSegmentLook(WormTailDefinition definition, Sprite sprite)
        {
            if (sprite)
                _segmentLooks[definition] = sprite;
        }

        // Phase 5b, step 5: an instance of the look's template (the shape prefab of Resources/CombatView/Bullets). It
        // shows from the first snapshot. A null visual draws the generic sprite or a plain beam; it should not happen
        // for a mapped weapon. Step 6: the Created sounds play at once, the Created effects at the first snapshot.
        public void AddBullet(in BattleEvent spawn, BulletVisual visual)
        {
            if (_bullets.ContainsKey(spawn.EntityId))
                return;

            if (visual == null)
            {
                WarnOnce(ref _visualWarned, "bullet without view data");
                visual = spawn.IsBeam ? _genericBeam : new BulletVisual(_genericBulletLook,
                    2f * spawn.BulletDefinition.Radius, Color.white);
            }

            if (visual.IsEmpty)
                return;

            var template = visual.Look != null ? _bulletTemplates.Get(visual.Look) : null;
            if (visual.Look != null && !template && visual.Triggers.Count == 0)
                return;

            var instance = _prefabs.Get(template ? template : _emptyBullet, _root.transform);
            var setup = template ? instance.GetComponent<BulletViewSetup>() : null;
            if (setup)
                setup.Bind(_trails);
            var view = new BulletView(instance, setup, visual);

            // Legacy HarpoonController: the rope is as long as the weapon range.
            if (setup && setup.Rope && !spawn.IsBeam)
                setup.Rope.Initialize(spawn.BulletDefinition.Range);

            if (visual.Triggers.Count > 0)
            {
                view.Triggers = new BulletTriggerPlayer(visual.Triggers, _effects, _sounds);
                view.Triggers.Start();
            }

            _bullets.Add(spawn.EntityId, view);
        }

        // Legacy Bullet.Dispose: the view resets and its trail fades out where it is. A view that never showed has not
        // initialized itself, so it has nothing to reset. Step 6: first the triggers of the removal's conditions (the
        // core's BattleEvent.RemovalConditions) at the last snapshot pose; the bullet's effects then stay or go.
        public void RemoveBullet(EntityId id, BulletTriggerConditions conditions)
        {
            if (!_bullets.TryGetValue(id, out var view))
                return;

            _bullets.Remove(id);
            view.Triggers?.Remove(conditions, LastPose(view), view.HasPose);
            if (view.HasPose && view.View)
                view.View.Dispose();
            _prefabs.Release(view.Root);
        }

        // A predicted copy took its owner's ID (network plan, P2): the view stays.
        public void RenameBullet(EntityId previous, EntityId id)
        {
            if (!_bullets.TryGetValue(previous, out var view) || _bullets.ContainsKey(id))
                return;

            _bullets.Remove(previous);
            _bullets.Add(id, view);
        }

        // Phase 5b, step 6: one accepted hit. Legacy CollisionManager runs the collision actions of the bullet that hits,
        // then OnCollision on both units, so the bullet that hits and a bullet that is hit run their Hit bullet actions.
        // A gravity wave (a device index) runs no Hit trigger (phase 5, step 11c). A remote hit has no point (owner
        // question 9 (a)): its effects go to the target.
        public void AddHit(in BattleEvent hit)
        {
            var time = hit.Tick * Battle.TickDuration;
            if (_bullets.TryGetValue(hit.AttackerId, out var bullet) && bullet.Triggers != null)
            {
                var point = hit.Point;
                var velocity = Vector2.zero;
                if (_ships.TryGetValue(hit.EntityId, out var ship) && ship.HasPose)
                {
                    velocity = ship.Velocity;
                    if (point.Equals(Vec2.Zero))
                        point = new Vec2(ship.Position.x, ship.Position.y);
                }
                else if (point.Equals(Vec2.Zero))
                {
                    if (_bullets.TryGetValue(hit.EntityId, out var other) && other.HasPose)
                        point = LastPose(other).Point;
                    else if (bullet.HasPose)
                        point = LastPose(bullet).Point;
                }

                // A bullet that hits in its spawn tick (a beam's first tick) has no pose yet: its hit effects start at
                // the hit point, unturned, and its effects on the bullet wait for the pose.
                if (bullet.HasPose)
                {
                    var pose = CurrentPose(bullet);
                    bullet.Triggers.Hit(hit.Tick, hit.EntityId, point, velocity, pose);
                    bullet.Triggers.Collide(time, pose, bullet.LifetimeLeft);
                }
                else if (!point.Equals(Vec2.Zero))
                    bullet.Triggers.Hit(hit.Tick, hit.EntityId, point, velocity, new BulletPose(null, point, 0f,
                        bullet.Visual.BodySize, bullet.Visual.BodySize * bullet.AuraPower));
            }

            if (hit.DeviceIndex < 0)
                Collide(hit.EntityId, time);
        }

        // Point defense: legacy runs the Hit bullet actions of the disarmed bullet before its Disarmed ones.
        public void AddDisarm(in BattleEvent disarm) => Collide(disarm.EntityId, disarm.Tick * Battle.TickDuration);

        // One flash at a world point, as a hit flash: a teleporter jump draws one at each end (phase 5, step 11b).
        public void AddFlash(Vec2 point, float diameter, Color color, float duration)
        {
            var renderer = _flashPool.Count > 0 ? _flashPool.Pop() : CreateRenderer("Flash", _root.transform, _flashSprite, FlashSortingOrder);
            renderer.gameObject.SetActive(true);
            renderer.transform.localScale = Vector3.one * ScaleToDiameter(_flashSprite, Mathf.Max(MinFlashDiameter, diameter));
            _flashes.Add(new Flash(renderer, point, color, Mathf.Max(MinFlashDuration, duration)));
        }

        // Legacy RadioTransmitter and the MessageText effect (ShowMessage, step 7f): the text starts at the ship and
        // drifts for 2 s with the ship's velocity plus 1 unit per second in a random direction. The random direction is
        // presentation only, so it does not use the battle seed.
        public void AddText(string text, Color color, Vec2 position, Vec2 velocity)
        {
            if (!_textFont || string.IsNullOrEmpty(text))
                return;

            var mesh = _textPool.Count > 0 ? _textPool.Pop() : CreateText();
            mesh.gameObject.SetActive(true);
            mesh.text = text;
            mesh.color = color;
            var angle = (float)(_textRandom.NextDouble() * 2.0 * Math.PI);
            var drift = new Vec2(velocity.X + TextDriftSpeed * Mathf.Cos(angle), velocity.Y + TextDriftSpeed * Mathf.Sin(angle));
            _texts.Add(new Text(mesh, position, drift, color));
        }

        // Phase 5b, step 3: the effects and sounds for the view groups of steps 4-8 and the effect preview.
        public CombatSounds Sounds => _sounds;

        // The camera centre: a shake is weaker far from it. The adapter moves the camera rect by CameraOffset.
        public Vector2 ViewPoint { get => _shake.ViewPoint; set => _shake.ViewPoint = value; }
        public Vector2 CameraOffset => _shake.Offset;

        // A record or a named effect prefab (Resources/CombatView/Effects/<name>) at a world point. The caller sets its
        // position offset, size and colour, and runs it; null without a record or a prefab.
        public IEffect AddEffect(EffectRecord record, Vec2 point) => _effects.Create(record, point);
        public IEffect AddEffect(string prefab, Vec2 point) => _effects.Create(LoadEffect(prefab), point);

        // A record or a named effect prefab on a ship view, in its local space, so it moves and scales with it. The
        // ship's removal destroys it with the view.
        public IEffect AddShipEffect(EntityId id, EffectRecord record) =>
            _ships.TryGetValue(id, out var view) ? _effects.Create(record, view.Root.transform) : null;

        public IEffect AddShipEffect(EntityId id, string prefab) =>
            _ships.TryGetValue(id, out var view) ? _effects.Create(LoadEffect(prefab), view.Root.transform) : null;

        // The event feed (phase 5b, step 2): the adapter passes every event of the tick in order, after its own handling
        // and before Capture of the same snapshot.
        public void OnEvent(BattleSnapshot snapshot, in BattleEvent item)
        {
            foreach (var view in _eventViews)
                view.OnEvent(snapshot, item);
        }

        // After every step. Copies the poses; the snapshot spans are valid only until the next step.
        public void Capture(BattleSnapshot snapshot)
        {
            foreach (var ship in snapshot.Ships)
            {
                if (!_ships.TryGetValue(ship.Id, out var view))
                    continue;

                view.Push(ship.Position, ship.Angle);
                view.Velocity = new Vector2(ship.LinearVelocity.X, ship.LinearVelocity.Y);
                view.AngularVelocity = ship.AngularVelocity * Mathf.Rad2Deg;
                view.Effects?.Capture(ship.Throttle, ship.Shield.Exists ? ship.Shield.Value / ship.Shield.Max : 0f,
                    ship.IsRetreating);
                // Legacy WarpDrive: Opacity 0.1 while the warp runs (phase 5, step 7a). Legacy Detonator: the colour
                // moves to the device colour during the lock (step 7b). Legacy StealthDevice: 40% for the viewer's
                // side, 5% for the others (step 9b); Render moves the opacity there. Legacy Ghost and Fortification
                // multiply the colour by the device colour with its alpha (step 9d): a ghost fades to 25%. Phase 5b,
                // step 4: the look's colour (legacy Features colour) multiplies the result.
                var look = view.Look;
                var color = view.IsDecoy ? look.Color : look.Color * Color.Lerp(Color.white, look.Tint ?? Color.white, ship.Tint);
                view.Opacity = (ship.Status & ShipStatusFlags.Faded) != 0 ? FadedAlpha : 1f;
                color.a *= view.Opacity;
                if (view.IsDecoy) color.a *= 1f - (1f - ship.Life) * (1f - ship.Life);
                view.Color = color;
                view.StealthTarget = (ship.Status & ShipStatusFlags.Stealth) == 0 ? 1f
                    : ship.Side == ViewerSide ? OwnStealthAlpha : EnemyStealthAlpha;
                var satellites = snapshot.GetSatellites(ship);
                for (var i = 0; i < view.Satellites.Count && i < satellites.Length; ++i)
                    view.Satellites[i].Push(satellites[i].Position, satellites[i].Angle);
                var barrels = snapshot.GetBarrels(ship);
                foreach (var turret in view.Turrets)
                {
                    turret.Previous = turret.HasAngle ? turret.Current : barrels[turret.Barrel].Angle;
                    turret.Current = barrels[turret.Barrel].Angle;
                    turret.HasAngle = true;
                }
                ApplyColor(view);
            }

            foreach (var shield in snapshot.Shields)
            {
                if (!_ships.TryGetValue(shield.ShipId, out var view)) continue;
                foreach (var shieldView in view.Shields)
                {
                    if (shieldView.DeviceIndex != shield.DeviceIndex) continue;
                    shieldView.On = shield.IsOn;
                    shieldView.HitTime = shield.HitAge < ShieldHitTicks ? shield.HitAge * Battle.TickDuration : float.MaxValue;
                }
            }

            foreach (var bullet in snapshot.Bullets)
            {
                if (!_bullets.TryGetValue(bullet.Id, out var view))
                    continue;

                // Legacy Bullet.UpdateView: the view's life is the share of the lifetime left, 1 in the spawn tick. The
                // first snapshot gives the full lifetime; a beam the device holds keeps 1.
                var life = 1f;
                if (bullet.ExpiryTick != long.MaxValue)
                {
                    var left = bullet.ExpiryTick - snapshot.Tick + 1;
                    if (view.LifetimeTicks <= 0)
                        view.LifetimeTicks = Math.Max(1L, left);
                    life = Mathf.Clamp01((float)left / view.LifetimeTicks);
                }

                view.PreviousLife = view.HasPose ? view.Life : life;
                view.Life = life;
                view.LifetimeLeft = bullet.ExpiryTick == long.MaxValue ? float.PositiveInfinity
                    : (bullet.ExpiryTick - snapshot.Tick + 1) * Battle.TickDuration;
                view.ParentId = bullet.ParentId;
                // An aura grows and shrinks with its power (phase 5, step 4g); the view takes the latest tick's size.
                view.AuraPower = bullet.IsBeam ? 1f : bullet.AuraPower;
                view.WaveScale = bullet.WaveScale;

                if (bullet.HasRay)
                    view.PushEnd(bullet.BeamEndPoint);
                var first = !view.HasPose;
                view.Push(bullet.Position, bullet.Angle);

                // Step 6: the Created effects at the first pose; the Cooldown triggers on every tick.
                if (view.Triggers == null)
                    continue;
                var pose = CurrentPose(view);
                if (first)
                    view.Triggers.Show(pose, view.LifetimeLeft);
                view.Triggers.Tick(snapshot.Tick * Battle.TickDuration, pose, view.LifetimeLeft);
            }

            CaptureSegments(snapshot);
            CaptureRepairBots(snapshot);
        }

        // Worm tail segments have no events: a view lives while the snapshot lists its segment. Legacy rule: the segment's
        // transform scale is its body scale. Legacy wreck sprites and the spawn and explosion effects stay out (H11).
        private void CaptureSegments(BattleSnapshot snapshot)
        {
            ++_segmentStamp;
            foreach (var segment in snapshot.Segments)
            {
                if (!_segments.TryGetValue(segment.Id, out var view))
                {
                    if (!_segmentLooks.TryGetValue(segment.Definition, out var sprite))
                        sprite = _genericSprite;
                    var renderer = CreateRenderer("Segment", _root.transform, sprite, SegmentSortingOrder);
                    renderer.gameObject.SetActive(false);
                    renderer.transform.localScale = Vector3.one * segment.Scale;
                    view = new SegmentView(renderer.gameObject, renderer);
                    _segments.Add(segment.Id, view);
                }

                view.Push(segment.Position, segment.Angle);
                view.Stamp = _segmentStamp;
            }

            if (_segments.Count == snapshot.Segments.Length)
                return;

            foreach (var pair in _segments)
                if (pair.Value.Stamp != _segmentStamp)
                    _removedSegments.Add(pair.Key);
            foreach (var id in _removedSegments)
            {
                Object.Destroy(_segments[id].Root);
                _segments.Remove(id);
            }
            _removedSegments.Clear();
        }

        private void CaptureRepairBots(BattleSnapshot snapshot)
        {
            ++_repairBotStamp;
            foreach (var bot in snapshot.RepairBots)
            {
                if (!_repairBots.TryGetValue(bot.Id, out var view))
                {
                    _repairBotLooks.TryGetValue(bot.Definition, out var look);
                    var renderer = CreateRenderer("RepairBot", _root.transform, look.sprite ? look.sprite : _genericSprite, ShipSortingOrder);
                    renderer.transform.localScale = Vector3.one * bot.Scale;
                    var line = renderer.gameObject.AddComponent<LineRenderer>();
                    line.sharedMaterial = _spriteMaterial;
                    line.useWorldSpace = true;
                    line.positionCount = 2;
                    line.startWidth = line.endWidth = 0.04f;
                    line.startColor = line.endColor = look.color;
                    line.sortingOrder = BulletSortingOrder;
                    view = new RepairBotView(renderer, line, bot.ShipId);
                    _repairBots.Add(bot.Id, view);
                }
                view.Push(bot.Position, bot.Angle);
                view.Line.enabled = bot.Repairing;
                view.Jitter = bot.Repairing ? _repairBotRandom.Next(-30, 30) : 0f;
                view.Stamp = _repairBotStamp;
            }
            foreach (var pair in _repairBots)
                if (pair.Value.Stamp != _repairBotStamp) _removedRepairBots.Add(pair.Key);
            foreach (var id in _removedRepairBots)
            {
                Object.Destroy(_repairBots[id].Root);
                _repairBots.Remove(id);
            }
            _removedRepairBots.Clear();
        }

        // Once per frame. The controlled ship goes to its image nearest to the anchor, and the anchor follows it,
        // so its drawn path never jumps at the wrap. Every other view goes to its image nearest to the new anchor.
        // Without a controlled ship the anchor stays. Alpha is the share of the next tick that has passed.
        public void Render(float alpha, EntityId controlled, ref Vector2 anchor, float deltaTime)
        {
            _shake.Update(Time.unscaledDeltaTime);

            if (!controlled.IsNull && _ships.TryGetValue(controlled, out var controlledView) && controlledView.HasPose)
                anchor = Place(controlledView, alpha, anchor);

            foreach (var pair in _ships)
                if (pair.Key != controlled)
                    Place(pair.Value, alpha, anchor);

            foreach (var ship in _ships.Values)
                foreach (var satellite in ship.Satellites)
                    Place(satellite, alpha, anchor);

            foreach (var ship in _ships.Values)
                foreach (var shield in ship.Shields)
                {
                    shield.Power = Mathf.Lerp(shield.Power, shield.On ? 1f : 0f, ShieldFadeRate * deltaTime);
                    shield.HitTime += deltaTime;
                    var color = shield.HitTime < 1f ? Color.Lerp(shield.Active, shield.Idle, shield.HitTime) : shield.Idle;
                    shield.Renderer.color = Color.Lerp(Color.clear, color, shield.Power);
                }

            // Legacy StealthDevice.OnUpdateView: the opacity moves by 1 per second.
            foreach (var ship in _ships.Values)
            {
                if (ship.StealthOpacity == ship.StealthTarget) continue;
                ship.StealthOpacity = Mathf.MoveTowards(ship.StealthOpacity, ship.StealthTarget, deltaTime);
                ApplyColor(ship);
            }

            // Phase 5b, step 4: legacy Features.Color and Features.Opacity, with the stealth opacity.
            foreach (var ship in _ships.Values)
            {
                var color = ship.Color;
                color.a *= ship.StealthOpacity;
                ship.Effects?.Update(deltaTime, color, ship.Opacity * ship.StealthOpacity);
                ship.DamageText?.Update(deltaTime);
            }

            foreach (var ship in _ships.Values)
                foreach (var turret in ship.Turrets)
                {
                    var angle = Mathf.LerpAngle(turret.Previous * Mathf.Rad2Deg, turret.Current * Mathf.Rad2Deg, alpha);
                    var radians = angle * Mathf.Deg2Rad;
                    var position = turret.Definition.PositionAt(radians) - 0.5f * turret.Size *
                        new Vec2(Mathf.Cos(radians), Mathf.Sin(radians));
                    turret.Renderer.transform.localPosition = new Vector3(position.X, position.Y, 0f) / turret.CarrierScale;
                    turret.Renderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }

            foreach (var pair in _bullets)
                PlaceBullet(pair.Value, alpha, anchor, deltaTime);

            foreach (var pair in _segments)
                Place(pair.Value, alpha, anchor);

            foreach (var bot in _repairBots.Values)
            {
                Place(bot, alpha, anchor);
                if (!bot.Line.enabled || !_ships.TryGetValue(bot.Ship, out var parent)) continue;
                bot.Renderer.transform.rotation = Quaternion.Euler(0f, 0f, bot.Degrees + bot.Jitter);
                var delta = _torus.Delta(new Vec2(bot.Position.x, bot.Position.y), new Vec2(parent.Position.x, parent.Position.y));
                delta = Rot.FromAngle(bot.Jitter * Mathf.Deg2Rad).Rotate(delta);
                bot.Line.SetPosition(0, new Vector3(bot.Position.x, bot.Position.y, 0f));
                bot.Line.SetPosition(1, new Vector3(bot.Position.x + delta.X, bot.Position.y + delta.Y, 0f));
            }

            UpdateFades(deltaTime);
            UpdateFlashes(deltaTime, anchor);
            UpdateTexts(deltaTime, anchor);
            _effects.Update(deltaTime, anchor);
        }

        // The pose of the last Render, in Unity units and degrees.
        public bool TryGetShipPose(EntityId id, out Vector2 position, out float degrees)
        {
            if (_ships.TryGetValue(id, out var view) && view.HasPose)
            {
                position = view.Position;
                degrees = view.Degrees;
                return true;
            }

            position = default;
            degrees = 0f;
            return false;
        }

        public void Dispose()
        {
            _sounds.Dispose();
            _effects.Dispose();
            _trails?.Dispose();
            _prefabs.Dispose();
            _hsv.Dispose();
            _eventViews.Clear();

            // The scene unload can destroy the root first.
            if (_root)
                Object.Destroy(_root);

            _ships.Clear();
            _bullets.Clear();
            _bulletTemplates.Clear();
            _segments.Clear();
            _segmentLooks.Clear();
            _repairBots.Clear();
            _repairBotLooks.Clear();
            _fades.Clear();
            _flashes.Clear();
            _flashPool.Clear();
            _texts.Clear();
            _textPool.Clear();
        }

        private void AddShip(EntityId id, ShipLook look, int side, bool isDecoy)
        {
            if (_ships.ContainsKey(id) || look == null)
                return;

            var shipView = CreateShipView(ShipPrefab, _root.transform, look.Sprite, ShipSortingOrder);
            shipView.gameObject.SetActive(false);
            shipView.transform.localScale = Vector3.one * look.Scale;
            if (look.IsHsv)
                shipView.ApplyHsv(look.Hue, look.Saturation, _hsv);

            var view = new EntityView(shipView.gameObject, shipView.Renderer) { Look = look, IsDecoy = isDecoy };
            if (!isDecoy)
                view.Effects = new ShipEffects(look, shipView.transform, _shipAssets);

            // Legacy ShipFactory: no damage text for drones; 0.75 opacity for the player's side, 0.5 for the others.
            if (!isDecoy && !look.IsDrone && Settings.DamageText && _damageColors != null)
            {
                var opacity = side == ViewerSide ? 0.75f : 0.5f;
                var colors = new Color[_damageColors.Length];
                for (var i = 0; i < colors.Length; ++i)
                    colors[i] = new Color(_damageColors[i].r, _damageColors[i].g, _damageColors[i].b, opacity);
                view.DamageText = new DamageText(colors, (damage, color) => ShowDamage(view, damage, color));
            }

            _ships.Add(id, view);
        }

        // A view of the ship, satellite or turret prefab; a plain ShipView without the prefab.
        private ShipView CreateShipView(string path, Transform parent, Sprite sprite, int sortingOrder)
        {
            var prefab = _prefabs.Load(path);
            var view = prefab ? Object.Instantiate(prefab, parent, false).GetComponent<ShipView>() : null;
            if (!view)
            {
                var gameObject = new GameObject(path);
                gameObject.transform.SetParent(parent, false);
                view = gameObject.AddComponent<ShipView>();
            }

            view.Renderer.sprite = sprite ? sprite : _genericSprite;
            view.Renderer.sortingOrder = sortingOrder;
            return view;
        }

        // Legacy ShipExplosionAction and ShipWreckAction at the view's last pose.
        private void AddDeathEffects(EntityView view)
        {
            var look = view.Look;
            var point = new Vec2(view.Position.x, view.Position.y);
            var drift = view.Velocity * 0.05f;
            if (look.Explosion != null)
            {
                var explosion = _effects.Create(look.Explosion, point);
                if (explosion != null)
                {
                    explosion.Rotation = view.Degrees;
                    explosion.Size = look.Scale;
                    explosion.Run(look.Explosion.MaxElementLifetime, drift, 0f);
                }
            }
            else
                AddObsoleteExplosion(look, point, drift);

            _sounds.Play(look.ExplosionSound);

            if (!look.LeavesWreck)
                return;

            var wreck = _effects.Create(_prefabs.Load(WreckPrefab), point);
            if (wreck == null)
                return;

            var renderer = ((Component)wreck).GetComponent<SpriteRenderer>();
            if (renderer)
                renderer.sprite = look.Sprite;
            wreck.Rotation = view.Degrees;
            wreck.Size = look.Scale;
            wreck.Color = Color.Lerp(Color.black, look.Color, 0.5f);
            if (Settings.StaticWrecks)
                wreck.Run(WreckLifetime, Vector2.zero, 0);
            else
                wreck.Run(WreckLifetime, view.Velocity * 0.2f, view.AngularVelocity);
        }

        // Legacy ShipExplosionActionObsolete and DroneExplosionActionObsolete, for a ship without an explosion record.
        // Their smoke layers wait for the Smoke prefabs (step 8); the disturbance has no view in the new scene.
        private void AddObsoleteExplosion(ShipLook look, Vec2 point, Vector2 drift)
        {
            if (look.IsSmall)
            {
                RunEffect(FlashPrefab, point, ExplosionColor, look.Scale * 4f, 0.3f, drift);
                return;
            }

            RunEffect(WavePrefab, point, ExplosionColor, look.Scale * 6f, 1f, drift);
            RunEffect(FlashAdditivePrefab, point, ExplosionColor, look.Scale * 4f, 1f, drift);
        }

        // Legacy DroneExplosionActionObsolete of a satellite: a flash and explosion_10; the smoke waits for step 8.
        private void AddSatelliteExplosion(EntityView satellite, Vector2 velocity)
        {
            var point = new Vec2(satellite.Position.x, satellite.Position.y);
            RunEffect(FlashPrefab, point, ExplosionColor, satellite.Root.transform.localScale.x * 4f, 0.3f, velocity * 0.05f);
            _sounds.Play(SatelliteExplosionSound);
        }

        // Legacy ShipWarpEffect.
        private void AddWarpEffects(EntityView view)
        {
            var point = new Vec2(view.Position.x, view.Position.y);
            var drift = view.Velocity * 0.05f;
            var scale = view.Look.Scale;
            RunEffect(WavePrefab, point, ShipEffects.WarpColor, scale * 6f, 1f, drift);
            RunEffect(FlashAdditivePrefab, point, ShipEffects.WarpColor, scale * 5f, 1f, drift);
            RunEffect(FlashAdditivePrefab, point, Color.white, scale * 4f, 1f, drift);
            _sounds.Play(WarpSound);
        }

        private void RunEffect(string prefab, Vec2 point, Color color, float size, float lifetime, Vector2 velocity)
        {
            var effect = _effects.Create(_prefabs.Load(prefab), point);
            if (effect == null)
                return;

            effect.Color = color;
            effect.Size = size;
            effect.Run(lifetime, velocity, 0f);
        }

        // Legacy EffectFactory.CreateDamageTextEffect: at the ship, for 2 s, drifting with the ship's velocity plus 1
        // unit per second in a random direction; nothing off screen.
        private void ShowDamage(EntityView view, float damage, Color color)
        {
            var camera = Camera.main;
            if (camera)
            {
                var viewport = camera.WorldToViewportPoint(view.Position);
                if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
                    return;
            }

            var effect = _effects.Create(_prefabs.Load(DamageTextPrefab), new Vec2(view.Position.x, view.Position.y));
            if (effect == null)
                return;

            if (effect is TextEffect text)
                text.Text = DamageText.Format(damage);
            effect.Color = color;
            var angle = (float)(_textRandom.NextDouble() * 2.0 * Math.PI);
            effect.Run(DamageTextLifetime, view.Velocity + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), 0f);
        }

        // Legacy Satellite.OnUpdateView: the satellite takes its owner's colour; a turret keeps its own with the ship's
        // opacity.
        private static void ApplyColor(EntityView ship)
        {
            var color = ship.Color;
            color.a *= ship.StealthOpacity;
            ship.Renderer.color = color;
            foreach (var satellite in ship.Satellites)
                satellite.Renderer.color = color;
            foreach (var turret in ship.Turrets)
                turret.Renderer.color = new Color(turret.Color.r, turret.Color.g, turret.Color.b, color.a);
        }

        private Vector2 Place(EntityView view, float alpha, Vector2 reference)
        {
            if (!view.HasPose)
                return reference;

            view.Position = Placement.NearestImage(_torus, reference, view.Pose.SamplePosition(alpha, _torus));
            view.Degrees = view.Pose.SampleAngle(alpha) * Mathf.Rad2Deg;
            view.Root.transform.SetPositionAndRotation(view.Position, Quaternion.Euler(0f, 0f, view.Degrees));
            return view.Position;
        }

        // Legacy Bullet.UpdateView after the body: the root takes the pose and the legacy root scale (BodySize), then the
        // view takes the life, and a beam its length. A beam root points at the ray end, so its line runs along local x.
        private void PlaceBullet(BulletView view, float alpha, Vector2 anchor, float deltaTime)
        {
            if (!view.HasPose)
                return;

            Place(view, alpha, anchor);
            var root = view.Root.transform;
            var visual = view.Visual;
            root.localScale = Vector3.one * (visual.BodySize * view.AuraPower);
            if (!view.View)
                return;

            var baseView = view.View;
            if (!view.WasPlaced)
            {
                baseView.Color = visual.Color;
                view.WasPlaced = true;
            }

            baseView.Life = Mathf.Lerp(view.PreviousLife, view.Life, alpha);
            if (visual.Look.IsLine && view.HasEnd)
            {
                var end = Placement.NearestImage(_torus, view.Position, view.End.SamplePosition(alpha, _torus));
                var delta = end - view.Position;
                if (delta.sqrMagnitude > 0f)
                    root.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                baseView.Size = delta.magnitude;
            }
            else if (visual.Look.Kind == BulletViewKind.Arc && visual.BodySize > 0f)
                baseView.Size = view.WaveScale / visual.BodySize;

            baseView.UpdateView(deltaTime);

            // Legacy DistanceJointVisualizer: the rope ends at the drawn ship; without it the rope is hidden.
            var rope = view.Setup.Rope;
            if (rope)
                rope.UpdateRope(_ships.TryGetValue(view.ParentId, out var ship) && ship.HasPose ? ship.Position : (Vector2?)null);
        }

        private void UpdateFades(float deltaTime)
        {
            for (var i = _fades.Count - 1; i >= 0; --i)
            {
                var fade = _fades[i];
                fade.Time += deltaTime;
                var progress = fade.Time / FadeDuration;
                if (progress >= 1f || !fade.Renderer)
                {
                    if (fade.Renderer)
                        Object.Destroy(fade.Renderer.gameObject);
                    _fades.RemoveAt(i);
                    continue;
                }

                var color = fade.Renderer.color;
                color.a = 1f - progress;
                fade.Renderer.color = color;
                fade.Renderer.transform.localScale = fade.Scale * (1f + (FadeScale - 1f) * progress);
                _fades[i] = fade;
            }
        }

        // A flash stays at its contact point, placed again each frame because the anchor moves. Legacy Flash prefab:
        // no growth; opacity 1 - t^2 times an alpha scale of 2.
        private void UpdateFlashes(float deltaTime, Vector2 anchor)
        {
            for (var i = _flashes.Count - 1; i >= 0; --i)
            {
                var flash = _flashes[i];
                flash.Time += deltaTime;
                var progress = flash.Time / flash.Duration;
                if (progress >= 1f || !flash.Renderer)
                {
                    if (flash.Renderer)
                    {
                        flash.Renderer.gameObject.SetActive(false);
                        _flashPool.Push(flash.Renderer);
                    }

                    _flashes.RemoveAt(i);
                    continue;
                }

                var color = flash.Color;
                color.a = Mathf.Clamp01(color.a * FlashAlphaScale * (1f - progress * progress));
                flash.Renderer.color = color;
                flash.Renderer.transform.position = Placement.NearestImage(_torus, anchor, flash.Point);
                _flashes[i] = flash;
            }
        }

        // Legacy TextEffect with fading: opacity 1 - t^2 and scale 1 - t^3 of the camera's orthographic size, where t
        // is the share of the lifetime that has passed, so the text keeps its screen size while the camera zooms.
        private void UpdateTexts(float deltaTime, Vector2 anchor)
        {
            var camera = Camera.main;
            var cameraScale = camera ? camera.orthographicSize : 1f;
            for (var i = _texts.Count - 1; i >= 0; --i)
            {
                var text = _texts[i];
                text.Time += deltaTime;
                var progress = text.Time / TextDuration;
                if (progress >= 1f || !text.Mesh)
                {
                    if (text.Mesh)
                    {
                        text.Mesh.gameObject.SetActive(false);
                        _textPool.Push(text.Mesh);
                    }

                    _texts.RemoveAt(i);
                    continue;
                }

                text.Point = new Vec2(text.Point.X + text.Velocity.X * deltaTime, text.Point.Y + text.Velocity.Y * deltaTime);
                var color = text.Color;
                color.a *= 1f - progress * progress;
                text.Mesh.color = color;
                var transform = text.Mesh.transform;
                transform.localScale = Vector3.one * (cameraScale * (1f - progress * progress * progress));
                var position = Placement.NearestImage(_torus, anchor, text.Point);
                transform.position = new Vector3(position.x, position.y, TextOffsetZ);
                _texts[i] = text;
            }
        }

        // Legacy MessageText prefab: TextMesh, character size 0.04, font size 20, centred.
        private TextMesh CreateText()
        {
            var gameObject = new GameObject("Text");
            gameObject.transform.SetParent(_root.transform, false);
            var mesh = gameObject.AddComponent<TextMesh>();
            mesh.font = _textFont;
            mesh.characterSize = TextCharacterSize;
            mesh.fontSize = TextFontSize;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            var renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _textFont.material;
            renderer.sortingOrder = TextSortingOrder;
            return mesh;
        }

        private GameObject LoadEffect(string name) => string.IsNullOrEmpty(name) ? null : _prefabs.Load(EffectFolder + name);

        private static SpriteRenderer CreateRenderer(string name, Transform parent, Sprite sprite, int sortingOrder)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static float ScaleToDiameter(Sprite sprite, float diameter)
        {
            if (!sprite)
                return diameter;

            var size = sprite.bounds.size;
            var extent = Mathf.Max(size.x, size.y);
            return extent > 0f ? diameter / extent : diameter;
        }

        // Step 6: a bullet's place for its triggers at the latest snapshot, on its root.
        private BulletPose CurrentPose(BulletView view)
        {
            var pose = LastPose(view);
            return new BulletPose(view.Root.transform, pose.Point, pose.Degrees, pose.BodySize, pose.WorldScale);
        }

        // The latest snapshot pose without the root: where a removed bullet leaves its effects.
        private BulletPose LastPose(BulletView view) => new BulletPose(null, _torus.Wrap(view.Pose.SamplePosition(1f, _torus)),
            view.Pose.SampleAngle(1f) * Mathf.Rad2Deg, view.Visual.BodySize, view.Visual.BodySize * view.AuraPower);

        // Legacy Bullet.OnCollision on a bullet that is hit or disarmed.
        private void Collide(EntityId id, float time)
        {
            if (_bullets.TryGetValue(id, out var bullet) && bullet.Triggers != null && bullet.HasPose)
                bullet.Triggers.Collide(time, CurrentPose(bullet), bullet.LifetimeLeft);
        }

        private static void WarnOnce(ref bool warned, string kind)
        {
            if (warned)
                return;

            warned = true;
            Debug.LogWarning($"BattleRenderer: {kind}; the generic view is used");
        }

        private class EntityView
        {
            public EntityView(GameObject root, SpriteRenderer renderer)
            {
                Root = root;
                Renderer = renderer;
            }

            public GameObject Root { get; }

            // Null for a line.
            public SpriteRenderer Renderer { get; }

            // Ships and decoys (phase 5b, step 4): the look, the ship's effects and damage text, and the last
            // snapshot's velocities (Unity units; degrees per second).
            public ShipLook Look { get; set; }
            public bool IsDecoy { get; set; }
            public ShipEffects Effects { get; set; }
            public DamageText DamageText { get; set; }
            public Vector2 Velocity { get; set; }
            public float AngularVelocity { get; set; }

            // Legacy Features.Opacity without the stealth: 0.1 while faded.
            public float Opacity { get; set; } = 1f;

            // A ship: the colour of the last snapshot, and the stealth opacity with its target (phase 5, step 9b).
            public Color Color { get; set; } = Color.white;
            public float StealthOpacity { get; set; } = 1f;
            public float StealthTarget { get; set; } = 1f;
            public readonly List<TurretView> Turrets = new List<TurretView>();
            public readonly List<EntityView> Satellites = new List<EntityView>();
            public readonly List<ShieldView> Shields = new List<ShieldView>();
            public PoseTrack Pose;
            public bool HasPose { get; private set; }
            public Vector2 Position { get; set; }
            public float Degrees { get; set; }

            // The first pose after a spawn starts the track, so a new view does not slide in from an old pose.
            public void Push(Vec2 position, float angle)
            {
                if (HasPose)
                {
                    Pose.Push(position, angle);
                    return;
                }

                Pose.Reset(position, angle);
                HasPose = true;
                Root.SetActive(true);
            }

            public virtual void Clear()
            {
                HasPose = false;
                Root.SetActive(false);
            }
        }

        private sealed class TurretView
        {
            public readonly SpriteRenderer Renderer;
            public readonly int Barrel;
            public readonly BarrelDefinition Definition;
            public readonly float Size, CarrierScale;
            public readonly Color Color;
            public float Previous, Current;
            public bool HasAngle;
            public TurretView(SpriteRenderer renderer, int barrel, BarrelDefinition definition, float size, float carrierScale, Color color)
            { Renderer = renderer; Barrel = barrel; Definition = definition; Size = size; CarrierScale = carrierScale; Color = color; }
        }

        private sealed class ShieldView
        {
            public readonly SpriteRenderer Renderer;
            public readonly int DeviceIndex;
            public readonly Color Active, Idle;
            public bool On;
            public float Power;

            // Seconds since the last hit (legacy _timeFromLastHit, 100 at the start).
            public float HitTime = float.MaxValue;
            public ShieldView(SpriteRenderer renderer, int deviceIndex, Color active, Color idle)
            { Renderer = renderer; DeviceIndex = deviceIndex; Active = active; Idle = idle; }
        }

        private sealed class RepairBotView : EntityView
        {
            public RepairBotView(SpriteRenderer renderer, LineRenderer line, EntityId ship) : base(renderer.gameObject, renderer)
            { Line = line; Ship = ship; }
            public readonly LineRenderer Line;
            public readonly EntityId Ship;
            public float Jitter;
            public int Stamp;
        }

        private sealed class SegmentView : EntityView
        {
            public SegmentView(GameObject root, SpriteRenderer renderer) : base(root, renderer) { }

            // The last capture that listed the segment.
            public int Stamp { get; set; }
        }

        // Phase 5b, step 5: an instance of a bullet prefab. The pool keeps the instance; this wrapper lives with the bullet.
        private sealed class BulletView : EntityView
        {
            public BulletView(GameObject root, BulletViewSetup setup, BulletVisual visual) : base(root, null)
            {
                Setup = setup;
                View = setup ? setup.View : null;
                Visual = visual;
            }

            // Null for a bullet without a look (step 6): its root carries only trigger effects.
            public BulletViewSetup Setup { get; }
            public BaseView View { get; }
            public BulletVisual Visual { get; }
            public bool WasPlaced { get; set; }

            // Step 6: null without sound or effect triggers. The seconds of lifetime left at the latest snapshot.
            public BulletTriggerPlayer Triggers { get; set; }
            public float LifetimeLeft { get; set; } = float.PositiveInfinity;

            // The life of the last two snapshots, and the ticks of the full lifetime.
            public float PreviousLife { get; set; } = 1f;
            public float Life { get; set; } = 1f;
            public long LifetimeTicks { get; set; }

            public EntityId ParentId { get; set; }
            public float AuraPower { get; set; } = 1f;
            public float WaveScale { get; set; }

            // Beams and beam bolts: the ray end point, interpolated like the pose. Called before Push, so the first end point
            // starts the track together with the first pose.
            public PoseTrack End;
            public bool HasEnd { get; private set; }

            public void PushEnd(Vec2 point)
            {
                if (HasPose)
                    End.Push(point, 0f);
                else
                    End.Reset(point, 0f);
                HasEnd = true;
            }
        }

        private struct Fade
        {
            public Fade(SpriteRenderer renderer)
            {
                Renderer = renderer;
                Scale = renderer.transform.localScale;
                Time = 0f;
            }

            public SpriteRenderer Renderer;
            public Vector3 Scale;
            public float Time;
        }

        private struct Flash
        {
            public Flash(SpriteRenderer renderer, Vec2 point, Color color, float duration)
            {
                Renderer = renderer;
                Point = point;
                Color = color;
                Duration = duration;
                Time = 0f;
            }

            public SpriteRenderer Renderer;
            public Vec2 Point;
            public Color Color;
            public float Duration;
            public float Time;
        }

        private struct Text
        {
            public Text(TextMesh mesh, Vec2 point, Vec2 velocity, Color color)
            {
                Mesh = mesh;
                Point = point;
                Velocity = velocity;
                Color = color;
                Time = 0f;
            }

            public TextMesh Mesh;
            public Vec2 Point;
            public Vec2 Velocity;
            public Color Color;
            public float Time;
        }

        // Legacy EnergyShield: the power moves by 4 x the elapsed time; a hit shows for 1 s.
        private const float ShieldFadeRate = 4f;
        private const long ShieldHitTicks = 50;

        private const int SegmentSortingOrder = -1;
        private const int ShipSortingOrder = 0;
        private const int BulletSortingOrder = 2;
        private const int FlashSortingOrder = 3;
        private const int TextSortingOrder = 4;
        private const float FadeDuration = 0.4f;
        private const float FadeScale = 1.3f;
        private const float FadedAlpha = 0.1f;
        private const float OwnStealthAlpha = 0.4f;
        private const float EnemyStealthAlpha = 0.05f;
        private const float GenericBeamWidth = 0.2f;

        // The named effect prefabs (step 3), with their legacy names.
        private const string EffectFolder = "Effects/";

        // Step 4: the ship prefabs and effects, with their legacy names; the shared HSV material.
        private const string ShipPrefab = "Ships/Default";
        private const string SatellitePrefab = "Satellites/default";
        private const string TurretPrefab = "Guns/turret";
        private const string TrailPrefab = "Objects/TrailRenderer";
        private const string WreckPrefab = "Effects/Wreck";
        private const string DamageTextPrefab = "Effects/DamageText";
        private const string FlashPrefab = "Effects/Flash";
        private const string FlashAdditivePrefab = "Effects/FlashAdditive";
        private const string WavePrefab = "Effects/Wave";
        private const string HsvMaterialPath = "Materials/ShipHsv";

        // Legacy ShipWreckAction, EffectFactory.CreateDamageTextEffect and ColorTable.ShipExplosionColor.
        private const float WreckLifetime = 60f;
        private const float DamageTextLifetime = 2f;
        private static readonly Color ExplosionColor = new Color(1f, 0.66f, 0.59f);

        // "Views (5)", item 6.
        private const float FlashAlphaScale = 2f;
        private const float MinFlashDiameter = 0.3f;
        private const float MinFlashDuration = 0.05f;

        // Legacy RadioTransmitter (_speed, time) and MessageText (TextMesh).
        private const float TextDriftSpeed = 1f;
        private const float TextDuration = 2f;
        private const float TextCharacterSize = 0.04f;
        private const int TextFontSize = 20;
        private const float TextOffsetZ = -0.1f;

        private bool _visualWarned;

        private readonly Torus _torus;
        private readonly PrefabPool _prefabs;
        private readonly EffectPool _effects;
        private readonly CameraShake _shake;
        private readonly CombatSounds _sounds;
        private readonly HsvMaterials _hsv;
        private readonly TrailPool _trails;
        private readonly ShipEffectAssets _shipAssets;
        private Color[] _damageColors;
        private readonly List<IBattleEventView> _eventViews = new List<IBattleEventView>();
        private readonly Sprite _genericSprite;
        private readonly Sprite _flashSprite;
        private readonly Font _textFont;
        private readonly Material _spriteMaterial;
        private readonly GameObject _root;
        private readonly Dictionary<EntityId, EntityView> _ships = new Dictionary<EntityId, EntityView>();
        private readonly Dictionary<EntityId, BulletView> _bullets = new Dictionary<EntityId, BulletView>();
        private readonly BulletTemplates _bulletTemplates;
        private readonly BulletLook _genericBulletLook;
        private readonly BulletVisual _genericBeam;
        private readonly GameObject _emptyBullet;
        private readonly Dictionary<EntityId, SegmentView> _segments = new Dictionary<EntityId, SegmentView>();
        private readonly Dictionary<WormTailDefinition, Sprite> _segmentLooks = new Dictionary<WormTailDefinition, Sprite>();
        private readonly List<EntityId> _removedSegments = new List<EntityId>();
        private int _segmentStamp;
        private readonly Dictionary<EntityId, RepairBotView> _repairBots = new Dictionary<EntityId, RepairBotView>();
        private readonly Dictionary<RepairBotDefinition, (Sprite sprite, Color color)> _repairBotLooks = new Dictionary<RepairBotDefinition, (Sprite, Color)>();
        private readonly List<EntityId> _removedRepairBots = new List<EntityId>();
        private int _repairBotStamp;
        private readonly System.Random _repairBotRandom = new System.Random();
        private readonly List<Fade> _fades = new List<Fade>();
        private readonly List<Flash> _flashes = new List<Flash>();
        private readonly Stack<SpriteRenderer> _flashPool = new Stack<SpriteRenderer>();
        private readonly List<Text> _texts = new List<Text>();
        private readonly Stack<TextMesh> _textPool = new Stack<TextMesh>();
        private readonly System.Random _textRandom = new System.Random();
    }
}
