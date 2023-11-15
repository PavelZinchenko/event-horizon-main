using GameDatabase.Model;
using GameDatabase.Serializable;
using GameDatabase.Utils;

namespace GameDatabase.DataModel
{
    public partial class Ship
    {
        partial void OnDataDeserialized(ShipSerializable serializable, Database.Loader loader)
        {
            LoadObsoleteData(serializable, loader);
            Barrels = new ImmutableCollection<Barrel>(BarrelConverter.Convert(Layout, serializable.Barrels));
        }

        private void LoadObsoleteData(ShipSerializable serializable, Database.Loader loader)
        {
            if (serializable.EngineSize > 0)
            {
                var engine = new EngineSerializable { Position = serializable.EnginePosition, Size = serializable.EngineSize };
                Engines = Engine.Create(engine, loader) + Engines;
            }

            switch (serializable.ShipCategory)
            {
                case 1: // Rare
                    ShipRarity = Enums.ShipRarity.Rare;
                    break;
                case 2: // Flagship
                    ShipType = Enums.ShipType.Flagship;
                    break;
                case 3: // Special
                    ShipType = Enums.ShipType.Special;
                    break;
                case 4: // Starbase
                    ShipType = Enums.ShipType.Starbase;
                    SizeClass = Enums.SizeClass.Starbase;
                    break;
                case 5: // Hidden
                    ShipRarity = Enums.ShipRarity.Hidden;
                    break;
                case 6: // Drone
                    ShipType = Enums.ShipType.Drone;
                    break;
            }

            if (serializable.EnergyResistance != 0 || 
                serializable.HeatResistance != 0 ||
                serializable.KineticResistance != 0 ||
                serializable.BaseWeightModifier != 0 ||
                serializable.Regeneration || 
                (serializable.BuiltinDevices != null && serializable.BuiltinDevices.Length > 0))
            {
                var data = new ShipFeaturesSerializable
                {
                    HeatResistance = serializable.HeatResistance,
                    EnergyResistance = serializable.EnergyResistance,
                    KineticResistance = serializable.KineticResistance,
                    ShipWeightBonus = serializable.BaseWeightModifier,
                    Regeneration = serializable.Regeneration,
                    BuiltinDevices = serializable.BuiltinDevices
                };

                Features = ShipFeatures.Create(data, loader);
            }
        }
    }
}
