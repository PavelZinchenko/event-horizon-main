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
    // (step 4), with the sounds and effects of their weapons, devices and drone bays (step 7a). Decoys, shields, repair
    // bots and worm tail segments use the device object prefabs (step 7b).
    public sealed class BattleRenderer : IDisposable
    {
        // The root goes into the given scene: the game loads scenes additively and never changes the active
        // scene, so a plain new GameObject would outlive the combat scene.
        // The text font is legacy MessageText's; without it ship texts are not drawn. A null sound output plays nothing.
        public BattleRenderer(Torus torus, Sprite genericSprite, Font textFont, Scene scene, CombatViewSettings settings,
            ISoundOutput soundOutput)
        {
            _torus = torus;
            _genericSprite = genericSprite;
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
            _deviceAssets = new DeviceEffectAssets(_effects, _prefabs, _sounds);

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
        public void AddShip(EntityId id, ShipLook look, int side)
        {
            if (_ships.ContainsKey(id) || look == null)
                return;

            var shipView = CreateShipView(ShipPrefab, _root.transform, look.Sprite, ShipSortingOrder);
            shipView.gameObject.SetActive(false);
            shipView.transform.localScale = Vector3.one * look.Scale;
            if (look.IsHsv)
                shipView.ApplyHsv(look.Hue, look.Saturation, _hsv);

            var view = new EntityView(shipView.gameObject, shipView.Renderer)
            {
                Look = look,
                Effects = new ShipEffects(look, shipView.transform, _shipAssets),
            };

            // Legacy ShipFactory: no damage text for drones; 0.75 opacity for the player's side, 0.5 for the others.
            if (!look.IsDrone && Settings.DamageText && _damageColors != null)
            {
                var opacity = side == ViewerSide ? 0.75f : 0.5f;
                var colors = new Color[_damageColors.Length];
                for (var i = 0; i < colors.Length; ++i)
                    colors[i] = new Color(_damageColors[i].r, _damageColors[i].g, _damageColors[i].b, opacity);
                view.DamageText = new DamageText(colors, (damage, color) => ShowDamage(view, damage, color));
            }

            _ships.Add(id, view);
        }

        // Legacy SpaceObjectFactory.CreateDecoy (phase 5, step 10c): the Decoy prefab at the body scale in the device
        // colour (step 7b). Its views ignore the life, as legacy. Step 7a, legacy DecoyDevice: the device sound once per
        // use (the decoys of one tick), and a FlashAdditive of flashSize in the device colour for 0.3 s at each decoy's
        // first pose.
        public void AddDecoy(EntityId id, float scale, Color color, SoundClip sound, float flashSize, long tick)
        {
            if (_ships.ContainsKey(id))
                return;

            var prefab = _prefabs.Load(DecoyPrefab);
            if (!prefab)
                return;

            var instance = Object.Instantiate(prefab, _root.transform, false);
            instance.SetActive(false);
            instance.transform.localScale = Vector3.one * scale;
            _ships.Add(id, new EntityView(instance, null)
            {
                Look = new ShipLook(null, scale) { Color = color },
                IsDecoy = true,
                View = instance.GetComponent<BaseView>(),
                SpawnFlashSize = flashSize,
            });
            if (sound.IsEmpty || (_decoySoundTick == tick && _decoySound.Clip == sound.Clip))
                return;

            _decoySoundTick = tick;
            _decoySound = sound;
            _sounds.Play(sound);
        }

        // Step 7a: a bullet spawn of a weapon (owner question 8 (b)). The adapter maps the bullet's definition to the
        // weapon's device; bullets of one tick are one shot.
        public void AddShot(EntityId ship, int device, long tick)
        {
            if (_ships.TryGetValue(ship, out var view))
                Devices(view)?.Shot(device, tick);
        }

        // Step 7a: a drone bay or a clonning center launched child. Legacy ClonningDevice runs the device's effect on the
        // clone for 0.5 s at the prefab's size and colour.
        public void AddLaunch(EntityId mothership, int device, EntityId child)
        {
            if (!_ships.TryGetValue(mothership, out var view) || view.IsDecoy)
                return;

            Devices(view)?.Launch(device);
            var look = device >= 0 && device < view.Look.Devices.Count ? view.Look.Devices[device] : null;
            if (look == null || look.Kind != DeviceViewKind.ClonningCenter || !_ships.TryGetValue(child, out var clone))
                return;

            var effect = look.Record != null ? _effects.Create(look.Record, clone.Root.transform)
                : _effects.Create(LoadEffect(look.Prefab), clone.Root.transform);
            effect?.Run(look.Lifetime, Vector2.zero, 0f);
        }

        // Step 7a: legacy TeleporterDevice at both ends of the jump (phase 5, step 11b).
        public void AddTeleport(in BattleEvent item)
        {
            if (_ships.TryGetValue(item.EntityId, out var view))
                Devices(view)?.Teleport(item.DeviceIndex, item.Point, item.Destination, view.HasPose ? LastDegrees(view) : 0f);
        }

        // Step 7a: legacy GravityGenerator (phase 5, step 11c); its effect follows the ship.
        public void AddGravityWave(in BattleEvent item)
        {
            if (_ships.TryGetValue(item.EntityId, out var view))
                Devices(view)?.GravityWave(item.DeviceIndex);
        }

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
        // Step 7b: the EnergyShield, EnergyShieldOutline or FrontalShield prefab (legacy SatelliteFactory and
        // CustomPrefabLoader).
        public void AddShield(EntityId id, int deviceIndex, ShieldLook look)
        {
            if (look == null || !_ships.TryGetValue(id, out var ship)) return;
            var prefab = _prefabs.Load(ShieldPrefab(look.Kind));
            var view = prefab ? Object.Instantiate(prefab, ship.Root.transform, false).GetComponent<BaseView>() : null;
            if (!view) return;

            var carrierScale = ship.Root.transform.localScale.x;
            view.transform.localScale = Vector3.one * (look.Size / carrierScale);
            view.transform.localPosition = new Vector3(look.Offset.x, look.Offset.y, 0f) / carrierScale;
            if (view is CircleView circle)
            {
                circle.Initialize(look.Image, look.ImageScale, look.Thickness, look.AspectRatio);
                circle.Renderer.sortingOrder = ShipSortingOrder + 1;
            }
            else if (view.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                if (look.Image) renderer.sprite = look.Image;
                renderer.sortingOrder = ShipSortingOrder + 1;
            }

            view.Color = Color.clear;
            var idle = look.Color;
            idle.a *= look.IdleOpacity;
            ship.Shields.Add(new ShieldView(view, deviceIndex, look.Color, idle));
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
            view.Devices?.Dispose();
            view.DamageText?.Dispose();
            if (view.HasPose && !view.IsDecoy && reason == RemovalReason.Death)
                AddDeathEffects(view);
            if (view.HasPose && reason == RemovalReason.Retreat)
                AddWarpEffects(view);

            // Legacy SatelliteController: the ship's death destroys its satellites (an explosion), a retreat makes them vanish.
            foreach (var satellite in view.Satellites)
            {
                if (reason == RemovalReason.Death && satellite.HasPose)
                    AddDroneExplosion(satellite.Position, satellite.Root.transform.localScale.x, view.Velocity);
                Object.Destroy(satellite.Root);
            }

            // Step 7b: legacy Decoy has DroneExplosionActionObsolete.
            if (view.IsDecoy && reason == RemovalReason.Death && view.HasPose)
                AddDroneExplosion(view.Position, view.Look.Scale, view.Velocity);
            Object.Destroy(view.Root);
        }

        // Step 7b: the core's AuxiliaryRemoval of a repair bot or a tail segment. Legacy RepairBot and the worm segments
        // have DroneExplosionActionObsolete; it runs at a death, not at a vanish or a retreat.
        public void RemoveAuxiliary(EntityId id, Vec2 point, RemovalReason reason)
        {
            var bot = _repairBots.TryGetValue(id, out var botView) ? botView : null;
            var segment = bot == null && _segments.TryGetValue(id, out var segmentView) ? segmentView : null;
            var view = (EntityView)bot ?? segment;
            if (view == null)
                return;

            if (reason == RemovalReason.Death && view.HasPose)
                AddDroneExplosion(new Vector2(point.X, point.Y), view.Scale, Vector2.zero);

            if (bot != null)
            {
                _repairBots.Remove(id);
                DisposeRepairBot(bot);
            }
            else
            {
                _segments.Remove(id);
                DisposeSegment(segment);
            }
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
        public SoundClip DroneExplosionSound { get; set; }
        public SoundClip AsteroidExplosionSound { get; set; }

        // Phase 5b, step 7a: legacy SatelliteFactory.CreateRepairBot plays the device sound while the bot repairs; a loop
        // stops when it ends. Step 7b: the RepairBot prefab with its beam.
        public void AddRepairBotLook(RepairBotDefinition definition, RepairBotLook look) => _repairBotLooks[definition] = look;

        // The segment prefab of one worm tail (phase 5, step 7d; phase 5b, step 7b).
        public void AddSegmentLook(WormTailDefinition definition, SegmentLook look) => _segmentLooks[definition] = look;

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

            // Step 7c (owner, 2026-11-11): the detonating ship dies in this tick; its explosion record replaces the ship's.
            if (visual.Burst != null && _ships.TryGetValue(spawn.ParentId, out var detonated))
                detonated.Detonated = true;

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
            else
                Zap(hit.AttackerId, hit.DeviceIndex, hit.EntityId);
        }

        // Point defense: legacy runs the Hit bullet actions of the disarmed bullet before its Disarmed ones. Step 7a: the
        // zap and its sound.
        public void AddDisarm(in BattleEvent disarm)
        {
            Zap(disarm.AttackerId, disarm.DeviceIndex, disarm.EntityId);
            Collide(disarm.EntityId, disarm.Tick * Battle.TickDuration);
        }

        // Phase 5b, step 8: legacy AsteroidExplosionAction. Grey Smoke at 2 x the diameter and SmokeAdditive at 1.5 x, for
        // 2 s each at a random angle and spin, and explosion_08. The disturbance has no view in the new scene.
        public void AddAsteroidExplosion(Vec2 point, float diameter)
        {
            RunEffect(SmokePrefab, point, Color.gray, diameter * 2f, 2f, Vector2.zero, EffectMath.Random.Next(360), EffectMath.Random.Next(-10, 10));
            RunEffect(SmokeAdditivePrefab, point, Color.gray, diameter * 1.5f, 2f, Vector2.zero, EffectMath.Random.Next(360),
                EffectMath.Random.Next(-10, 10));
            _sounds.Play(AsteroidExplosionSound);
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
                if (view.SpawnFlashSize > 0f)
                    AddDecoyFlash(view, ship.Position);
                Devices(view)?.Capture(snapshot.GetDevices(ship), snapshot.GetBarrels(ship),
                    (ship.Status & ShipStatusFlags.Faded) != 0, ship.Position, ship.Angle * Mathf.Rad2Deg);
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
                if (first && view.Visual.Burst != null)
                    PlayBurst(view.Visual, bullet.Position);

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

        // Worm tail segments have no spawn events: a view lives while the snapshot lists its segment, or until the core's
        // AuxiliaryRemoval (step 7b). Legacy rule: the segment's transform scale is its body scale. Step 7b, legacy
        // SpaceObjectFactory.CreateWormTailSegment: the segment prefab with the ship's colour scheme; legacy
        // WormSegmentFactory runs OrbAdditive on each segment after the head, 2 wide in the scheme colour at half alpha,
        // for 0.5 s.
        private void CaptureSegments(BattleSnapshot snapshot)
        {
            ++_segmentStamp;
            foreach (var segment in snapshot.Segments)
            {
                var created = false;
                if (!_segments.TryGetValue(segment.Id, out var view))
                {
                    view = CreateSegment(segment);
                    _segments.Add(segment.Id, view);
                    created = true;
                }

                view.Push(segment.Position, segment.Angle);
                view.Stamp = _segmentStamp;
                if (created && segment.Number > 1)
                    view.SpawnEffect = AddSegmentSpawnEffect(view, segment.ShipId);
            }

            if (_segments.Count == snapshot.Segments.Length)
                return;

            foreach (var pair in _segments)
                if (pair.Value.Stamp != _segmentStamp)
                    _removedSegments.Add(pair.Key);
            foreach (var id in _removedSegments)
            {
                DisposeSegment(_segments[id]);
                _segments.Remove(id);
            }
            _removedSegments.Clear();
        }

        private SegmentView CreateSegment(in TailSegmentState segment)
        {
            _segmentLooks.TryGetValue(segment.Definition, out var look);
            var prefab = _prefabs.Load(look != null && look.IsSegment2 ? Segment2Prefab : SegmentPrefab);
            GameObject root;
            if (prefab)
                root = Object.Instantiate(prefab, _root.transform, false);
            else
                root = CreateRenderer("Segment", _root.transform, _genericSprite, SegmentSortingOrder).gameObject;

            root.SetActive(false);
            root.transform.localScale = Vector3.one * segment.Scale;
            var segmentView = root.GetComponent<ShipSegmentView>();
            if (segmentView)
            {
                if (look != null && look.HasRecord && !look.IsSegment2)
                    segmentView.Initialize(look.Body, look.Joint, look.JointScale, look.JointOffset);
                if (_ships.TryGetValue(segment.ShipId, out var ship) && ship.Look != null)
                    segmentView.ApplyHsv(ship.Look.Hue, ship.Look.Saturation, _hsv);
            }

            return new SegmentView(root);
        }

        private IEffect AddSegmentSpawnEffect(SegmentView view, EntityId shipId)
        {
            if (!_ships.TryGetValue(shipId, out var ship) || ship.Look == null)
                return null;

            var effect = _effects.Create(_prefabs.Load(OrbAdditivePrefab), view.Root.transform);
            if (effect == null)
                return null;

            var color = ship.Look.Color;
            color.a *= 0.5f;
            effect.Size = SegmentSpawnEffectSize;
            effect.Color = color;
            effect.Run(SegmentSpawnEffectLifetime, Vector2.zero, 0f);
            return effect;
        }

        private static void DisposeSegment(SegmentView view)
        {
            if (view.SpawnEffect != null && view.SpawnEffect.IsAlive)
                view.SpawnEffect.Dispose();
            Object.Destroy(view.Root);
        }

        // Repair bots have no spawn events either; the core's AuxiliaryRemoval ends them (step 7b).
        private void CaptureRepairBots(BattleSnapshot snapshot)
        {
            ++_repairBotStamp;
            foreach (var bot in snapshot.RepairBots)
            {
                if (!_repairBots.TryGetValue(bot.Id, out var view))
                {
                    view = CreateRepairBot(bot);
                    _repairBots.Add(bot.Id, view);
                }
                view.Push(bot.Position, bot.Angle);
                if (bot.Repairing && !view.Repairing)
                    StartRepairSound(view);
                else if (!bot.Repairing && view.Repairing)
                    StopRepairSound(view);
                view.Repairing = bot.Repairing;
                view.Jitter = bot.Repairing ? _repairBotRandom.Next(-30, 30) : 0f;
                view.Stamp = _repairBotStamp;
            }
            foreach (var pair in _repairBots)
                if (pair.Value.Stamp != _repairBotStamp) _removedRepairBots.Add(pair.Key);
            foreach (var id in _removedRepairBots)
            {
                DisposeRepairBot(_repairBots[id]);
                _repairBots.Remove(id);
            }
            _removedRepairBots.Clear();
        }

        // Step 7b, legacy SatelliteFactory.CreateRepairBot: the RepairBot prefab at the device size, and the Laser effect
        // on it, 0.4 x the device size ahead, 1 + flight radius + device size long, in the device colour. Both values are
        // in the bot's frame, which the device size scales, as legacy. Step 7c: the beam is the Laser element of the
        // device's record; its Size multiplies the length, its colour mode combines its colour with the device colour,
        // and its offset and rotation add to the legacy pose.
        private RepairBotView CreateRepairBot(in RepairBotState bot)
        {
            _repairBotLooks.TryGetValue(bot.Definition, out var look);
            var prefab = _prefabs.Load(RepairBotPrefab);
            var root = prefab ? Object.Instantiate(prefab, _root.transform, false)
                : CreateRenderer("RepairBot", _root.transform, _genericSprite, ShipSortingOrder).gameObject;
            root.SetActive(false);
            root.transform.localScale = Vector3.one * bot.Scale;

            var view = new RepairBotView(root) { Sound = look?.Sound ?? default };
            var element = look?.Beam;
            var beam = _effects.Create(element, root.transform);
            if (beam != null)
            {
                beam.Position = new Vector2(RepairBeamOffset * bot.Definition.Size, 0f) + element.Offset;
                beam.Rotation = element.Rotation;
                beam.Color = element.ColorMode.Apply(element.Color, look.Color);
                beam.Size = element.Size * (1f + bot.Definition.Range + bot.Definition.Size);
                beam.Life = 0f;
                beam.Visible = false;
                view.Beam = beam;
            }

            return view;
        }

        private void DisposeRepairBot(RepairBotView view)
        {
            StopRepairSound(view);
            if (view.Beam != null && view.Beam.IsAlive)
                view.Beam.Dispose();
            Object.Destroy(view.Root);
        }

        // Legacy SoundEffect on the bot: a plain clip plays at each start, a loop runs until the bot stops repairing.
        private void StartRepairSound(RepairBotView view)
        {
            if (view.Sound.IsEmpty)
                return;
            if (view.Sound.Loop)
                view.SoundId = _sounds.Start(view.Sound);
            else
                _sounds.Play(view.Sound);
        }

        private void StopRepairSound(RepairBotView view)
        {
            _sounds.Stop(view.SoundId);
            view.SoundId = 0;
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
                    shield.View.Color = Color.Lerp(Color.clear, color, shield.Power);
                    shield.View.UpdateView(deltaTime);
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
                if (ship.View)
                    ship.View.UpdateView(deltaTime);
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

            // Step 7a: the barrel flashes ride on the drawn barrels.
            foreach (var ship in _ships.Values)
                ship.Devices?.Update(deltaTime, alpha);

            foreach (var pair in _bullets)
                PlaceBullet(pair.Value, alpha, anchor, deltaTime);

            foreach (var pair in _segments)
                Place(pair.Value, alpha, anchor);

            // Legacy UnitEffect: the beam fades in and out over 0.1 s while the bot repairs. The bot faces its ship; the
            // jitter turns it with its beam.
            foreach (var bot in _repairBots.Values)
            {
                Place(bot, alpha, anchor);
                if (bot.Repairing)
                    bot.Root.transform.rotation = Quaternion.Euler(0f, 0f, bot.Degrees + bot.Jitter);
                var beam = bot.Beam;
                if (beam == null || !beam.IsAlive) continue;
                if (bot.Repairing)
                {
                    beam.Visible = true;
                    beam.Life = Mathf.Clamp01(beam.Life + deltaTime / RepairBeamFadeTime);
                }
                else if (beam.Visible)
                {
                    beam.Life = Mathf.Clamp01(beam.Life - deltaTime / RepairBeamFadeTime);
                    if (beam.Life <= 0f) beam.Visible = false;
                }
            }

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
            _texts.Clear();
            _textPool.Clear();
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
            if (view.Detonated)
            {
                // Step 7c: the detonator's record is the explosion.
            }
            else if (look.Explosion != null)
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
        // The disturbance has no view in the new scene.
        private void AddObsoleteExplosion(ShipLook look, Vec2 point, Vector2 drift)
        {
            if (look.IsSmall)
            {
                AddDroneExplosionEffects(point, look.Scale, drift);
                return;
            }

            RunEffect(WavePrefab, point, ExplosionColor, look.Scale * 6f, 1f, drift);
            RunEffect(FlashAdditivePrefab, point, ExplosionColor, look.Scale * 4f, 1f, drift);
            RunEffect(SmokeAdditivePrefab, point, ExplosionColor, look.Scale * 1.5f, 1f, drift, 0f, EffectMath.Random.Next(-20, 20));
            RunEffect(SmokePrefab, point, ShipSmokeColor, look.Scale * 2f, 3f, drift, 0f, EffectMath.Random.Next(-20, 20));
        }

        // Legacy DroneExplosionActionObsolete of a satellite, a decoy, a repair bot or a tail segment: explosion_10.
        private void AddDroneExplosion(Vector2 position, float scale, Vector2 velocity)
        {
            AddDroneExplosionEffects(new Vec2(position.x, position.y), scale, velocity * 0.05f);
            _sounds.Play(DroneExplosionSound);
        }

        // Legacy DroneExplosionActionObsolete: a flash at 4 x the body scale for 0.3 s and black smoke at 2 x for 1 s.
        private void AddDroneExplosionEffects(Vec2 point, float scale, Vector2 drift)
        {
            RunEffect(FlashPrefab, point, ExplosionColor, scale * 4f, 0.3f, drift);
            RunEffect(SmokePrefab, point, Color.black, scale * 2f, 1f, drift, 0f, EffectMath.Random.Next(-30, 30));
        }

        // Step 7c: the detonator's explosion record at the area's first pose, at its radius, in the device colour, for the
        // whole record (legacy CreateStrongExplosion ran its effects for DetonatorDevice's 1 s).
        private void PlayBurst(BulletVisual visual, Vec2 point)
        {
            var effect = _effects.Create(visual.Burst, point);
            if (effect == null)
                return;
            effect.Color = visual.Color;
            effect.Size = 0.5f * visual.BodySize;
            effect.Run(0f, Vector2.zero, 0f);
        }

        private static string ShieldPrefab(ShieldViewKind kind)
        {
            switch (kind)
            {
                case ShieldViewKind.EnergyOutline: return EnergyShieldOutlinePrefab;
                case ShieldViewKind.Frontal: return FrontalShieldPrefab;
                default: return EnergyShieldPrefab;
            }
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

        private void RunEffect(string prefab, Vec2 point, Color color, float size, float lifetime, Vector2 velocity,
            float rotation = 0f, float angularVelocity = 0f)
        {
            var effect = _effects.Create(_prefabs.Load(prefab), point);
            if (effect == null)
                return;

            effect.Color = color;
            effect.Size = size;
            effect.Rotation = rotation;
            effect.Run(lifetime, velocity, angularVelocity);
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
            if (ship.Renderer)
                ship.Renderer.color = color;
            if (ship.View)
                ship.View.Color = color;
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

        // Step 7a: built on first use, after the adapter added the ship's satellites, which carry some barrels. Decoys
        // have no devices.
        private DeviceEffects Devices(EntityView view)
        {
            if (view.Devices != null || view.IsDecoy || view.Look.Devices.Count == 0)
                return view.Devices;

            view.Devices = new DeviceEffects(view.Look.Devices, view.Look.Engines, view.Root.transform,
                satellite => satellite < 0 ? view.Root.transform
                    : satellite < view.Satellites.Count ? view.Satellites[satellite].Root.transform : null,
                _deviceAssets);
            return view.Devices;
        }

        // Legacy PointDefenseCollisionAction: the zap runs from the ship to the target's last snapshot pose.
        private void Zap(EntityId attacker, int device, EntityId target)
        {
            if (!_ships.TryGetValue(attacker, out var ship) || !ship.HasPose || Devices(ship) == null)
                return;

            Vec2 to;
            if (_bullets.TryGetValue(target, out var bullet) && bullet.HasPose)
                to = LastPose(bullet).Point;
            else if (_ships.TryGetValue(target, out var other) && other.HasPose)
                to = other.Pose.SamplePosition(1f, _torus);
            else
                return;

            var from = _torus.Wrap(ship.Pose.SamplePosition(1f, _torus));
            var delta = _torus.Delta(from, to);
            ship.Devices.Zap(device, new Vector2(from.X, from.Y), new Vector2(from.X + delta.X, from.Y + delta.Y),
                ship.Velocity);
        }

        // Legacy CreateDecoy: the flash drifts with the parent; the core's decoy event has no parent, so it drifts with
        // the decoy.
        private void AddDecoyFlash(EntityView view, Vec2 point)
        {
            var effect = _effects.Create(_prefabs.Load(FlashAdditivePrefab), point);
            if (effect != null)
            {
                effect.Size = view.SpawnFlashSize;
                effect.Color = view.Look.Color;
                effect.Run(DecoyFlashLifetime, view.Velocity, 0f);
            }

            view.SpawnFlashSize = 0f;
        }

        private float LastDegrees(EntityView view) => view.Pose.SampleAngle(1f) * Mathf.Rad2Deg;

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

            // Null for a line, and for a prefab of several renderers (a decoy, a repair bot, a segment).
            public SpriteRenderer Renderer { get; }

            // Step 7b: the root view of a decoy prefab, which takes the colour; null for a ship.
            public BaseView View { get; set; }

            // The body scale: the root's transform scale.
            public float Scale => Root.transform.localScale.x;

            // Ships and decoys (phase 5b, step 4): the look, the ship's effects and damage text, and the last
            // snapshot's velocities (Unity units; degrees per second).
            public ShipLook Look { get; set; }
            public bool IsDecoy { get; set; }
            public ShipEffects Effects { get; set; }
            public DamageText DamageText { get; set; }

            // Step 7a: the device effects (built on first use); a decoy's spawn flash size until its first pose.
            public DeviceEffects Devices { get; set; }
            public float SpawnFlashSize { get; set; }

            // Step 7c: its detonator exploded; the death shows no ship explosion, only its sound and wreck.
            public bool Detonated { get; set; }
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
            public readonly BaseView View;
            public readonly int DeviceIndex;
            public readonly Color Active, Idle;
            public bool On;
            public float Power;

            // Seconds since the last hit (legacy _timeFromLastHit, 100 at the start).
            public float HitTime = float.MaxValue;
            public ShieldView(BaseView view, int deviceIndex, Color active, Color idle)
            { View = view; DeviceIndex = deviceIndex; Active = active; Idle = idle; }
        }

        private sealed class RepairBotView : EntityView
        {
            public RepairBotView(GameObject root) : base(root, null) { }
            public bool Repairing;
            public float Jitter;
            public int Stamp;
            public SoundClip Sound;
            public int SoundId;

            // Step 7b: the Laser effect on the bot, or null.
            public IEffect Beam;
        }

        private sealed class SegmentView : EntityView
        {
            public SegmentView(GameObject root) : base(root, null) { }

            // The last capture that listed the segment.
            public int Stamp { get; set; }

            // Step 7b: the OrbAdditive of a new segment, or null.
            public IEffect SpawnEffect { get; set; }
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
        private const int TextSortingOrder = 4;
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
        // Step 8: the smoke of the asteroid, drone and obsolete ship explosions.
        private const string SmokePrefab = "Effects/Smoke";
        private const string SmokeAdditivePrefab = "Effects/SmokeAdditive";
        private const string HsvMaterialPath = "Materials/ShipHsv";

        // Step 7b: the device object prefabs, with their legacy names.
        private const string DecoyPrefab = "Objects/Decoy";
        private const string EnergyShieldPrefab = "Objects/EnergyShield";
        private const string EnergyShieldOutlinePrefab = "Objects/EnergyShieldOutline";
        private const string FrontalShieldPrefab = "Objects/FrontalShield";
        private const string RepairBotPrefab = "Objects/RepairBot";
        private const string SegmentPrefab = "Objects/WormSegment";
        private const string Segment2Prefab = "Objects/WormSegment2";
        private const string OrbAdditivePrefab = "Effects/OrbAdditive";

        // Legacy SatelliteFactory.CreateRepairBot and UnitEffect; SpaceObjectFactory.CreateSpawnEffect.
        private const float RepairBeamOffset = 0.4f;
        private const float RepairBeamFadeTime = 0.1f;
        private const float SegmentSpawnEffectSize = 2f;
        private const float SegmentSpawnEffectLifetime = 0.5f;

        // Legacy ShipWreckAction, EffectFactory.CreateDamageTextEffect and ColorTable.ShipExplosionColor.
        private const float WreckLifetime = 60f;
        private const float DamageTextLifetime = 2f;

        // Legacy SpaceObjectFactory.CreateDecoy: the spawn flash runs 0.3 s.
        private const float DecoyFlashLifetime = 0.3f;
        private static readonly Color ExplosionColor = new Color(1f, 0.66f, 0.59f);
        // Legacy ShipExplosionActionObsolete: the dark smoke.
        private static readonly Color ShipSmokeColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

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
        private readonly DeviceEffectAssets _deviceAssets;
        private long _decoySoundTick = -1;
        private SoundClip _decoySound;
        private Color[] _damageColors;
        private readonly List<IBattleEventView> _eventViews = new List<IBattleEventView>();
        private readonly Sprite _genericSprite;
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
        private readonly Dictionary<WormTailDefinition, SegmentLook> _segmentLooks = new Dictionary<WormTailDefinition, SegmentLook>();
        private readonly List<EntityId> _removedSegments = new List<EntityId>();
        private int _segmentStamp;
        private readonly Dictionary<EntityId, RepairBotView> _repairBots = new Dictionary<EntityId, RepairBotView>();
        private readonly Dictionary<RepairBotDefinition, RepairBotLook> _repairBotLooks = new Dictionary<RepairBotDefinition, RepairBotLook>();
        private readonly List<EntityId> _removedRepairBots = new List<EntityId>();
        private int _repairBotStamp;
        private readonly System.Random _repairBotRandom = new System.Random();
        private readonly List<Text> _texts = new List<Text>();
        private readonly Stack<TextMesh> _textPool = new Stack<TextMesh>();
        private readonly System.Random _textRandom = new System.Random();
    }
}
