using GameDatabase.DataModel;

namespace Domain.Quests
{
    public interface IQuestActionProcessor
    {
        void ShowUiDialog(IUserInteraction userInteraction);
        void StartCombat(QuestEnemyData enemyData, ILoot specialLoot);
        void AttackOccupants(int starId);
        void SuppressOccupant(int starId, bool destroy);
        void StartTrading(ILoot merchantItems);
        void Retreat();
        void SetCharacterRelations(int characterId, int value, bool additive);
        void SetFactionRelations(int starId, int value, bool additive);
        void StartQuest(QuestModel quest);
        void OpenShipyard(Faction faction, int level);
        void CaptureStarBase(int starId, bool capture);
        void ChangeFaction(int starId, Faction faction);
    }
}