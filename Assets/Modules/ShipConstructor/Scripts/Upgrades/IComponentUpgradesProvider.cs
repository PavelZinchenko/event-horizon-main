using System.Collections.Generic;

namespace Constructor
{
    public interface IComponentUpgradesProvider
    {
        IEnumerable<ComponentUpgradeLevel> GetAllUpgrades();
        IComponentUpgrades GetComponentUpgrades(GameDatabase.DataModel.Component component);
    }

    public readonly struct ComponentUpgradeLevel
    {
        public ComponentUpgradeLevel(
            GameDatabase.DataModel.Component component,
            GameDatabase.DataModel.ComponentStatUpgrade upgrade,
            int level)
        {
            Component = component;
            Upgrade = upgrade;
            Level = level;
        }

        public readonly GameDatabase.DataModel.Component Component;
        public readonly GameDatabase.DataModel.ComponentStatUpgrade Upgrade;
        public readonly int Level;
    }
}
