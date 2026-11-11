#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulation.Physics;
using CombatView;
using CombatView.Effects;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Services.Resources;
using UnityEngine;
using CoreEntityId = CombatSimulation.Core.EntityId;

namespace Combat.Integration.Core
{
    // The owner's review of the step 3 effects before they have users (migration phase 5b, step 3). Editor only: the
    // window Combat > Effect Preview plays them in front of the player ship of a running core battle. It lists one
    // single-element record per element type (the first built-in element of the type), every built-in VisualEffect
    // record, the named effect prefabs and the built-in sounds.
    public sealed class CoreEffectPreview
    {
        public static CoreEffectPreview Current { get; private set; }

        public CoreEffectPreview(BattleRenderer renderer, CoreVisualEffects effects, CoreSoundOutput sounds,
            IDatabase database, IResourceLocator resourceLocator, Func<CoreEntityId> controlledShip)
        {
            _renderer = renderer;
            _soundOutput = sounds;
            _controlledShip = controlledShip;

            var records = database.VisualEffectList.OrderBy(item => item.Id.Value).ToList();
            foreach (var kind in (EffectKind[])Enum.GetValues(typeof(EffectKind)))
                Types.Add(CreateTypeItem(kind, records, effects, resourceLocator));
            foreach (var record in records)
                Records.Add(new Item($"{record.Id.Value}: {Describe(record)}", effects.Get(record), null));
            foreach (var name in NamedPrefabs)
                Prefabs.Add(new Item(name, null, name));
            Sounds.AddRange(CollectSounds(database));

            Current = this;
        }

        public readonly List<Item> Types = new List<Item>();
        public readonly List<Item> Records = new List<Item>();
        public readonly List<Item> Prefabs = new List<Item>();

        // Clip names as the database writes them: a '*' loops the clip.
        public readonly List<string> Sounds = new List<string>();

        // The size, rotation (degrees from the ship's heading) and distance ahead of the ship. Seconds 0 plays a record
        // for its whole lifetime and a prefab for 1 s. Without SetColor the elements keep their own colours.
        public float Size = 1f;
        public float Rotation;
        public float Distance = 5f;
        public float Seconds;
        public bool SetColor;
        public Color Color = Color.white;
        public bool OnShip;

        public bool HasShip => _renderer.TryGetShipPose(_controlledShip(), out _, out _);

        public void Play(Item item)
        {
            var ship = _controlledShip();
            if (!_renderer.TryGetShipPose(ship, out var position, out var degrees))
                return;

            IEffect effect;
            if (OnShip)
            {
                effect = item.Record != null ? _renderer.AddShipEffect(ship, item.Record) : _renderer.AddShipEffect(ship, item.Prefab);
                if (effect == null) return;
                effect.Position = new Vector2(Distance, 0f);
                effect.Rotation = Rotation;
            }
            else
            {
                var point = position + Distance * new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
                var world = new Vec2(point.x, point.y);
                effect = item.Record != null ? _renderer.AddEffect(item.Record, world) : _renderer.AddEffect(item.Prefab, world);
                if (effect == null) return;
                effect.Rotation = degrees + Rotation;
            }

            effect.Size = Size;
            if (SetColor) effect.Color = Color;
            effect.Run(Seconds > 0f ? Seconds : item.Record != null ? 0f : 1f, Vector2.zero, 0f);
        }

        // A loop plays until StopSounds.
        public void PlaySound(string name)
        {
            var sound = _soundOutput.Resolve(new AudioClipId(name));
            if (sound.IsEmpty)
            {
                Debug.LogWarning($"CoreEffectPreview: no clip {name}");
                return;
            }

            if (sound.Loop)
                _loops.Add(_renderer.Sounds.Start(sound));
            else
                _renderer.Sounds.Play(sound);
        }

        public void StopSounds()
        {
            foreach (var id in _loops)
                _renderer.Sounds.Stop(id);
            _loops.Clear();
        }

        public void Dispose()
        {
            StopSounds();
            if (Current == this)
                Current = null;
        }

        // The first built-in element of the type, alone.
        private static Item CreateTypeItem(EffectKind kind, List<VisualEffect> records, CoreVisualEffects effects,
            IResourceLocator resourceLocator)
        {
            foreach (var record in records)
            {
                for (var i = 0; i < record.Elements.Count; ++i)
                {
                    var element = record.Elements[i];
                    if (!CoreVisualEffects.TryGetKind(element.Type, out var elementKind) || elementKind != kind)
                        continue;

                    var single = new EffectRecord(new[] { effects.ToElement(element, kind) });
                    return new Item($"{kind} (record {record.Id.Value}, element {i})", single, null);
                }
            }

            return new Item($"{kind} (no built-in element)", null, null);
        }

        private static string Describe(VisualEffect record) =>
            string.Join(", ", record.Elements.Select(item => item.Quantity > 1 ? $"{item.Type} x{item.Quantity}" : item.Type.ToString()));

        // The clips that built-in content names (inventory, step 1), plus the hard-coded asteroid death.
        private static IEnumerable<string> CollectSounds(IDatabase database)
        {
            var names = new SortedSet<string>(StringComparer.Ordinal) { AsteroidDeathSound };
            void AddClip(AudioClipId id) { if (id) names.Add(id.Loop ? "*" + id.Id : id.Id); }

            foreach (var weapon in database.WeaponList)
            {
                AddClip(weapon.Stats.ShotSound);
                AddClip(weapon.Stats.ChargeSound);
            }
            foreach (var device in database.DeviceList)
                AddClip(device.Stats.Sound);
            foreach (var bay in database.DroneBayList)
                AddClip(bay.Stats.LaunchSound);
            foreach (var ammunition in database.AmmunitionList)
                foreach (var trigger in ammunition.Triggers)
                    switch (trigger)
                    {
                        case BulletTrigger_PlaySfx sfx: AddClip(sfx.AudioClip); break;
                        case BulletTrigger_SpawnStaticSfx sfx: AddClip(sfx.AudioClip); break;
                        case BulletTrigger_SpawnBullet spawn: AddClip(spawn.AudioClip); break;
                    }

            var settings = database.ShipSettings;
            if (settings != null)
            {
                AddClip(settings.ShipExplosionSound);
                AddClip(settings.DroneExplosionSound);
            }

            return names;
        }

        public readonly struct Item
        {
            public Item(string name, EffectRecord record, string prefab)
            {
                Name = name;
                Record = record;
                Prefab = prefab;
            }

            public readonly string Name;
            public readonly EffectRecord Record;
            public readonly string Prefab;
            public bool IsEmpty => Record == null && Prefab == null;
        }

        // Owner question 10 (a) list: the named prefabs of step 3.
        private static readonly string[] NamedPrefabs = { "FlashAdditive", "OrbAdditive", "Charge1", "Charge2", "Flicker", "Wave", "Lightning" };
        private const string AsteroidDeathSound = "explosion_08";

        private readonly BattleRenderer _renderer;
        private readonly CoreSoundOutput _soundOutput;
        private readonly Func<CoreEntityId> _controlledShip;
        private readonly List<int> _loops = new List<int>();
    }
}
#endif
