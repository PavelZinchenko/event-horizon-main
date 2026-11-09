namespace CombatView
{
    // The view settings of the combat scene (migration phase 5b, owner question 6): settings of this module only; the
    // core and the legacy ShipFactory.Settings never see them. Legacy combat sets only the damage text (game setting
    // ShowDamage); shadows and static wrecks are exploration settings, so the combat scene leaves them off.
    public readonly struct CombatViewSettings
    {
        public CombatViewSettings(bool damageText, bool shadows, bool staticWrecks, bool trails = true)
        {
            DamageText = damageText;
            Shadows = shadows;
            StaticWrecks = staticWrecks;
            Trails = trails;
        }

        // Legacy NoDamageIndicator, inverted.
        public readonly bool DamageText;
        public readonly bool Shadows;
        public readonly bool StaticWrecks;

        // Legacy TrailRendererPool: the power saving mode (game setting QualityMode below 0) draws no engine trails
        // (phase 5b, step 4).
        public readonly bool Trails;
    }
}
