using GameDatabase.Model;
using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class Faction
    {
        static Faction()
        {
            Neutral = new Faction(0, UnityEngine.Color.grey, "$NeutralFaction");
            DefaultValue = Undefined = new Faction(-1, UnityEngine.Color.black, "UNDEFINED");
        }

        partial void OnDataDeserialized(FactionSerializable serializable, Database.Loader loader)
        {
            if (serializable.Hidden)
            {
                NoTerritories = true;
                NoWanderingShips = true;
                HideFromMerchants = true;
                HideResearchTree = true;
            }

            if (serializable.Hostile)
            {
                NoMissions = true;
            }
        }

        private Faction(int id, UnityEngine.Color color, string name)
        {
            Id = new ItemId<Faction>(id);
            Color = color;
            Name = name;
        }

        public static readonly Faction Neutral;
        public static readonly Faction Undefined;
    }
}
