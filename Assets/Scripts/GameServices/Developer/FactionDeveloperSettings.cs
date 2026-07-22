using System.Collections.Generic;
using GameDatabase;
using GameDatabase.DataModel;
using UnityEngine;

namespace GameServices.Developer
{
    // A deliberately local developer override.  It never rewrites mod JSON;
    // values are loaded after the database and affect subsequent galaxy/fleet
    // generation for the current installation.
    public static class FactionDeveloperSettings
    {
        private const string Prefix = "ThreeBody.FactionDeveloper.";
        private static readonly Dictionary<int, Values> Defaults = new Dictionary<int, Values>();

        public struct Values
        {
            public bool HasTerritories;
            public bool HasStarbases;
            public bool AllowsWanderingShips;
            public int HomeStarDistance;
            public int HomeStarDistanceMax;
            public int WanderingShipsDistance;
            public int WanderingShipsDistanceMax;
        }

        public static Values Read(Faction faction)
        {
            CaptureDefault(faction);
            var values = Defaults[faction.Id.Value];
            var key = Key(faction);
            if (!PlayerPrefs.HasKey(key + "enabled"))
                return values;

            values.HasTerritories = PlayerPrefs.GetInt(key + "territories", values.HasTerritories ? 1 : 0) != 0;
            values.HasStarbases = PlayerPrefs.GetInt(key + "starbases", values.HasStarbases ? 1 : 0) != 0;
            values.AllowsWanderingShips = PlayerPrefs.GetInt(key + "wandering", values.AllowsWanderingShips ? 1 : 0) != 0;
            values.HomeStarDistance = PlayerPrefs.GetInt(key + "homeMin", values.HomeStarDistance);
            values.HomeStarDistanceMax = PlayerPrefs.GetInt(key + "homeMax", values.HomeStarDistanceMax);
            values.WanderingShipsDistance = PlayerPrefs.GetInt(key + "spawnMin", values.WanderingShipsDistance);
            values.WanderingShipsDistanceMax = PlayerPrefs.GetInt(key + "spawnMax", values.WanderingShipsDistanceMax);
            return Normalize(values);
        }

        public static void Save(Faction faction, Values values)
        {
            CaptureDefault(faction);
            values = Normalize(values);
            var key = Key(faction);
            PlayerPrefs.SetInt(key + "enabled", 1);
            PlayerPrefs.SetInt(key + "territories", values.HasTerritories ? 1 : 0);
            PlayerPrefs.SetInt(key + "starbases", values.HasStarbases ? 1 : 0);
            PlayerPrefs.SetInt(key + "wandering", values.AllowsWanderingShips ? 1 : 0);
            PlayerPrefs.SetInt(key + "homeMin", values.HomeStarDistance);
            PlayerPrefs.SetInt(key + "homeMax", values.HomeStarDistanceMax);
            PlayerPrefs.SetInt(key + "spawnMin", values.WanderingShipsDistance);
            PlayerPrefs.SetInt(key + "spawnMax", values.WanderingShipsDistanceMax);
            Apply(faction, values);
        }

        public static void Apply(IDatabase database)
        {
            foreach (var faction in database.FactionList)
                Apply(faction, Read(faction));
        }

        public static void ResetAll(IDatabase database)
        {
            foreach (var faction in database.FactionList)
            {
                CaptureDefault(faction);
                var key = Key(faction);
                foreach (var suffix in Suffixes)
                    PlayerPrefs.DeleteKey(key + suffix);
                Apply(faction, Defaults[faction.Id.Value]);
            }
            PlayerPrefs.Save();
        }

        private static void Apply(Faction faction, Values values)
        {
            values = Normalize(values);
            faction.ApplyDeveloperSettings(values.HasTerritories, values.HasStarbases, values.AllowsWanderingShips,
                values.HomeStarDistance, values.HomeStarDistanceMax,
                values.WanderingShipsDistance, values.WanderingShipsDistanceMax);
        }

        private static void CaptureDefault(Faction faction)
        {
            if (faction == null || Defaults.ContainsKey(faction.Id.Value))
                return;

            Defaults.Add(faction.Id.Value, new Values
            {
                HasTerritories = !faction.NoTerritories,
                HasStarbases = faction.DeveloperHasStarbases,
                AllowsWanderingShips = !faction.NoWanderingShips,
                HomeStarDistance = faction.HomeStarDistance,
                HomeStarDistanceMax = faction.HomeStarDistanceMax,
                WanderingShipsDistance = faction.WanderingShipsDistance,
                WanderingShipsDistanceMax = faction.WanderingShipsDistanceMax,
            });
        }

        private static Values Normalize(Values values)
        {
            values.HomeStarDistance = Mathf.Clamp(values.HomeStarDistance, 0, 5000);
            values.HomeStarDistanceMax = Mathf.Clamp(values.HomeStarDistanceMax, 0, 5000);
            values.WanderingShipsDistance = Mathf.Clamp(values.WanderingShipsDistance, 0, 5000);
            values.WanderingShipsDistanceMax = Mathf.Clamp(values.WanderingShipsDistanceMax, 0, 5000);
            return values;
        }

        private static string Key(Faction faction) => Prefix + faction.Id.Value + ".";
        private static readonly string[] Suffixes =
        {
            "enabled", "territories", "starbases", "wandering", "homeMin", "homeMax", "spawnMin", "spawnMax",
        };
    }
}
