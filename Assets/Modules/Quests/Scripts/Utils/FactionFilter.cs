using System;
using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Domain.Quests
{
    public class FactionFilter
    {
        public FactionFilter(RequiredFactions requiredFactions, int level, Faction starFaction = null)
        {
            if (requiredFactions == null) return;

            _starFaction = starFaction;
            _starLevel = level;

            _type = requiredFactions.Type;
            var count = requiredFactions.List.Count;

            if (count == 0)
                return;
            if (count == 1)
                _faction = requiredFactions.List[0];
            else if (count > 1)
                _factions = new HashSet<Faction>(requiredFactions.List);
        }

        public bool IsSuitableForFleet(Faction faction)
        {
            if (_type == FactionFilterType.AllAvailable)
            {
                if (IsInList(faction)) return false;
                return !faction.NoWanderingShips && faction.WanderingShipsRange.Contains(_starLevel);
            }

            return IsSuitable(faction, _starFaction);
        }

        public bool IsSuitableForLoot(Faction faction)
        {
            if (_type == FactionFilterType.AllAvailable)
            {
                if (IsInList(faction)) return false;
                return !faction.HideFromMerchants &&
                    (faction == Faction.Empty || faction.HomeStarRange.Contains(_starLevel));
            }

            return IsSuitable(faction, _starFaction);
        }

        public bool IsSuitableForResearch(Faction faction)
        {
            if (_type == FactionFilterType.AllAvailable)
            {
                if (IsInList(faction)) return false;
                return !faction.HideResearchTree &&
                    (faction == Faction.Empty || faction.HomeStarRange.Contains(_starLevel));
            }

            return IsSuitable(faction, _starFaction);
        }

        public bool IsSuitableForBase(Faction faction)
        {
            if (_type == FactionFilterType.AllAvailable)
            {
                if (IsInList(faction)) return false;
                return !faction.NoTerritories && faction.HomeStarRange.Contains(_starLevel);
            }

            return IsSuitable(faction, _starFaction);
        }

        private bool IsInList(Faction faction) => (_faction != null && faction == _faction) || (_factions != null && _factions.Contains(faction));

        private bool IsSuitable(Faction faction, Faction starFaction)
        {
            if (_type == FactionFilterType.StarOwnersAndList && faction == starFaction)
                return true;

            if (_type == FactionFilterType.AllAvailable)
                return false;

            var foundInList = faction == _faction || (_factions != null && _factions.Contains(faction));
            if (_type == FactionFilterType.AllButList)
                return !foundInList;

            if (_type == FactionFilterType.ListOnly || _type == FactionFilterType.StarOwnersAndList)
                return foundInList;

            throw new ArgumentException();
        }

        private readonly FactionFilterType _type;
        private readonly HashSet<Faction> _factions;
        private readonly Faction _faction;
        private readonly Faction _starFaction;
        private readonly int _starLevel;
    }
}
