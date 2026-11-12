using System;
using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // The sounds and effects of one ship's weapons, devices and drone bays (migration phase 5b, step 7a), as legacy
    // WeaponFactory, DeviceFactory and DroneBayFactory added them as triggers. The core's device states drive the ones
    // legacy ran on activation edges; the renderer passes the shots, launches and device events. Effects that follow
    // the ship sit under an unscaled child of the ship view, so their sizes stay in world units as legacy FlashEffect
    // set them; a weapon's flash sits on its barrel. Dispose gives every effect and sound back before the view goes.
    internal sealed class DeviceEffects
    {
        public DeviceEffects(IReadOnlyList<DeviceLook> devices, IReadOnlyList<EngineLook> engines, Transform view,
            Func<int, Transform> carrier, DeviceEffectAssets assets)
        {
            _assets = assets;
            _carrier = carrier;
            _devices = new Device[devices.Count];
            _anchor = CreateAnchor("Devices", view);

            for (var i = 0; i < devices.Count; ++i)
            {
                var look = devices[i];
                if (look == null || look.Kind == DeviceViewKind.None)
                    continue;

                var device = new Device(look);
                _devices[i] = device;
                switch (look.Kind)
                {
                    case DeviceViewKind.Weapon:
                        device.Barrel = AddBarrel(look);
                        var parent = device.Barrel?.Anchor;
                        if (look.Mode == WeaponViewMode.Charging)
                            device.Charge = CreateCharge(look, parent);
                        else
                            device.Flash = CreateFlash(look, parent, look.Lifetime);
                        break;
                    case DeviceViewKind.Accelerator:
                        // Legacy: one Flicker for 0.5 s at each engine, in the ship's frame.
                        device.Flickers = new List<Flash>();
                        foreach (var engine in engines)
                        {
                            var flicker = CreateFlash(look, _anchor, AcceleratorFlashTime);
                            if (flicker == null) continue;
                            flicker.Effect.Position = engine.Position * view.localScale.x;
                            device.Flickers.Add(flicker);
                        }
                        break;
                    case DeviceViewKind.Ghost:
                    case DeviceViewKind.InfinityStone:
                    case DeviceViewKind.GravityGenerator:
                    case DeviceViewKind.DroneBay:
                        device.Flash = CreateFlash(look, _anchor, look.Lifetime);
                        break;
                }
            }
        }

        // Once per tick, after the tick's events. faded: legacy WarpDrive's warp runs (ShipStatusFlags.Faded); position
        // and degrees: the ship's pose of the tick, for the warp effects.
        public void Capture(ReadOnlySpan<DeviceState> states, ReadOnlySpan<BarrelState> barrels, bool faded, Vec2 position,
            float degrees)
        {
            foreach (var barrel in _barrels)
            {
                if (barrel.Index >= barrels.Length) continue;
                var angle = barrels[barrel.Index].Angle;
                barrel.Previous = barrel.HasAngle ? barrel.Current : angle;
                barrel.Current = angle;
                barrel.HasAngle = true;
            }

            for (var i = 0; i < _devices.Length && i < states.Length; ++i)
            {
                var device = _devices[i];
                if (device == null)
                    continue;

                var state = states[i];
                var look = device.Look;
                if (!_captured)
                {
                    // The first tick only records the state: a device can start with a running cooldown.
                    device.WasActive = state.Active;
                    device.Cooldown = state.Cooldown;
                    device.Power = state.Charge;
                    continue;
                }

                var rising = state.Active && !device.WasActive;
                var falling = !state.Active && device.WasActive;
                switch (look.Kind)
                {
                    case DeviceViewKind.Weapon:
                        CaptureWeapon(device, state);
                        break;
                    case DeviceViewKind.Toggle:
                        if (rising) PlaySound(device);
                        break;
                    case DeviceViewKind.Accelerator:
                        if (rising) PlaySound(device);
                        if (state.Active)
                            foreach (var flicker in device.Flickers)
                                flicker.Trigger();
                        break;
                    // Legacy GhostDevice gives the sound both conditions at once, so it never plays; the view plays it
                    // on both edges, as the data asks.
                    case DeviceViewKind.Ghost:
                        if (rising || falling)
                        {
                            PlaySound(device);
                            device.Flash?.Trigger();
                        }
                        break;
                    // A use starts the cooldown, also the automatic use at a lethal hit, which leaves Active off.
                    case DeviceViewKind.InfinityStone:
                        if (state.Cooldown > device.Cooldown + CooldownStep)
                        {
                            PlaySound(device);
                            device.Flash?.Trigger();
                        }
                        break;
                    case DeviceViewKind.WarpDrive:
                        if (faded && !_faded)
                        {
                            PlaySound(device);
                            RunStatic(look, position, degrees);
                        }
                        else if (!faded && _faded)
                            RunStatic(look, position, degrees);
                        break;
                }

                device.WasActive = state.Active;
                device.Cooldown = state.Cooldown;
            }

            _faded = faded;
            _captured = true;
        }

        // A weapon's bullet spawn with no parent bullet (owner question 8 (b)): one shot per weapon per tick.
        public void Shot(int index, long tick)
        {
            var device = Get(index, DeviceViewKind.Weapon);
            if (device == null || device.LastShotTick == tick)
                return;

            device.LastShotTick = tick;
            if (device.Look.Mode == WeaponViewMode.Charging)
            {
                // Legacy ChargeableCannon: the shot sound on the discharge; its effect is the charge.
                PlaySound(device);
                return;
            }

            PlaySound(device);
            device.Flash?.Trigger();
            if (device.Look.Mode != WeaponViewMode.Single)
                device.HasBullet = true;
        }

        // Legacy DroneBayFactory: the launch effect for 0.2 s on the bay platform at the ship's centre, white, size 1.
        // Legacy ClonningDevice: the effect on the clone; the renderer places it.
        public void Launch(int index)
        {
            var device = _devices.Length > index && index >= 0 ? _devices[index] : null;
            if (device == null)
                return;

            PlaySound(device);
            device.Flash?.Trigger();
        }

        // Legacy TeleporterDevice: the effect at the start and the end, for 0.5 s each, turned with the ship.
        public void Teleport(int index, Vec2 from, Vec2 to, float degrees)
        {
            var device = Get(index, DeviceViewKind.Teleporter);
            if (device == null)
                return;

            PlaySound(device);
            RunStatic(device.Look, from, degrees);
            RunStatic(device.Look, to, degrees);
        }

        // Legacy GravityGenerator: the effect follows the ship for the device lifetime.
        public void GravityWave(int index)
        {
            var device = Get(index, DeviceViewKind.GravityGenerator);
            if (device == null)
                return;

            PlaySound(device);
            device.Flash?.Trigger();
        }

        // Legacy PointDefenseCollisionAction and PointDefenseShield: a lightning from the ship to the target for 0.1 s,
        // drifting with the ship, and the sound. False for a device that is not a point defense.
        public bool Zap(int index, Vector2 from, Vector2 to, Vector2 velocity)
        {
            var device = Get(index, DeviceViewKind.PointDefense);
            if (device == null)
                return false;

            PlaySound(device);
            var delta = to - from;
            var effect = Create(device.Look, new Vec2(from.x, from.y));
            if (effect == null)
                return true;

            effect.Color = device.Look.Color;
            effect.Rotation = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            effect.Size = delta.magnitude;
            effect.Run(device.Look.Lifetime, velocity, 0f);
            return true;
        }

        // The sound of the first clonning center: every center of a ship launches the same clone.
        public SoundClip CloneSound
        {
            get
            {
                foreach (var device in _devices)
                    if (device != null && device.Look.Kind == DeviceViewKind.ClonningCenter)
                        return device.Look.Sound;
                return default;
            }
        }

        // Once per frame, after the ship and its satellites are placed and before the effect pool ticks.
        public void Update(float deltaTime, float alpha)
        {
            foreach (var barrel in _barrels)
            {
                if (!barrel.Anchor || !barrel.HasAngle) continue;
                var angle = Mathf.LerpAngle(barrel.Previous * Mathf.Rad2Deg, barrel.Current * Mathf.Rad2Deg, alpha);
                var position = barrel.Definition.PositionAt(angle * Mathf.Deg2Rad);
                barrel.Anchor.localPosition = new Vector3(position.X, position.Y, 0f) / barrel.CarrierScale;
                barrel.Anchor.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            foreach (var device in _devices)
            {
                if (device == null) continue;
                device.Flash?.Update(deltaTime);
                if (device.Flickers != null)
                    foreach (var flicker in device.Flickers)
                        flicker.Update(deltaTime);
                if (device.Charge != null)
                    UpdateCharge(device, deltaTime);
            }
        }

        public void Dispose()
        {
            foreach (var device in _devices)
            {
                if (device == null) continue;
                device.Flash?.Effect.Dispose();
                device.Charge?.Dispose();
                if (device.Flickers != null)
                    foreach (var flicker in device.Flickers)
                        flicker.Effect.Dispose();
                _assets.Sounds.Stop(device.SoundId);
                _assets.Sounds.Stop(device.ChargeSoundId);
            }
        }

        // Held: legacy OnDeactivate when the weapon's bullet goes stops a looped shot sound. Continuous: the flash stays
        // lit while the bullet lives (OnRemainActive). Charging: the charge sound when the charge starts (OnActivate).
        private void CaptureWeapon(Device device, in DeviceState state)
        {
            var look = device.Look;
            switch (look.Mode)
            {
                case WeaponViewMode.Charging:
                    if (state.Charge > 0f && device.Power <= 0f)
                        device.ChargeSoundId = Play(look.ChargeSound, device.ChargeSoundId);
                    device.Power = state.Charge;
                    break;
                case WeaponViewMode.Held:
                case WeaponViewMode.Continuous:
                    var hasBullet = !state.ActiveBullet.IsNull;
                    if (hasBullet && look.Mode == WeaponViewMode.Continuous)
                        device.Flash?.Trigger();
                    if (!hasBullet && device.HasBullet)
                    {
                        _assets.Sounds.Stop(device.SoundId);
                        device.SoundId = 0;
                    }
                    device.HasBullet = hasBullet;
                    break;
            }
        }

        // Legacy PowerLevelEffect: the effect's life is the charge, falling no faster than 1 / lifetime per second.
        private void UpdateCharge(Device device, float deltaTime)
        {
            var effect = device.Charge;
            var life = effect.Life < device.Power ? device.Power
                : Mathf.MoveTowards(effect.Life, device.Power, deltaTime / device.Look.Lifetime);
            if (life != effect.Life)
                effect.Life = life;
        }

        // Legacy SoundEffect: a plain clip plays on every call; a loop plays until its stop condition or the disposal.
        private void PlaySound(Device device) => device.SoundId = Play(device.Look.Sound, device.SoundId);

        private int Play(SoundClip sound, int running)
        {
            if (sound.IsEmpty)
                return running;
            if (!sound.Loop)
            {
                _assets.Sounds.Play(sound);
                return running;
            }

            return running != 0 ? running : _assets.Sounds.Start(sound);
        }

        private Device Get(int index, DeviceViewKind kind) =>
            index >= 0 && index < _devices.Length && _devices[index]?.Look.Kind == kind ? _devices[index] : null;

        // Legacy StaticEffect: at a world point, turned with the ship, for its lifetime.
        private void RunStatic(DeviceLook look, Vec2 point, float degrees)
        {
            var effect = Create(look, point);
            if (effect == null)
                return;

            effect.Rotation = degrees;
            effect.Size = look.Size;
            effect.Color = look.Color;
            effect.Run(look.Lifetime, Vector2.zero, 0f);
        }

        private Flash CreateFlash(DeviceLook look, Transform parent, float lifetime)
        {
            var effect = Create(look, parent);
            if (effect == null)
                return null;

            effect.Color = look.Color;
            effect.Size = look.Size;
            return new Flash(effect, lifetime);
        }

        // Legacy PowerLevelEffect starts at life 0: a charge of 0 draws nothing.
        private IEffect CreateCharge(DeviceLook look, Transform parent)
        {
            var effect = Create(look, parent);
            if (effect == null)
                return null;

            effect.Color = look.Color;
            effect.Size = look.Size;
            effect.Life = 0f;
            return effect;
        }

        private IEffect Create(DeviceLook look, Transform parent)
        {
            if (!parent)
                return null;
            return look.Record != null ? _assets.Effects.Create(look.Record, parent)
                : _assets.Effects.Create(_assets.LoadEffect(look.Prefab), parent);
        }

        private IEffect Create(DeviceLook look, Vec2 point) => look.Record != null
            ? _assets.Effects.Create(look.Record, point)
            : _assets.Effects.Create(_assets.LoadEffect(look.Prefab), point);

        // One anchor per barrel, under its carrier (the hull or a satellite), with the carrier's scale undone.
        private Barrel AddBarrel(DeviceLook look)
        {
            if (look.BarrelDefinition == null || look.Barrel < 0 || !look.HasEffect)
                return null;

            foreach (var barrel in _barrels)
                if (barrel.Index == look.Barrel)
                    return barrel;

            var carrier = _carrier(look.BarrelDefinition.Satellite);
            if (!carrier)
                return null;

            var result = new Barrel(look.Barrel, look.BarrelDefinition, CreateAnchor("Barrel", carrier),
                carrier.localScale.x > 0f ? carrier.localScale.x : 1f);
            _barrels.Add(result);
            return result;
        }

        private static Transform CreateAnchor(string name, Transform parent)
        {
            var anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            var scale = parent.localScale.x;
            anchor.localScale = Vector3.one * (scale > 0f ? 1f / scale : 1f);
            return anchor;
        }

        // Legacy FlashEffect: one effect for the device's life; each trigger lights it at full life, and it fades out
        // over its lifetime, then hides.
        private sealed class Flash
        {
            public Flash(IEffect effect, float lifetime)
            {
                Effect = effect;
                _lifetime = lifetime > 0f ? lifetime : MinLifetime;
                Effect.Life = 0f;
                Effect.Visible = false;
            }

            public readonly IEffect Effect;

            public void Trigger()
            {
                _life = 1f;
                Effect.Visible = true;
                Effect.Life = 1f;
            }

            public void Update(float deltaTime)
            {
                if (_life <= 0f)
                    return;

                _life -= deltaTime / _lifetime;
                if (_life <= 0f)
                    Effect.Visible = false;
                else
                    Effect.Life = _life;
            }

            private const float MinLifetime = 0.01f;
            private readonly float _lifetime;
            private float _life;
        }

        private sealed class Device
        {
            public Device(DeviceLook look) => Look = look;

            public readonly DeviceLook Look;
            public Barrel Barrel;
            public Flash Flash;
            public IEffect Charge;
            public List<Flash> Flickers;
            public bool WasActive;
            public float Cooldown;
            public long LastShotTick = -1;
            public bool HasBullet;
            public float Power;
            public int SoundId;
            public int ChargeSoundId;
        }

        private sealed class Barrel
        {
            public Barrel(int index, BarrelDefinition definition, Transform anchor, float carrierScale)
            {
                Index = index;
                Definition = definition;
                Anchor = anchor;
                CarrierScale = carrierScale;
            }

            public readonly int Index;
            public readonly BarrelDefinition Definition;
            public readonly Transform Anchor;
            public readonly float CarrierScale;
            public float Previous, Current;
            public bool HasAngle;
        }

        // Legacy DeviceFactory: the accelerator's flickers live for 0.5 s after the last tick it was active.
        private const float AcceleratorFlashTime = 0.5f;

        // A cooldown that grows by more than this between two ticks started a new use.
        private const float CooldownStep = 0.001f;

        private readonly DeviceEffectAssets _assets;
        private readonly Func<int, Transform> _carrier;
        private readonly Device[] _devices;
        private readonly List<Barrel> _barrels = new List<Barrel>();
        private readonly Transform _anchor;
        private bool _faded;
        private bool _captured;
    }

    // What the device effects of every ship share.
    internal sealed class DeviceEffectAssets
    {
        public DeviceEffectAssets(EffectPool effects, PrefabPool prefabs, CombatSounds sounds)
        {
            Effects = effects;
            Prefabs = prefabs;
            Sounds = sounds;
        }

        public EffectPool Effects { get; }
        public PrefabPool Prefabs { get; }
        public CombatSounds Sounds { get; }

        // A named effect prefab of Resources/CombatView/Effects; null without a name.
        public GameObject LoadEffect(string name) => string.IsNullOrEmpty(name) ? null : Prefabs.Load(EffectFolder + name);

        private const string EffectFolder = "Effects/";
    }
}
