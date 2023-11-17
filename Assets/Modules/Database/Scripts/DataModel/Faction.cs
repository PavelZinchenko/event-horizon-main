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

            WanderingShipsRange = new Range(
                WanderingShipsDistance != 0 ? WanderingShipsDistance : int.MinValue,
                WanderingShipsDistanceMax != 0 ? WanderingShipsDistanceMax : int.MaxValue);
            HomeStarRange = new Range(
                HomeStarDistance != 0 ? HomeStarDistance : int.MinValue,
                HomeStarDistanceMax != 0 ? HomeStarDistanceMax : int.MaxValue);
        }

        private Faction(int id, UnityEngine.Color color, string name)
        {
            Id = new ItemId<Faction>(id);
            Color = color;
            Name = name;
        }

        public Range HomeStarRange { get; private set; }
        public Range WanderingShipsRange { get; private set; }

        public static readonly Faction Neutral;
        public static readonly Faction Undefined;
    }

    public struct Range
    {
        public Range(int min, int max)
        {
            if (min < max)
            {
                Min = min;
                Max = max;
            }
            else
            {
                Min = max;
                Max = min;
            }
        }

        public bool Contains(int value) => value >= Min && value <= Max;

        public readonly int Min;
        public readonly int Max;
    }
}
