using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Constructor.Modification;
using Economy.ItemType;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using Services.Localization;
using UnityEngine;
using IComponent = Constructor.Component.IComponent;

namespace Constructor
{
    public struct ComponentInfo : IEquatable<ComponentInfo>
    {
        public bool Equals(ComponentInfo other)
        {
            return ModificationType == other.ModificationType && _quality == other._quality && Data.Id == other.Data.Id && _level == other._level;
        }

        public static bool operator ==(ComponentInfo c1, ComponentInfo c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(ComponentInfo c1, ComponentInfo c2)
        {
            return !c1.Equals(c2);
        }

        public static implicit operator bool(ComponentInfo info)
        {
            return info._data != null;
        }

        public ComponentInfo(GameDatabase.DataModel.Component data)
        {
            _data = data;
            _modification = ComponentMod.Empty;
            _quality = ModificationQuality.N3;
            _level = 0;
        }

        public static bool TryCreateRandomComponent(IDatabase database, int level, Faction faction, System.Random random, bool allowRare, 
            ComponentQuality maxQuality, out ComponentInfo componentInfo)
        {
            var maxLevel = 3*level/2;
            var components = allowRare ? database.ComponentList.CommonAndRare() : database.ComponentList.Common();
            if (faction != null) components = components.FilterByFactionOrEmpty(faction);
            var component = components.LevelLessOrEqual(maxLevel).RandomElement(random);
            if (component == null)
            {
                componentInfo = Empty;
                return false;
            }

            var componentLevel = Mathf.Max(10, component.Level);
            var requiredLevel = Mathf.Max(10, level);
            var componentQuality = ComponentQualityExtensions.FromLevel(requiredLevel, componentLevel).Randomize(random);
            if (componentQuality > maxQuality)
                componentQuality = maxQuality;

            if (componentQuality == ComponentQuality.P0)
            {
                componentInfo = new ComponentInfo(component);
                return true;
            }

            var modification = component.PossibleModifications.RandomElement(random);
            componentInfo = new ComponentInfo(component, modification, componentQuality.ToModificationQuality());
            return true;
        }

        public static IEnumerable<ComponentInfo> CreateRandom(IEnumerable<GameDatabase.DataModel.Component> components, int count, int level, System.Random random, ComponentQuality maxQuality = ComponentQuality.P3)
        {
            foreach (var component in components.RandomElements(count, random))
            {
                var componentLevel = Mathf.Max(10, component.Level);
                var requiredLevel = Mathf.Max(10, level);
                var componentQuality = ComponentQualityExtensions.FromLevel(requiredLevel, componentLevel).Randomize(random);
                if (componentQuality > maxQuality)
                    componentQuality = maxQuality;

                if (componentQuality == ComponentQuality.P0)
                {
                    yield return new ComponentInfo(component);
                }
                else
                {
                    var modification = component.PossibleModifications.RandomElement(random);
                    yield return new ComponentInfo(component, modification, componentQuality.ToModificationQuality());
                }
            }
        }

        public bool IsValidModification => ModificationType == ComponentMod.Empty || _data.PossibleModifications.Contains(ModificationType);

        public static ComponentInfo CreateRandomModification(GameDatabase.DataModel.Component data, System.Random random, ModificationQuality minQuality = ModificationQuality.N3, ModificationQuality maxQuality = ModificationQuality.P3)
        {
            if (minQuality > maxQuality)
                Generic.Swap(ref minQuality, ref maxQuality);

            var quality = (ModificationQuality)random.SquareRange((int)minQuality, (int)maxQuality);
            var modification = data.PossibleModifications.RandomElement(random);

            return new ComponentInfo(data, modification, quality);
        }

        public string GetName(ILocalization localization)
        {
            return _level <= 0 ? localization.GetString(Data.Name) : localization.GetString(Data.Name) + " +" + _level;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ComponentInfo))
                return false;
            return Equals((ComponentInfo)obj);
        }

        public override int GetHashCode()
        {
            return Data.Id.GetHashCode() + ((int)_quality << 16) + ModificationType.Id.Value;
        }

		[Obsolete]
		public int SerializeToInt32Obsolete() // 30 bits used
        {
            #if UNITY_EDITOR
            if (Data.Id.Value < 0 || Data.Id.Value > 0x3fff)
            {
                Debug.LogError("Bad component id - " + Data.Id.Value);
                UnityEngine.Debug.Break();
            }

			var modificationType = ModificationType.Id.Value;
            if (modificationType < 0 || modificationType > 0xff)
            {
                Debug.LogError("Bad modification type " + modificationType);
                UnityEngine.Debug.Break();
            }
            #endif

            int value = Data.Id.Value & 0x3fff;
            value <<= 8;
            value += (byte)_quality;
            value <<= 8;
            value += (byte)ModificationType.Id.Value;

            return value;
        }

        [Obsolete] 
		public long SerializeToInt64()
        {
#if UNITY_EDITOR
			var modificationType = ModificationType.Id.Value;
			if ((int)modificationType < 0 || (int)modificationType > 0xff)
                Debug.Break();
            if ((int)_quality < 0 || (int)_quality > 0xff)
                Debug.Break();
            if ((int)_level < 0 || (int)_level > 0xff)
                Debug.Break();
#endif

            long value = Data.Id.Value;
            value <<= 8;
            value += (byte)_quality;
            value <<= 8;
            value += (byte)ModificationType.Id.Value;
            value <<= 8;
            value += (byte)_level;
            value <<= 8;
            // reserved

            return value;
        }

		[Obsolete]
        public static ComponentInfo FromInt32Obsolete(IDatabase database, int data)
        {
            var modification = database.GetComponentMod(ItemId<ComponentMod>.Create((byte)data));
            data >>= 8;
            var quality = (ModificationQuality)(byte)data;
            data >>= 8;
            var component = database.GetComponent(new ItemId<GameDatabase.DataModel.Component>(data));

            return new ComponentInfo(component, modification, quality);
        }

		[Obsolete]
        public static ComponentInfo FromInt64(IDatabase database, long data)
        {
            data >>= 8;
            var level = (byte)data;
            data >>= 8;
            var modification = database.GetComponentMod(ItemId<ComponentMod>.Create((byte)data));
            data >>= 8;
            var quality = (ModificationQuality)(byte)data;
            data >>= 8;
            var component = database.GetComponent(new ItemId<GameDatabase.DataModel.Component>((int)data));

            return new ComponentInfo(component, modification, quality, level);
        }

        public IComponent CreateComponent(int shipSize)
        {
            var component = _data.Create(shipSize);
            component.Modification = ComponentModification.Create(ModificationType, _quality);
            return component;
        }

		public ComponentMod ModificationType => _modification ?? ComponentMod.Empty;

        public IModification CreateModification()
        {
            return ComponentModification.Create(ModificationType, _quality);
        }

        public ModificationQuality ModificationQuality { get { return _quality; } }
		public int Level => _level;

        public Economy.ItemType.ItemQuality ItemQuality
        {
            get
            {
                if (ModificationType == ComponentMod.Empty)
                    return ItemQuality.Common;

                switch (_quality)
                {
                    case ModificationQuality.N1:
                    case ModificationQuality.N2:
                    case ModificationQuality.N3:
                        return ItemQuality.Low;
                    case ModificationQuality.P1:
                        return ItemQuality.Medium;
                    case ModificationQuality.P2:
                        return ItemQuality.High;
                    case ModificationQuality.P3:
                        return ItemQuality.Perfect;
                    default:
                        throw new InvalidEnumArgumentException("_quality", (int)_quality, typeof(ModificationQuality));
                }
            }
        }

        public GameDatabase.DataModel.Component Data { get { return _data ?? GameDatabase.DataModel.Component.Empty; } }

        public Economy.Price Price => Economy.Price.Common(ModificationType == ComponentMod.Empty ? Data.Price(): Data.Price(_quality));

        public Economy.Price PremiumPrice
        {
            get
            {
                ModificationQuality? quality = ModificationType == ComponentMod.Empty ? null : _quality;
#if IAP_DISABLED
                return Economy.Price.Common(Data.PremiumPriceInCredits(quality));
#else
                return Economy.Price.Premium(Data.PremiumPriceInStas(quality));
#endif
            }
        }

        public ComponentInfo(GameDatabase.DataModel.Component data, ComponentMod modification, ModificationQuality quality, int level = 0)
        {
            _data = data;
            _modification = modification;
            if (_modification == null || _modification == ComponentMod.Empty)
                _quality = ModificationQuality.N3;
            else
                _quality = quality;

            _level = Mathf.Clamp(level, 0, MaxUpgradeLevel);
        }

        private static ModificationQuality FromItemQuality(ItemQuality itemQuality, System.Random random)
        {
            switch (itemQuality)
            {
                case ItemQuality.Perfect:
                    return ModificationQuality.P3;
                case ItemQuality.High:
                    return ModificationQuality.P2;
                case ItemQuality.Medium:
                    return ModificationQuality.P1;
                case ItemQuality.Low:
                    return (ModificationQuality)random.Range((int)ModificationQuality.N3, (int)ModificationQuality.N1);
                default:
                    throw new InvalidEnumArgumentException("itemQuality", (int)itemQuality, typeof(ItemQuality));
            }
        }

        private readonly GameDatabase.DataModel.Component _data;
        private readonly ComponentMod _modification;
        private readonly ModificationQuality _quality;
        private readonly int _level;

        public const int MaxUpgradeLevel = 20;
        public static ComponentInfo Empty = new ComponentInfo();
    }
}
