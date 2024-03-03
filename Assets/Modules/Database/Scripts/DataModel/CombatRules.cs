using GameDatabase.Enums;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
    public partial class CombatRules
    {
        public CombatRulesAdapter Create(int level, bool hasRescueUnit) => new CombatRulesAdapter(this, level, hasRescueUnit);
    }

    public readonly struct CombatRulesAdapter
    {
        private readonly CombatRules _combatRules;

        public CombatRulesAdapter(CombatRules combatRules, int level, bool hasRescueUnit)
        {
            _combatRules = combatRules;
            StarLevel = level;
            InitialEnemyShips = combatRules.InitialEnemyShips(level);
            MaxEnemyShips = combatRules.MaxEnemyShips(level);
            TimeLimit = combatRules.TimeLimit(level);

            var canRetreat = hasRescueUnit || combatRules.DisableSkillBonuses;
            if (combatRules.ShipSelection == PlayerShipSelectionMode.Default && !canRetreat)
                ShipSelection = PlayerShipSelectionMode.NoRetreats;
            else
                ShipSelection = combatRules.ShipSelection;
        }

        public int InitialEnemyShips { get; }
        public int MaxEnemyShips { get; }
        public int TimeLimit { get; }
        public int StarLevel { get; }
        public int BattleMapSize => _combatRules.BattleMapSize;
        public TimeOutMode TimeOutMode => _combatRules.TimeOutMode;
        public RewardCondition LootCondition => _combatRules.LootCondition;
        public RewardCondition ExpCondition => _combatRules.ExpCondition;
        public PlayerShipSelectionMode ShipSelection { get; }
        public bool DisableSkillBonuses => _combatRules.DisableSkillBonuses;
        public bool DisableRandomLoot => _combatRules.DisableRandomLoot;
        public bool DisableAsteroids => _combatRules.DisableAsteroids;
        public bool DisablePlanet => _combatRules.DisablePlanet;
        public bool NextEnemyButton => _combatRules.NextEnemyButton;
        public bool KillThemAllButton => _combatRules.KillThemAllButton;
        public ImmutableCollection<SoundTrack> CustomSoundtrack => _combatRules.CustomSoundtrack;
    }
}
