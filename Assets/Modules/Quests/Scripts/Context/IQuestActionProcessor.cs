using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Domain.Quests
{
    public interface IQuestActionProcessor
    {
        void ShowUiDialog(IUserInteraction userInteraction);
        void StartCombat(QuestEnemyData enemyData, ILoot specialLoot);
		void AttackStarbase(int starId);
		void AttackOccupants(int starId);
		void SuppressOccupants(int starId, bool destroy);
        void StartTrading(ILoot merchantItems);
        void Retreat();
        void SetCharacterRelations(int characterId, int value, bool additive);
        void SetFactionRelations(int starId, int value, bool additive);
        void SetFactionStarbasePower(int starId, int value, bool additive);
        void StartQuest(QuestModel quest);
        void OpenShipyard(Faction faction, int level);
        void OpenWorkshop(Faction faction, int level);
        void CaptureStarBase(int starId, bool capture);
        void ChangeFaction(int starId, Faction faction);
    }
}