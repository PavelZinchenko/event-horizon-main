namespace Combat.Integration
{
    // Player intent. The runtime validates each request when it applies it;
    // submitting a request does not mean that it succeeded.
    public interface ICombatCommands
    {
        void Deploy(RosterId slot);
        void Retreat(EntityId ship);
        void CallReinforcement();

        // Destroys every player slot, including ready reserves, then ends the battle.
        void Surrender();

        // Test action: Unity Editor, or rules with KillThemAllButton. Destroys every enemy slot, including reserves.
        void KillAllEnemies();
    }
}
