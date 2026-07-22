using GameDatabase.Model;
using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class Faction
    {
        static Faction()
        {
            DefaultValue = Empty = new Faction(0, UnityEngine.Color.gray, "$NeutralFaction");
        }

        partial void OnDataDeserialized(FactionSerializable serializable, Database.Loader loader)
        {
            DeveloperHasStarbases = !NoTerritories;
            DeveloperAllowsWanderingShips = !NoWanderingShips;
            UpdateRanges();
        }

        private Faction(int id, UnityEngine.Color color, string name)
        {
            Id = new ItemId<Faction>(id);
            Color = color;
            Name = name;
            Icon = "faction_0";
        }

        public Range HomeStarRange { get; private set; }
        public Range WanderingShipsRange { get; private set; }
        public bool DeveloperHasStarbases { get; private set; }
        public bool DeveloperAllowsWanderingShips { get; private set; }

        // The developer faction editor changes the live database only.  The
        // generated model keeps these properties private, so expose one narrow
        // intentional mutation point instead of editing generated code.
        public void ApplyDeveloperSettings(bool hasTerritories, bool hasStarbases, bool allowsWanderingShips,
            int homeStarDistance, int homeStarDistanceMax, int wanderingShipsDistance, int wanderingShipsDistanceMax)
        {
            NoTerritories = !hasTerritories;
            DeveloperHasStarbases = hasStarbases;
            NoWanderingShips = !allowsWanderingShips;
            DeveloperAllowsWanderingShips = allowsWanderingShips;
            HomeStarDistance = UnityEngine.Mathf.Clamp(homeStarDistance, 0, 5000);
            HomeStarDistanceMax = UnityEngine.Mathf.Clamp(homeStarDistanceMax, 0, 5000);
            WanderingShipsDistance = UnityEngine.Mathf.Clamp(wanderingShipsDistance, 0, 5000);
            WanderingShipsDistanceMax = UnityEngine.Mathf.Clamp(wanderingShipsDistanceMax, 0, 5000);
            UpdateRanges();
        }

        private void UpdateRanges()
        {
            WanderingShipsRange = new Range(
                WanderingShipsDistance != 0 ? WanderingShipsDistance : int.MinValue,
                WanderingShipsDistanceMax != 0 ? WanderingShipsDistanceMax : int.MaxValue);
            HomeStarRange = new Range(
                HomeStarDistance != 0 ? HomeStarDistance : int.MinValue,
                HomeStarDistanceMax != 0 ? HomeStarDistanceMax : int.MaxValue);
        }

        public static readonly Faction Empty;
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
