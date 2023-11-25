using GameDatabase.Serializable;
using GameDatabase.Enums;

namespace GameDatabase.DataModel
{
    // TODO: remove this class after database editor can edit builds
    public partial class InstalledComponent
    {
        public InstalledComponent(
            Component component, 
            ComponentModType modType, 
            ModificationQuality quality, 
            int x, 
            int y, 
            int barrelId, 
            int behaviour, 
            int keyBinding)
        {
            Component = component;
            Modification = modType;
            Quality = quality;
            X = x;
            Y = y;
            BarrelId = barrelId;
            Behaviour = behaviour;
            KeyBinding = keyBinding;
        }

        public InstalledComponentSerializable Serialize()
        {
            return new InstalledComponentSerializable
            {
                ComponentId = Component.Id.Value,
                Modification = Modification,
                Quality = Quality,
                X = X,
                Y = Y,
                BarrelId = BarrelId,
                Behaviour = Behaviour,
                KeyBinding = KeyBinding,
            };
        }
    }
}
