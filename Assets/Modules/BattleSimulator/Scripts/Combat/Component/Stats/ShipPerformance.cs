namespace Combat.Component.Stats
{
    public class ShipPerformance
    {
        private float _totalArmorDamage;
        private float _totalShieldDamage;
        private float _totalArmorDamageReceived;
        private float _totalShieldDamageReceived;
        private float _totalDamageToAllies;
        private float _totalRepair;
        private int _totalKills;
        private int _totalDeaths;

        public float TotalArmorDamageDealt => _totalArmorDamage;
        public float TotalShieldDamageDealt => _totalShieldDamage;
        public float TotalArmorDamageReceived => _totalArmorDamageReceived;
        public float TotalShieldDamageReceived => _totalShieldDamageReceived;
        public float TotalDamageRepaired => _totalRepair;
        public float TotalDamageToAllies => _totalDamageToAllies;
        public int TotalKills => _totalKills;
        public int TotalDeaths => _totalDeaths;

        public void Inherit(ShipPerformance other)
        {
            if (other == null) return;
            _totalArmorDamage += other._totalArmorDamage;
            _totalShieldDamage += other._totalShieldDamage;
            _totalArmorDamageReceived += other._totalArmorDamageReceived;
            _totalShieldDamageReceived += other._totalShieldDamageReceived;
            _totalDamageToAllies += other._totalDamageToAllies;
            _totalRepair += other._totalRepair;
            _totalKills += other._totalKills;
            _totalDeaths += other._totalDeaths + 1;
        }

        public void OnDamageArmor(float damage) => _totalArmorDamage += damage;
        public void OnDamageShield(float damage) => _totalShieldDamage += damage;
        public void OnDamageAlly(float damage) => _totalDamageToAllies += damage;
        public void OnArmorDamageReceived(float damage) => _totalArmorDamageReceived += damage;
        public void OnShieldDamageReceived(float damage) => _totalShieldDamageReceived += damage;
        public void OnDamageRepaired(float repair) => _totalRepair += repair;
        public void OnEnemyKilled() => _totalKills++;
        public void OnDied() => _totalDeaths++;
    }
}
