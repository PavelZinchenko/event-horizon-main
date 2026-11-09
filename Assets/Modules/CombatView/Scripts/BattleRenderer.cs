using System;
using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatView.Effects;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CombatView
{
    // Draws one core battle: ships, satellites, worm tail segments, bullets, beams, hit flashes and ship texts at
    // interpolated poses (migration phase 3c, step 4; phase 4, steps 5 and 7f; phase 5, steps 7d and 12c). Events decide
    // when a view exists; snapshots give its poses. Nothing is written back to the simulation. Bullet looks come from
    // the adapter as BulletVisual values.
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
        }

        public CombatViewSettings Settings { get; }

        // The side of the local player (legacy UnitSide.Player): a stealth ship of it stays more visible.
        public int ViewerSide { get; set; }

        // Ships and drones. Legacy rule: the ship's transform scale is its model scale. ShipState.Tint moves the colour
        // from white to tint.
        public void AddShip(EntityId id, Sprite sprite, float scale, Color? tint = null)
        {
            if (_ships.ContainsKey(id))
                return;

            var renderer = CreateRenderer("Ship", _root.transform, sprite ? sprite : _genericSprite, ShipSortingOrder);
            renderer.gameObject.SetActive(false);
            renderer.transform.localScale = Vector3.one * scale;
            _ships.Add(id, new EntityView(renderer.gameObject, renderer) { Tint = tint ?? Color.white });
        }

        // Legacy Decoy (phase 5, step 10c): the device colour, faded by ShipState.Life as legacy SpriteView with life
        // opacity (1 - (1 - life)^2).
        public void AddDecoy(EntityId id, Sprite sprite, float scale, Color color)
        {
            AddShip(id, sprite, scale, color);
            if (_ships.TryGetValue(id, out var view)) view.IsDecoy = true;
        }

        // Legacy SatelliteFactory (phase 5, step 12c): a satellite has its own body, so its view is not a child of the
        // ship. Add the ship's satellites in definition order, before its turrets.
        public void AddSatellite(EntityId id, Sprite sprite, float scale)
        {
            if (!_ships.TryGetValue(id, out var ship)) return;
            var renderer = CreateRenderer("Satellite", _root.transform, sprite ? sprite : _genericSprite, ShipSortingOrder);
            renderer.gameObject.SetActive(false);
            renderer.transform.localScale = Vector3.one * scale;
            ship.Satellites.Add(new EntityView(renderer.gameObject, renderer));
        }

        // The turret rides the interpolated hull or satellite; its local angle comes from the same two snapshots.
        public void AddTurret(EntityId id, int barrel, BarrelDefinition definition, Sprite sprite, float size, Color color)
        {
            if (!sprite || !_ships.TryGetValue(id, out var ship)) return;
            if (definition.Satellite >= ship.Satellites.Count) return;
            var carrier = definition.Satellite < 0 ? ship : ship.Satellites[definition.Satellite];
            var renderer = CreateRenderer("Turret", carrier.Root.transform, sprite, ShipSortingOrder + 1);
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

        public void RemoveShip(EntityId id, RemovalReason reason)
        {
            if (!_ships.TryGetValue(id, out var view))
                return;

            _ships.Remove(id);
            foreach (var turret in view.Turrets) Object.Destroy(turret.Renderer.gameObject);
            foreach (var shield in view.Shields) Object.Destroy(shield.Renderer.gameObject);
            // Legacy SatelliteController: the ship's death destroys its satellites (an explosion), a retreat makes them vanish.
            foreach (var satellite in view.Satellites)
                if (reason == RemovalReason.Death && satellite.HasPose)
                    _fades.Add(new Fade(satellite.Renderer));
                else
                    Object.Destroy(satellite.Root);
            if (reason == RemovalReason.Death && view.HasPose)
                _fades.Add(new Fade(view.Renderer));
            else
                Object.Destroy(view.Root);
        }

        // The body sprite of the segments of one worm tail (phase 5, step 7d); without one they draw the generic sprite.
        public void AddRepairBotLook(RepairBotDefinition definition, Sprite sprite, Color laserColor) =>
            _repairBotLooks[definition] = (sprite, laserColor);

        public void AddSegmentLook(WormTailDefinition definition, Sprite sprite)
        {
            if (sprite)
                _segmentLooks[definition] = sprite;
        }

        // A null visual draws the generic sprite or a plain line; it should not happen for a mapped weapon.
        public void AddBullet(in BattleEvent spawn, BulletVisual visual)
        {
            if (visual == BulletVisual.Hidden || _bullets.ContainsKey(spawn.EntityId))
                return;

            if (visual == null)
            {
                WarnOnce(ref _visualWarned, "bullet without view data");
                var flash = new HitFlash(GenericFlashDiameter, Color.white, GenericFlashDuration);
                visual = spawn.IsBeam
                    ? BulletVisual.ForLine(null, GenericBeamWidth, 0f, Color.white, flash)
                    : BulletVisual.ForSprite(null, ScaleToDiameter(_genericSprite, 2f * spawn.BulletDefinition.Radius), Color.white, flash);
            }

            var pool = _bulletPools[(int)visual.Kind];
            var view = pool.Count > 0 ? pool.Pop() : CreateBulletView(visual.Kind);
            Configure(view, visual);
            _bullets.Add(spawn.EntityId, view);
        }

        public void RemoveBullet(EntityId id)
        {
            if (!_bullets.TryGetValue(id, out var view))
                return;

            _bullets.Remove(id);
            view.Clear();
            _bulletPools[(int)view.Visual.Kind].Push(view);
        }

        // A predicted copy took its owner's ID (network plan, P2): the view stays.
        public void RenameBullet(EntityId previous, EntityId id)
        {
            if (!_bullets.TryGetValue(previous, out var view) || _bullets.ContainsKey(id))
                return;

            _bullets.Remove(previous);
            _bullets.Add(id, view);
        }

        // One accepted hit by a drawn bullet; rams and point defense have no bullet and no flash. A beam hits every
        // tick, so one attacker flashes at most every FlashInterval seconds.
        public void AddHit(in BattleEvent hit)
        {
            if (!_bullets.TryGetValue(hit.AttackerId, out var bullet) || _time - bullet.LastFlashTime < FlashInterval)
                return;

            bullet.LastFlashTime = _time;
            var look = bullet.Visual.Flash;
            AddFlash(hit.Point, look.Diameter, look.Color, look.Duration);
        }

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
                // Legacy WarpDrive: Opacity 0.1 while the warp runs (phase 5, step 7a). Legacy Detonator: the colour
                // moves to the device colour during the lock (step 7b). Legacy StealthDevice: 40% for the viewer's
                // side, 5% for the others (step 9b); Render moves the opacity there. Legacy Ghost and Fortification
                // multiply the colour by the device colour with its alpha (step 9d): a ghost fades to 25%.
                var color = view.IsDecoy ? view.Tint : Color.Lerp(Color.white, view.Tint, ship.Tint);
                if ((ship.Status & ShipStatusFlags.Faded) != 0) color.a *= FadedAlpha;
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

                if (bullet.HasRay)
                    view.PushEnd(bullet.BeamEndPoint);
                if (view.Visual.Kind == BulletViewKind.Harpoon)
                {
                    view.Line.enabled = snapshot.TryGetShip(bullet.ParentId, out var source);
                    view.PushEnd(view.Line.enabled ? source.Position : bullet.Position);
                }
                // The saw's sprite shares its collider's forward offset, scaled with the aura (phase 5, step 6a).
                var position = bullet.Position;
                if (!bullet.HasRay)
                    position = _torus.Wrap(position + Rot.FromAngle(bullet.Angle).Rotate(
                        bullet.Definition.CircleCenter * bullet.AuraPower));
                view.Push(position, bullet.Angle);

                if (view.Visual.Kind == BulletViewKind.Arc)
                {
                    var wave = bullet.Definition.Wave;
                    for (var i = 0; i < view.Line.positionCount; ++i)
                    {
                        var point = wave.ArcPoint((float)i / (view.Line.positionCount - 1)) * bullet.WaveScale;
                        view.Line.SetPosition(i, new Vector3(point.X, point.Y, 0f));
                    }
                }

                // An aura grows and shrinks with its power (phase 5, step 4g); the sprite takes the latest tick's size.
                if (view.Renderer && !bullet.IsBeam && bullet.Definition.IsAura)
                    view.Renderer.transform.localScale = Vector3.one * (view.Visual.SpriteScale * bullet.AuraPower);
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
            _time += deltaTime;
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
                PlaceBullet(pair.Value, alpha, anchor);

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
            _prefabs.Dispose();
            _eventViews.Clear();

            // The scene unload can destroy the root first.
            if (_root)
                Object.Destroy(_root);

            foreach (var material in _lineMaterials.Values)
                Object.Destroy(material);

            _ships.Clear();
            _bullets.Clear();
            _segments.Clear();
            _segmentLooks.Clear();
            _repairBots.Clear();
            _repairBotLooks.Clear();
            foreach (var pool in _bulletPools)
                pool.Clear();
            _fades.Clear();
            _flashes.Clear();
            _flashPool.Clear();
            _texts.Clear();
            _textPool.Clear();
            _lineMaterials.Clear();
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

        private void PlaceBullet(BulletView view, float alpha, Vector2 anchor)
        {
            if (!view.HasPose)
                return;

            var previous = view.Position;
            Place(view, alpha, anchor);

            // Legacy TrailView: a fresh view, or a jump over 1 unit (a new image at the wrap), starts a new trail.
            if (view.Trail && (!view.WasPlaced || (view.Position - previous).sqrMagnitude > 1f))
                view.Trail.Clear();
            view.WasPlaced = true;

            if (view.Line && view.Visual.Kind == BulletViewKind.Harpoon)
            {
                view.Line.SetPosition(0, view.Position);
                view.Line.SetPosition(1, Placement.NearestImage(_torus, view.Position, view.End.SamplePosition(alpha, _torus)));
            }
            if (view.Line && view.Visual.Kind == BulletViewKind.Line)
                PlaceLine(view, Placement.NearestImage(_torus, view.Position, view.End.SamplePosition(alpha, _torus)));
        }

        // Legacy LaserView.UpdateLine: four points, so the texture's end thirds keep their length (the border) and the
        // middle third stretches. A line shorter than two borders is hidden.
        private static void PlaceLine(BulletView view, Vector2 end)
        {
            var start = view.Position;
            var length = Vector2.Distance(start, end);
            var border = view.Visual.LineBorder;
            view.Line.enabled = length >= 2f * border;
            if (!view.Line.enabled)
                return;

            var direction = length > 0f ? (end - start) / length : Vector2.zero;
            view.Line.SetPosition(0, start);
            view.Line.SetPosition(1, start + direction * border);
            view.Line.SetPosition(2, end - direction * border);
            view.Line.SetPosition(3, end);
        }

        private BulletView CreateBulletView(BulletViewKind kind)
        {
            var root = new GameObject(kind.ToString());
            root.SetActive(false);
            root.transform.SetParent(_root.transform, false);

            SpriteRenderer sprite = null;
            TrailRenderer trail = null;
            LineRenderer line = null;
            switch (kind)
            {
                case BulletViewKind.Harpoon:
                    sprite = CreateRenderer("Sprite", root.transform, _genericSprite, BulletSortingOrder);
                    line = root.AddComponent<LineRenderer>();
                    line.useWorldSpace = true;
                    line.positionCount = 2;
                    line.sortingOrder = TrailSortingOrder;
                    break;
                case BulletViewKind.Rocket:
                    var trailObject = new GameObject("Trail");
                    trailObject.transform.SetParent(root.transform, false);
                    trail = trailObject.AddComponent<TrailRenderer>();
                    trail.sharedMaterial = _spriteMaterial;
                    trail.sortingOrder = TrailSortingOrder;
                    trail.time = TrailDuration;
                    trail.minVertexDistance = 0.1f;
                    trail.numCapVertices = 0;
                    sprite = CreateRenderer("Sprite", root.transform, _genericSprite, BulletSortingOrder);
                    break;
                case BulletViewKind.Line:
                case BulletViewKind.Arc:
                    line = root.AddComponent<LineRenderer>();
                    line.useWorldSpace = kind == BulletViewKind.Line;
                    line.positionCount = kind == BulletViewKind.Line ? 4 : 17;
                    line.sortingOrder = BulletSortingOrder;
                    line.numCapVertices = 0;
                    break;
                default:
                    sprite = CreateRenderer("Sprite", root.transform, _genericSprite, BulletSortingOrder);
                    break;
            }

            return new BulletView(root, sprite, trail, line);
        }

        private void Configure(BulletView view, BulletVisual visual)
        {
            view.Visual = visual;
            view.LastFlashTime = float.NegativeInfinity;

            if (view.Renderer)
            {
                view.Renderer.sprite = visual.Sprite ? visual.Sprite : _genericSprite;
                view.Renderer.color = visual.Color;
                view.Renderer.transform.localScale = Vector3.one * visual.SpriteScale;
            }

            if (view.Trail)
            {
                view.Trail.transform.localPosition = new Vector3(-visual.TrailOffset, 0f, 0f);
                view.Trail.startWidth = visual.TrailWidth;
                view.Trail.endWidth = 0.5f * visual.TrailWidth;
                view.Trail.startColor = visual.TrailColor;
                var end = visual.TrailColor;
                end.a = 0f;
                view.Trail.endColor = end;
            }

            if (view.Line)
            {
                view.Line.enabled = true;
                view.Line.sharedMaterial = LineMaterial(visual.Kind == BulletViewKind.Harpoon ? null : visual.Sprite);
                view.Line.startWidth = view.Line.endWidth = visual.LineWidth;
                view.Line.textureMode = visual.LineBorder > 0f ? LineTextureMode.DistributePerSegment : LineTextureMode.Stretch;
                view.Line.startColor = view.Line.endColor = visual.Color;
            }
        }

        // Legacy BeamInitializer: the record image's texture along the line. One material per texture.
        private Material LineMaterial(Sprite sprite)
        {
            if (!sprite)
                return _spriteMaterial;

            var texture = sprite.texture;
            if (!_lineMaterials.TryGetValue(texture, out var material))
            {
                material = new Material(_spriteMaterial) { mainTexture = texture };
                _lineMaterials.Add(texture, material);
            }

            return material;
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

            // A ship: the colour ShipState.Tint moves it to. A decoy: its colour.
            public Color Tint { get; set; } = Color.white;
            public bool IsDecoy { get; set; }

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

        private sealed class BulletView : EntityView
        {
            public BulletView(GameObject root, SpriteRenderer renderer, TrailRenderer trail, LineRenderer line)
                : base(root, renderer)
            {
                Trail = trail;
                Line = line;
            }

            public TrailRenderer Trail { get; }
            public LineRenderer Line { get; }
            public BulletVisual Visual { get; set; }
            public float LastFlashTime { get; set; }
            public bool WasPlaced { get; set; }

            // Beams and beam bolts: the ray end point, interpolated like the pose. Called before Push, so the first end point
            // starts the track together with the first pose.
            public PoseTrack End;

            public void PushEnd(Vec2 point)
            {
                if (HasPose)
                    End.Push(point, 0f);
                else
                    End.Reset(point, 0f);
            }

            public override void Clear()
            {
                base.Clear();
                WasPlaced = false;
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
        private const int TrailSortingOrder = 1;
        private const int BulletSortingOrder = 2;
        private const int FlashSortingOrder = 3;
        private const int TextSortingOrder = 4;
        private const float FadeDuration = 0.4f;
        private const float FadeScale = 1.3f;
        private const float FadedAlpha = 0.1f;
        private const float OwnStealthAlpha = 0.4f;
        private const float EnemyStealthAlpha = 0.05f;
        private const float GenericBeamWidth = 0.2f;
        private const float GenericFlashDiameter = 1f;
        private const float GenericFlashDuration = 0.2f;

        // The named effect prefabs (step 3), with their legacy names.
        private const string EffectFolder = "Effects/";

        // Legacy TrailView on CommonRocket (_duration).
        private const float TrailDuration = 0.5f;

        // "Views (5)", item 6.
        private const float FlashInterval = 0.1f;
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
        private float _time;

        private readonly Torus _torus;
        private readonly PrefabPool _prefabs;
        private readonly EffectPool _effects;
        private readonly CameraShake _shake;
        private readonly CombatSounds _sounds;
        private readonly List<IBattleEventView> _eventViews = new List<IBattleEventView>();
        private readonly Sprite _genericSprite;
        private readonly Sprite _flashSprite;
        private readonly Font _textFont;
        private readonly Material _spriteMaterial;
        private readonly GameObject _root;
        private readonly Dictionary<EntityId, EntityView> _ships = new Dictionary<EntityId, EntityView>();
        private readonly Dictionary<EntityId, BulletView> _bullets = new Dictionary<EntityId, BulletView>();
        private readonly Dictionary<EntityId, SegmentView> _segments = new Dictionary<EntityId, SegmentView>();
        private readonly Dictionary<WormTailDefinition, Sprite> _segmentLooks = new Dictionary<WormTailDefinition, Sprite>();
        private readonly List<EntityId> _removedSegments = new List<EntityId>();
        private int _segmentStamp;
        private readonly Dictionary<EntityId, RepairBotView> _repairBots = new Dictionary<EntityId, RepairBotView>();
        private readonly Dictionary<RepairBotDefinition, (Sprite sprite, Color color)> _repairBotLooks = new Dictionary<RepairBotDefinition, (Sprite, Color)>();
        private readonly List<EntityId> _removedRepairBots = new List<EntityId>();
        private int _repairBotStamp;
        private readonly System.Random _repairBotRandom = new System.Random();
        private readonly Stack<BulletView>[] _bulletPools = { new Stack<BulletView>(), new Stack<BulletView>(), new Stack<BulletView>(), new Stack<BulletView>(), new Stack<BulletView>() };
        private readonly List<Fade> _fades = new List<Fade>();
        private readonly List<Flash> _flashes = new List<Flash>();
        private readonly Stack<SpriteRenderer> _flashPool = new Stack<SpriteRenderer>();
        private readonly List<Text> _texts = new List<Text>();
        private readonly Stack<TextMesh> _textPool = new Stack<TextMesh>();
        private readonly System.Random _textRandom = new System.Random();
        private readonly Dictionary<Texture, Material> _lineMaterials = new Dictionary<Texture, Material>();
    }
}
