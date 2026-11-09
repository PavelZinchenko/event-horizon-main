namespace CombatView
{
    // The view settings of the combat scene (migration phase 5b, owner question 6): settings of this module only; the
    // core and the legacy ShipFactory.Settings never see them. Legacy combat sets only the damage text (game setting
    // ShowDamage); shadows and static wrecks are exploration settings, so the combat scene leaves them off.
    public readonly struct CombatViewSettings
    {
        public CombatViewSettings(bool damageText, bool shadows, bool staticWrecks)
        {
            DamageText = damageText;
            Shadows = shadows;
            StaticWrecks = staticWrecks;
        }

        // Legacy NoDamageIndicator, inverted.
        public readonly bool DamageText;
        public readonly bool Shadows;
        public readonly bool StaticWrecks;
    }
}
