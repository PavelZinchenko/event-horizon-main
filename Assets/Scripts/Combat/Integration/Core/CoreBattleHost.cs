using System;
using System.Collections.Generic;
using Combat.Domain;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using CombatView;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using GameStateMachine.States;
using Services.Audio;
using Services.Localization;
using Services.Resources;
using UnityEngine;
using Zenject;
using CoreEntityId = CombatSimulation.Core.EntityId;
using CoreUnitId = CombatSimulation.Core.UnitId;
using LegacyViewRect = Combat.Scene.ViewRect;
using PlayerInputSignal = Combat.Ai.PlayerInputSignal;

namespace Combat.Integration.Core
{
    // The mode-neutral part of a scene on the shared combat core (exploration plan, E1): the step loop, the renderer,
    // entity IDs, ship views, controls, sounds, broadcasts, camera, planet and asteroids. A mode adapter on top of it owns
    // the units and the rules: it builds the battle, then calls Initialize.
    // The BattleRunner is the authority: commands go to it, and the projection only copies its snapshots.
    // Steps the battle at the core's fixed rate and handles each tick's events right after its step, so no event is lost.
    // Online duel (phase 7, step 6): with a CoreDuelBattle, the session thread is the authority, and every tick's snapshot
    // copy goes through the same handling, oldest first.
    public abstract class CoreBattleHost : IShipViews, IShipControlSource, ITickable, IDisposable
    {
        protected CoreBattleHost(
            IDatabase database,
            IResourceLocator resourceLocator,
            ExitSignal.Trigger exitTrigger,
            ILocalization localization,
            PlayerInputSignal playerInputSignal,
            ISoundPlayer soundPlayer,
            UnityEngine.SceneManagement.Scene scene)
        {
            _database = database;
            _resourceLocator = resourceLocator;
            _exitTrigger = exitTrigger;
            _localization = localization;
            _playerInputSignal = playerInputSignal;
            _soundPlayer = soundPlayer;
            _scene = scene;
        }

        // Called once by the mode adapter's constructor, after it has mapped its ships: the mapper's looks are read here.
        // A local battle gives the runner and the trees; a duel gives neither.
        protected void Initialize(CoreShipMapper mapper, CorePilotProvider pilots, BattleRunner runner,
            BehaviorTreeFactory trees, CoreDuelBattle duel, ulong seed, Torus torus,
            IReadOnlyList<AsteroidSetup> asteroids, bool planet, CombatViewSettings viewSettings)
        {
            _mapper = mapper;
            _pilots = pilots;
            _runner = runner;
            _trees = trees;
            _duel = duel;
            LocalSide = duel?.LocalSide ?? PlayerSide;

            var resourceLocator = _resourceLocator;
            var bulletSprite = resourceLocator.GetSprite(new SpriteId(BulletSpriteName, SpriteId.Type.Ammunition));
            // Only the font of the legacy text effect prefab is used; the text view is built in code (step 7f).
            var textFont = Resources.Load<TextMesh>(TextEffectPrefab)?.font;
            var sounds = new CoreSoundOutput(_soundPlayer, resourceLocator);
            _sounds = sounds;
            _renderer = new BattleRenderer(torus, bulletSprite, textFont, _scene, viewSettings, sounds)
            {
                ViewerSide = LocalSide,
                SmoothCorrections = duel != null,
                // Phase 5b, step 4: the legacy combat Settings clips (a loop, as legacy SoundLoopEffect plays it) and the
                // satellite's hard-coded explosion. Step 8: the asteroid's.
                RetreatSound = new SoundClip(resourceLocator.GetAudioClip(new AudioClipId(RetreatSoundName)), true),
                WarpSound = sounds.Resolve(new AudioClipId(WarpSoundName)),
                DroneExplosionSound = sounds.Resolve(new AudioClipId(DroneExplosionSoundName)),
                AsteroidExplosionSound = sounds.Resolve(new AudioClipId(AsteroidExplosionSoundName)),
            };
            // Legacy DamageIndicator colours: the shield, then the LegacyDamageModel types.
            _renderer.SetDamageTextColors(new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 1f, 0.5f), new Color(0f, 1f, 1f),
                new Color(1f, 1f, 0f), new Color(1f, 0.5f, 1f));
            _visualEffects = new CoreVisualEffects(resourceLocator);
            RegisterMappedLooks();
            _deviceLooks = new CoreDeviceLooks(_visualEffects, sounds);
            _bulletVisuals = new CoreBulletVisuals(_mapper.BulletSources, _mapper.SpawnTriggers, resourceLocator, _visualEffects,
                sounds);
#if UNITY_EDITOR
            // Phase 5b, step 3: the owner reviews the effects in the Combat > Effect Preview window. Not in a duel: it maps
            // ships while the session thread reads the mapper.
            if (duel == null)
                _effectPreview = new CoreEffectPreview(_renderer, _visualEffects, sounds, _database, resourceLocator, () =>
                _ids.TryGetCore(PlayerShip, out var core) ? core : default);
#endif
            _anchor = new Vector2(0.5f * torus.Width, 0.5f * torus.Height);
            // Step 10: the legacy planet, presentation only (D8).
            if (planet)
                _planet = CorePlanet.Create(seed, torus, _scene);
            if (asteroids != null)
                _asteroids = CoreAsteroids.Create(torus, _scene);

            _playerInputSignal.Event += OnPlayerInput;
        }

        #region IShipViews

        // Exploration plan, E2: each new ship view, in the tick's event handling. A view that vanishes and returns is a
        // new view with a new ID.
        public event Action<EntityId, CombatSide> ShipAdded;

        public bool TryGetShip(EntityId id, out IShipView ship)
        {
            var found = _views.TryGetValue(id, out var view);
            ship = view;
            return found;
        }

        // Camera framing by the legacy rule: the player ship and the target with margins, zoomed in on each new
        // player ship. Drawn positions are continuous across the wrap, so the camera never jumps at the map edge.
        // Phase 5b, step 3: the effects' camera shake moves the rect, as legacy Scene.ViewRect.
        public Rect CameraRect
        {
            get
            {
                var rect = _framing.Rect;
                rect.position += _renderer.CameraOffset;
                return rect;
            }
        }

        public Vector2 ViewPoint => _framing.Center;

        #endregion

        #region IShipControlSource

        // Only player ships have controls. A redeployed ship has a new pilot and a new description.
        public bool TryGetControls(EntityId ship, out IShipControlDescription description, out IShipCommandSink commands)
        {
            description = null;
            commands = null;
            if (!IsStepping || !_ids.TryGetCore(ship, out var core) || !_viewsByCore.TryGetValue(core, out var view))
                return false;
            if (!_pilots.TryGetPlayerPilot(core, out var pilot))
                return false;

            description = view.Controls;
            commands = pilot;
            return true;
        }

        #endregion

        public void Tick()
        {
            if (_runner == null && _duel == null)
                return;

            if (_duel != null)
                RunDuel();
            else
                RunSteps();
            _playerInput = false;
            Render();
            UpdateExit();
        }

        // Scene unload. Clears the battle without events: the outcome is already final or abandoned.
        public void Dispose()
        {
            _playerInputSignal.Event -= OnPlayerInput;
            _runner = null;
            // The duel state owns the session thread and stops it.
            _duel = null;
#if UNITY_EDITOR
            _effectPreview?.Dispose();
#endif
            _renderer.Dispose();
            _planet?.Dispose();
            _asteroids?.Dispose();
            DisposeMode();
            _views.Clear();
            _viewsByCore.Clear();
            _ids.Clear();
            _pilots.Clear();
            _broadcasts.Clear();
        }

        // The mode's view of the player ship and the camera: the controlled ship, the framed target (None for no target),
        // and whether the player stays in the middle.
        protected abstract EntityId PlayerShip { get; }
        protected abstract EntityId CameraTarget { get; }
        protected abstract bool CameraCentersPlayer { get; }

        // Before each local step while the battle runs: the mode's own commands.
        protected virtual void BeforeStep() { }

        // Each duel frame after the copies, while the duel runs and its link holds.
        protected virtual void OnDuelFrame() { }

        // Each tick's snapshot, before its events.
        protected virtual void BeginSnapshot(BattleSnapshot snapshot) { }

        // A ship spawn: the mode names its side and specification; false draws no view.
        protected abstract bool TryDescribeShip(CoreUnitId unit, out CombatSide side,
            out Constructor.IShipSpecification specification);

        // A ship's view went inactive; the host releases its pilot and removes it from the renderer next.
        protected virtual void OnShipRemoved(EntityId ship) { }

        protected virtual void OnUnitTransition(BattleSnapshot snapshot, in BattleEvent item) { }

        // Each tick's snapshot, after its events and the views' update.
        protected virtual void EndSnapshot(BattleSnapshot snapshot) { }

        // An event that the battle mode published (BattleEventKind.Mode), in the tick's event order.
        protected virtual void OnModeEvent(BattleSnapshot snapshot, in ModeEvent item) { }

        // Once per frame after the renderer, the planet and the asteroids: the mode's own presentation (exploration
        // objects). Anchor: the drawn controlled ship, or its last position.
        protected virtual void RenderMode(Vector2 anchor, float deltaTime) { }

        // In Dispose, after the host's views: the mode's own presentation.
        protected virtual void DisposeMode() { }

        // The first completed snapshot, and every one after it while the battle steps.
        protected abstract void OnCompleted(BattleResult result);

        // Null after disposal and in a duel.
        protected BattleRunner Runner => _runner;
        // Null after disposal and in a local battle.
        protected CoreDuelBattle Duel => _duel;
        protected int LocalSide { get; private set; }
        // The last handled snapshot's tick, in both modes.
        protected long LastTick => _lastTick;

        // False after the result and after disposal: no deployment, mode command or timer.
        protected bool IsRunning => _duel != null ? !_duelCompleted : _runner != null && !_runner.IsCompleted;

        // The battle steps and the player flies. A local battle also steps after its result (phase 2b, D12); a duel
        // stops at its result, and the session thread then publishes nothing more.
        protected bool IsStepping => _duel != null ? !_duelCompleted : _runner != null && !_runner.IsStopped;

        // The tick that the next command targets: the runner's, or in a duel the one after the last handled snapshot.
        protected long CommandTick => _runner?.Tick ?? _lastTick + 1;

        protected bool IsExitScheduled => _exitDelay >= 0f;

        // A local battle counts game time, as legacy CombatManager: a pause stops the battle and the delay together. A duel
        // counts real time; its session thread does not pause. A delay of 0 exits in this frame, paused or not.
        protected void ScheduleExit(float seconds) => _exitDelay = seconds;

        protected void ZoomCamera() => _framing.Zoom();

        protected bool TryGetCore(EntityId ship, out CoreEntityId core) => _ids.TryGetCore(ship, out core);
        protected bool TryGetEntity(CoreEntityId core, out EntityId ship) => _ids.TryGet(core, out ship);

        // M1 check: the console lists the inert devices (D1) once per battle, by mapped ship or drone.
        protected void LogInertDevices(CoreShipMapper mapper)
        {
            var report = new System.Text.StringBuilder();
            var count = 0;
            foreach (var mapping in mapper.Mappings.Values)
            {
                for (var i = 0; i < mapping.Devices.Count; ++i)
                {
                    var device = mapping.Devices[i];
                    if (!device.IsInert) continue;
                    report.AppendLine($"  {mapping.Name}, device {i}: {device.Name}: {device.InertReason}");
                    ++count;
                }
            }

            if (count > 0)
                Debug.Log($"{GetType().Name}: {count} inert devices (phase 5 or step 4).\n{report}");
        }

        private void RunSteps()
        {
            if (!IsStepping)
                return;

            _accumulator += Time.deltaTime;
            var steps = 0;
            while (_accumulator >= Battle.TickDuration && IsStepping)
            {
                if (steps == MaxStepsPerFrame)
                {
                    // A long frame drops the rest instead of catching up later.
                    _accumulator = 0f;
                    break;
                }

                _accumulator -= Battle.TickDuration;
                Step();
                steps++;
            }
        }

        // Every snapshot copy the session thread published since the last frame, oldest first. After the result the thread
        // only pumps the network and publishes nothing more.
        private void RunDuel()
        {
            var thread = _duel.Thread;
            // Read before the copies: a thread that stopped has published all of its copies by then.
            var linkLoss = thread.Status.LinkLoss;
            var error = thread.Error;
            if (!_threadErrorLogged && error != null)
            {
                _threadErrorLogged = true;
                Debug.LogError($"{GetType().Name}: the duel session thread stopped at tick {thread.Status.Tick}: {error}");
            }

            while (thread.TryTake(out var snapshot, out var publishedAt))
            {
                if (IsRunning)
                {
                    _duelCompleted = snapshot.IsCompleted;
                    _publishedAt = publishedAt;
                    Process(snapshot);
                }
                thread.Release(snapshot);
            }

            if (!IsRunning)
                return;

            if (linkLoss != SessionLinkLoss.None || error != null)
            {
                EndDuelWithoutResult(linkLoss);
                return;
            }

            OnDuelFrame();
            if (_playerInput)
                _duel.MarkPlayerInput();
        }

        // Also after the stepping stops (a duel's result, disposal), so the last frame stays on screen.
        private void Render()
        {
            var controlled = default(CoreEntityId);
            var player = ActiveView(PlayerShip);
            if (player != null)
                _ids.TryGetCore(player.Id, out controlled);

            var alpha = !IsStepping ? 1f : _duel != null
                ? Mathf.Clamp01((float)(_duel.Now() - _publishedAt) / Battle.TickDuration)
                : Mathf.Clamp01(_accumulator / Battle.TickDuration);
            _renderer.ViewPoint = _framing.Center;
            _renderer.Render(alpha, controlled, ref _anchor, Time.deltaTime);
            _planet?.Render(_anchor, Time.deltaTime);
            _asteroids?.Render(alpha, _anchor, Time.deltaTime);
            RenderMode(_anchor, Time.deltaTime);

            foreach (var pair in _viewsByCore)
                if (_renderer.TryGetShipPose(pair.Key, out var position, out var degrees))
                    pair.Value.SetVisualPose(position, degrees);

            var target = ActiveView(CameraTarget);
            _framing.Update(player?.VisualPosition, target?.VisualPosition, CameraCentersPlayer);
        }

        private CoreShipView ActiveView(EntityId id) => _views.TryGetValue(id, out var view) && view.IsActive ? view : null;

        // One tick: the mode's commands, step, then handle the tick's events before the next step.
        protected void Step()
        {
            if (!IsStepping)
                return;

            // After the result the mode submits nothing.
            if (IsRunning)
                BeforeStep();

            // The pilots of this step think on the snapshot of the current tick.
            if (_playerInput)
                _trees.OnPlayerInput(_runner.Tick);

            _runner.Step();
            Process(_runner.Snapshot);
        }

        // One tick's snapshot: the runner's after its step, or a copy from the session thread.
        private void Process(BattleSnapshot snapshot)
        {
            _lastTick = snapshot.Tick;
            BeginSnapshot(snapshot);
            HandleEvents(snapshot);
            _renderer.Capture(snapshot);
            _asteroids?.Capture(snapshot);
            ShowBroadcasts(snapshot);
            UpdateProjection(snapshot);
            EndSnapshot(snapshot);

            if (snapshot.IsCompleted)
                OnCompleted(snapshot.Result);
        }

        private void HandleEvents(BattleSnapshot snapshot)
        {
            var events = snapshot.Events;
            for (var i = 0; i < events.Length; ++i)
            {
                var item = events[i];
                switch (item.Kind)
                {
                    case BattleEventKind.Spawn:
                        if (item.IsBullet)
                            OnBulletSpawned(item);
                        else if (item.IsDrone)
                            OnDroneSpawned(item);
                        else if (item.IsDecoy)
                            OnDecoySpawned(item);
                        else
                            OnShipSpawned(snapshot, item);
                        break;
                    case BattleEventKind.Removal:
                        if (item.IsBullet)
                            _renderer.RemoveBullet(item.EntityId, item.RemovalConditions);
                        else if (item.IsDrone || item.IsDecoy)
                            _renderer.RemoveShip(item.EntityId, item.RemovalReason);
                        else
                            RemoveShip(item, ViewReason(events, i));
                        break;
                    case BattleEventKind.UnitTransition:
                        OnUnitTransition(snapshot, item);
                        break;
                    case BattleEventKind.Rename:
                        _renderer.RenameBullet(item.PreviousId, item.EntityId);
                        break;
                    case BattleEventKind.Hit:
                        _renderer.AddHit(item);
                        break;
                    case BattleEventKind.Disarm:
                        _renderer.AddDisarm(item);
                        break;
                    case BattleEventKind.Teleport:
                        _renderer.AddTeleport(item);
                        break;
                    case BattleEventKind.GravityWave:
                        _renderer.AddGravityWave(item);
                        break;
                    case BattleEventKind.AsteroidRemoval:
                        OnAsteroidRemoved(item);
                        break;
                    case BattleEventKind.Damage:
                        _renderer.AddDamage(item);
                        break;
                    case BattleEventKind.AuxiliaryRemoval:
                        _renderer.RemoveAuxiliary(item.EntityId, item.Point, item.RemovalReason);
                        break;
                    case BattleEventKind.SatelliteChange:
                        _renderer.ChangeSatellite(item);
                        break;
                    case BattleEventKind.Mode:
                        OnModeEvent(snapshot, item.ModeEvent);
                        break;
                }

                _renderer.OnEvent(snapshot, item);
            }
        }

        private void OnShipSpawned(BattleSnapshot snapshot, in BattleEvent item)
        {
            if (!TryDescribeShip(item.UnitId, out var side, out var specification) ||
                !snapshot.TryGetShip(item.EntityId, out var state))
                return;

            var id = _ids.Assign(item.EntityId);
            var view = new CoreShipView(id, side, specification, state, new CoreShipControls(_mapper.Mappings[state.Definition]));
            _views.Add(id, view);
            _viewsByCore.Add(item.EntityId, view);

            RegisterMappedLooks();
            var mapping = _mapper.Mappings[state.Definition];
            _renderer.AddShip(item.EntityId, GetLook(mapping), item.Side);
            AddTurrets(item.EntityId, mapping);
            ShipAdded?.Invoke(id, side);
        }

        // The looks of the mapper's worm tails and repair bots. Exploration plan, step 8: the exploration content maps a
        // ship when it first spawns, so each ship spawn registers the looks that the mapper added since. The mapper's
        // dictionaries only grow.
        private void RegisterMappedLooks()
        {
            if (_mapper.SegmentSources.Count != _segmentLookCount)
            {
                _segmentLookCount = _mapper.SegmentSources.Count;
                foreach (var source in _mapper.SegmentSources)
                    _renderer.AddSegmentLook(source.Key, SegmentLook(source.Value, _resourceLocator));
            }

            if (_mapper.RepairBotColors.Count != _repairBotLookCount)
            {
                _repairBotLookCount = _mapper.RepairBotColors.Count;
                foreach (var look in _mapper.RepairBotColors)
                    _renderer.AddRepairBotLook(look.Key, new RepairBotLook(look.Value.ToUnity(),
                        RepairBeam(_mapper.RepairBotEffects.TryGetValue(look.Key, out var effect) ? effect : null),
                        _mapper.RepairBotSounds.TryGetValue(look.Key, out var sound) ? _sounds.Resolve(sound) : default));
            }
        }

        // The view of every ship of one definition (phase 5b, step 4), built on first use.
        private ShipLook GetLook(CoreShipMapping mapping)
        {
            if (_looks.TryGetValue(mapping.Definition, out var look))
                return look;

            look = new ShipLook(_resourceLocator.GetSprite(mapping.ModelImage), mapping.ModelScale)
                { Tint = mapping.TintColor?.ToUnity() };
            var source = mapping.Look;
            if (source != null)
            {
                var engines = new EngineLook[source.Engines.Count];
                for (var i = 0; i < engines.Length; ++i)
                    engines[i] = new EngineLook(source.Engines[i].Position.ToUnity() * 0.5f, source.Engines[i].Size);

                look.Color = source.ShipColor.Color.ToUnity();
                look.IsHsv = source.ShipColor.IsHsv;
                look.Hue = source.ShipColor.Hue;
                look.Saturation = source.ShipColor.Saturation;
                look.IsDrone = source.IsDrone;
                look.EngineColor = source.EngineColor.ToUnity();
                look.Engines = engines;
                look.HasShield = source.HasShield;
                look.Explosion = _visualEffects.Get(source.Explosion);
                look.ExplosionSound = _sounds.Resolve(source.ExplosionSound);
                look.LeavesWreck = source.LeavesWreck;
                look.HasShadow = source.HasShadow;
            }

            // Phase 5b, step 7a: the sounds and effects of the weapons, devices and drone bays.
            look.Devices = _deviceLooks.Create(mapping);

            _looks.Add(mapping.Definition, look);
            return look;
        }

        // Phase 5b, step 7a (owner question 8 (b)): a bullet of a weapon's own definition is its shot. A charged copy has
        // the weapon's definition as its uncharged one; child bullets have definitions of their own.
        private void OnBulletSpawned(in BattleEvent item)
        {
            _renderer.AddBullet(item, _bulletVisuals.Get(item));
            object key = item.IsBeam ? item.BeamDefinition.Uncharged : (object)item.BulletDefinition.Uncharged;
            if (_mapper.WeaponDevices.TryGetValue(key, out var device))
                _renderer.AddShot(item.ParentId, device, item.Tick);
        }

        // Legacy Decoy prefab at the body scale, in the device colour (phase 5, step 10c; phase 5b, step 7b). Step 7a:
        // legacy CreateDecoy's flash at 4 x the decoy size, and the device sound.
        private void OnDecoySpawned(in BattleEvent item)
        {
            var color = _mapper.DecoyColors.TryGetValue(item.Definition, out var data) ? data.ToUnity() : Color.white;
            var sound = _mapper.DecoySounds.TryGetValue(item.Definition, out var clip) ? _sounds.Resolve(clip) : default;
            _renderer.AddDecoy(item.EntityId, DecoyDefinition.Size, color, sound, 4f * DecoyDefinition.Size, item.Tick);
        }

        // Phase 5b, step 8: legacy AsteroidExplosionAction.
        private void OnAsteroidRemoved(in BattleEvent item)
        {
            if (_asteroids != null && _asteroids.Remove(item.EntityId, out var size))
                _renderer.AddAsteroidExplosion(item.Point, size);
        }

        // Phase 5b, step 7a: the launch of a drone bay or a clonning center.
        private void OnDroneSpawned(in BattleEvent item)
        {
            var mapping = _mapper.Mappings[item.Definition];
            _renderer.AddShip(item.EntityId, GetLook(mapping), item.Side);
            AddTurrets(item.EntityId, mapping);
            if (item.BayKey.HasValue)
                _renderer.AddLaunch(item.ParentId, item.BayKey.Value.DeviceIndex, item.EntityId);
        }

        // Satellites first: a turret on a satellite rides its view (phase 5, step 12c).
        private void AddTurrets(CoreEntityId id, CoreShipMapping mapping)
        {
            foreach (var satellite in mapping.Satellites)
                _renderer.AddSatellite(id, _resourceLocator.GetSprite(satellite.Image), satellite.Scale);
            // Legacy ShipFactory.CreatePlatform applies the turret scheme's HSV to every turret.
            var turretColor = mapping.Look?.TurretColor ?? default;
            foreach (var turret in mapping.Turrets)
                _renderer.AddTurret(id, turret.Barrel, mapping.Definition.Barrels[turret.Barrel],
                    _resourceLocator.GetSprite(turret.Image), turret.Size, turret.Color.ToUnity(), turretColor.Hue,
                    turretColor.Saturation);

            // Energy and frontal shields (phase 5, step 10d).
            var devices = mapping.Definition.Devices;
            for (var i = 0; i < devices.Count; ++i)
                if (devices[i] is ShieldDefinition shield)
                    _renderer.AddShield(id, i, GetShieldLook(shield));
        }

        // Phase 5b, step 7b, legacy CustomPrefabLoader: a CircularSpriteObject record sets the EnergyShield prefab's sprite,
        // a CircularOutlineObject record builds the EnergyShieldOutline ring. Step 7c: a ColliderSpriteObject record sets
        // the FrontalShield prefab's sprite.
        private ShieldLook GetShieldLook(ShieldDefinition shield)
        {
            if (_shieldLooks.TryGetValue(shield, out var look) || !_mapper.ShieldLooks.TryGetValue(shield, out var source))
                return look;

            look = new ShieldLook
            {
                Kind = source.IsFrontal ? ShieldViewKind.Frontal : ShieldViewKind.Energy,
                Offset = new Vector2(source.OffsetX, source.OffsetY),
                Size = source.Size,
                Color = source.Color.ToUnity(),
                IdleOpacity = source.IdleOpacity,
            };
            switch (source.Prefab)
            {
                case GameObjectPrefab_CircularSpriteObject sprite:
                    look.Image = _resourceLocator.GetSprite(sprite.Image);
                    break;
                case GameObjectPrefab_CircularOutlineObject outline:
                    look.Kind = ShieldViewKind.EnergyOutline;
                    look.Image = _resourceLocator.GetSprite(outline.Image);
                    look.ImageScale = outline.ImageScale;
                    look.Thickness = outline.Thickness;
                    look.AspectRatio = outline.AspectRatio;
                    break;
                case GameObjectPrefab_ColliderSpriteObject frontal:
                    look.Image = _resourceLocator.GetSprite(frontal.Image);
                    break;
            }

            _shieldLooks.Add(shield, look);
            return look;
        }

        // Phase 5b, step 7b, legacy WormTailSegmentInitializer: a database record sets the WormSegment prefab's sprites and
        // its joint.
        private static SegmentLook SegmentLook(in CoreSegmentSource source, IResourceLocator resourceLocator)
        {
            var record = source.Record;
            return new SegmentLook
            {
                IsSegment2 = source.IsSegment2,
                HasRecord = record != null,
                Body = record != null ? resourceLocator.GetSprite(record.BodyImage) : null,
                Joint = record != null ? resourceLocator.GetSprite(record.JointImage) : null,
                JointScale = record?.JointImageScale ?? 0f,
                JointOffset = record?.JointImageOffset ?? 0f,
            };
        }

        // Phase 5b, step 7c: the first Laser element of the repair bot's record. A device without a record (a mod) gets
        // the legacy Laser effect prefab: its texture, 1 wide, in the device colour; a record without a Laser element
        // draws no beam.
        private CombatView.Effects.EffectElement RepairBeam(VisualEffect effect)
        {
            if (effect == null)
                return new CombatView.Effects.EffectElement(CombatView.Effects.EffectKind.Laser,
                    _resourceLocator.GetSprite(new SpriteId(RepairBeamImage, SpriteId.Type.Effect)),
                    CombatView.Effects.ColorMode.TakeFromOwner, Color.white, 1, 1f, 0f, 0f, 0f, 1f, 1f, Vector2.zero, 0f,
                    loop: false, inverse: false, useRealTime: false);

            foreach (var element in effect.Elements)
                if (element.Type == VisualEffectType.Laser)
                    return _visualEffects.ToElement(element, CombatView.Effects.EffectKind.Laser);
            return null;
        }

        // The unit transition of the same removal comes later in the batch, so the ID stays mapped until the tick ends.
        // Legacy ShipInfo.Destroy: surrender and kill-all make the ship vanish, without an explosion or a sound. The core
        // removes it with Death and names the ForcedDestruction in its unit's transition later in the same batch.
        private static RemovalReason ViewReason(ReadOnlySpan<BattleEvent> events, int index)
        {
            var removal = events[index];
            if (removal.RemovalReason != RemovalReason.Death)
                return removal.RemovalReason;

            for (var i = index + 1; i < events.Length; ++i)
                if (events[i].Kind == BattleEventKind.UnitTransition && events[i].EntityId == removal.EntityId)
                    return events[i].TransitionReason == UnitTransitionReason.ForcedDestruction
                        ? RemovalReason.Vanish : removal.RemovalReason;
            return removal.RemovalReason;
        }

        private void RemoveShip(in BattleEvent item, RemovalReason viewReason)
        {
            if (_viewsByCore.TryGetValue(item.EntityId, out var view))
            {
                view.Deactivate();
                OnShipRemoved(view.Id);
            }

            _pilots.Release(item.EntityId);
            _renderer.RemoveShip(item.EntityId, viewReason);
            _removed.Add(item.EntityId);
        }

        // Copies what the views need from the snapshot; spans are valid only until the next step.
        private void UpdateProjection(BattleSnapshot snapshot)
        {
            foreach (var ship in snapshot.Ships)
                if (_viewsByCore.TryGetValue(ship.Id, out var view))
                    view.Update(ship, snapshot.GetDevices(ship));

            foreach (var core in _removed)
            {
                if (_viewsByCore.TryGetValue(core, out var view))
                {
                    _viewsByCore.Remove(core);
                    _views.Remove(view.Id);
                }

                _ids.Release(core);
            }

            _removed.Clear();
        }

        // Phase 7, step 8: the session thread stopped before the result. A lost link is a draw on this player (decision 3);
        // a thread error (linkLoss None) ends without a result. The battle freezes, then exits as after a result.
        private void EndDuelWithoutResult(SessionLinkLoss linkLoss)
        {
            _duelCompleted = true;
            _duel.LinkLoss = linkLoss;
            Debug.LogWarning($"{GetType().Name}: duel ended without a result ({linkLoss}), local side {LocalSide}, tick {_lastTick}");
            if (_exitDelay < 0f)
                _exitDelay = ExitDelaySeconds;
        }

        private void UpdateExit()
        {
            if (_exitDelay < 0f || _exited)
                return;

            _exitDelay -= _duel != null ? Time.unscaledDeltaTime : Time.deltaTime;
            if (_exitDelay > 0f)
                return;

            _exited = true;
            _exitTrigger.Fire();
        }

        // Legacy PlayerInputSignal: the keyboard driver and the HUD fire it in every frame with player input (step 7f).
        // The flag holds until the end of the frame's steps.
        private void OnPlayerInput() => _playerInput = true;

        // ShowMessage (step 7e): called inside the step, while the pilots think. Local only (D6).
        protected void OnBroadcast(CoreEntityId ship, string text, ColorData color) =>
            _broadcasts.Add(new Broadcast(ship, text, color));

        // Legacy RadioTransmitter: the localized text at the ship, drifting with it. Legacy NodeBuilder localizes at
        // build time; the text is the same.
        private void ShowBroadcasts(BattleSnapshot snapshot)
        {
            _duel?.TakeBroadcasts(snapshot.Tick, OnBroadcast);
            foreach (var item in _broadcasts)
                if (snapshot.TryGetShip(item.Ship, out var state))
                    _renderer.AddText(_localization.Localize(item.Text), item.Color.ToUnity(), state.Position, state.LinearVelocity);

            _broadcasts.Clear();
        }

        private readonly struct Broadcast
        {
            public Broadcast(CoreEntityId ship, string text, ColorData color)
            {
                Ship = ship;
                Text = text;
                Color = color;
            }

            public readonly CoreEntityId Ship;
            public readonly string Text;
            public readonly ColorData Color;
        }

        protected const int PlayerSide = 0;
        protected const int EnemySide = 1;
        protected const int TicksPerSecond = 50;
        protected const float ExitDelaySeconds = 3f;
        private const int MaxStepsPerFrame = 5;
        private const string BulletSpriteName = "ball";
        // Legacy ShowMessage effect (RadioTransmitter).
        private const string TextEffectPrefab = "Combat/Effects/MessageText";
        // Legacy combat Settings component (CombatScene): ShipRetreatSound and ShipWarpSound. Legacy
        // DroneExplosionActionObsolete of a satellite, a decoy, a repair bot or a tail segment; AsteroidExplosionAction.
        private const string RetreatSoundName = "flux_02";
        private const string WarpSoundName = "shot_06";
        private const string DroneExplosionSoundName = "explosion_10";
        private const string AsteroidExplosionSoundName = "explosion_08";
        // The texture of the legacy Laser effect prefab (Textures/Bullets/laser; the same image is Textures/Effects/laser).
        private const string RepairBeamImage = "laser";
        private readonly Dictionary<ShieldDefinition, ShieldLook> _shieldLooks = new Dictionary<ShieldDefinition, ShieldLook>();

        private BattleRunner _runner;
        private CoreDuelBattle _duel;
        // The last handled snapshot's tick, in both modes; the duel's result flag and the copy's publish time.
        private long _lastTick = -1;
        private bool _duelCompleted;
        private double _publishedAt;
        private bool _threadErrorLogged;
        private BehaviorTreeFactory _trees;
        private CorePilotProvider _pilots;
        private readonly List<Broadcast> _broadcasts = new List<Broadcast>();
        private bool _playerInput;
        private CoreShipMapper _mapper;
        // The drawn controlled ship, or its last position: every other entity is drawn at its image nearest to it.
        private Vector2 _anchor;
        private float _accumulator;
        // Seconds left until the exit after the result; negative while the battle runs.
        private float _exitDelay = -1f;
        private bool _exited;

        private readonly Dictionary<EntityId, CoreShipView> _views = new Dictionary<EntityId, CoreShipView>();
        private readonly Dictionary<CoreEntityId, CoreShipView> _viewsByCore = new Dictionary<CoreEntityId, CoreShipView>();
        private readonly List<CoreEntityId> _removed = new List<CoreEntityId>();
        private readonly EntityIdMap _ids = new EntityIdMap();
        private BattleRenderer _renderer;
        private CoreBulletVisuals _bulletVisuals;
        private CoreVisualEffects _visualEffects;
        private CoreDeviceLooks _deviceLooks;
        private CoreSoundOutput _sounds;
        private readonly Dictionary<ShipDefinition, ShipLook> _looks = new Dictionary<ShipDefinition, ShipLook>();
#if UNITY_EDITOR
        private CoreEffectPreview _effectPreview;
#endif
        private CorePlanet _planet;
        private CoreAsteroids _asteroids;
        private int _segmentLookCount;
        private int _repairBotLookCount;
        private readonly LegacyViewRect _framing = new LegacyViewRect();

        private readonly IDatabase _database;
        private readonly IResourceLocator _resourceLocator;
        private readonly ExitSignal.Trigger _exitTrigger;
        private readonly ILocalization _localization;
        private readonly PlayerInputSignal _playerInputSignal;
        private readonly ISoundPlayer _soundPlayer;
        private readonly UnityEngine.SceneManagement.Scene _scene;
    }
}
