using System;
using System.Collections.Generic;

namespace Combat.Component.Unit.Classification
{
    public static class CombatRelations
    {
        private static readonly Dictionary<long, bool> Relations = new();

        public static void SetRelation(int firstFaction, int secondFaction, bool allied)
        {
            Relations[Key(firstFaction, secondFaction)] = allied;
        }

        public static bool AreAllies(UnitType first, UnitType second)
        {
            if (first == null || second == null) return false;
            if ((first.Side == UnitSide.Player && second.Side == UnitSide.Ally) ||
                (first.Side == UnitSide.Ally && second.Side == UnitSide.Player))
                return true;
            if ((first.Side == UnitSide.Ally && second.Side == UnitSide.Enemy) ||
                (first.Side == UnitSide.Enemy && second.Side == UnitSide.Ally))
                return false;
            if ((first.Side == UnitSide.Player && second.Side == UnitSide.Enemy) ||
                (first.Side == UnitSide.Enemy && second.Side == UnitSide.Player))
                return false;
            if (first.FactionId == second.FactionId) return true;
            if (Relations.TryGetValue(Key(first.FactionId, second.FactionId), out var allied)) return allied;

            // Different factions only become allies through an explicit
            // battle-scoped invitation/relation. Strategic friendliness alone
            // must not suppress ordinary encounters.
            return false;
        }

        public static bool AreEnemies(UnitType first, UnitType second) => !AreAllies(first, second);

        private static long Key(int first, int second)
        {
            var min = Math.Min(first, second);
            var max = Math.Max(first, second);
            return ((long)min << 32) | (uint)max;
        }
    }
}
