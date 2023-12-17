using System.Collections.Generic;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
    public partial class ShipBuild
    {
        public void SetComponents(IEnumerable<InstalledComponent> components)
        {
            Components = new ImmutableCollection<InstalledComponent>(components);
        }

		public Faction Faction => BuildFaction != Faction.Empty ? BuildFaction : Ship.Faction;
    }
}
