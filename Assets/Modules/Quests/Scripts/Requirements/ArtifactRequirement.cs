using GameDatabase.DataModel;
using Services.Localization;

namespace Domain.Quests
{
    public class ArtifactRequirement : IRequirements
    {
        public ArtifactRequirement(QuestItem questItem, int amount, IInventoryDataProvider inventoryData)
        {
            _questItem = questItem;
            _amount = amount;
            _inventoryData = inventoryData;
        }

        public bool IsMet => _inventoryData.GetQuantity(_questItem.Id) >= _amount;
        public bool CanStart(int starId, int seed) => IsMet;

        public string GetDescription(ILocalization localization)
        {
            return _amount > 1
                ? _amount + "x " + localization.GetString(_questItem.Name)
                : localization.GetString(_questItem.Name);
        }

        public int BeaconPosition { get { return -1; } }

        private readonly int _amount;
        private readonly QuestItem _questItem;
        private readonly IInventoryDataProvider _inventoryData;
    }
}
