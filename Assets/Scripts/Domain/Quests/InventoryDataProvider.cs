using GameDatabase.DataModel;
using GameDatabase.Model;
using Session;

namespace Domain.Quests
{
    public class InventoryDataProvider : IInventoryDataProvider
    {
        private readonly ISessionData _session;

        public InventoryDataProvider(ISessionData session)
        {
            _session = session;
        }

        public int GetQuantity(ItemId<QuestItem> id) => _session.Resources.Resources.GetQuantity(id.Value);
    }
}
