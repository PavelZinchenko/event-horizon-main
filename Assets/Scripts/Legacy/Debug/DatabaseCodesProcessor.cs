using System.Collections.Generic;
using System.Linq;
using Zenject;
using GameServices.Gui;
using Economy.Products;
using GameDatabase;
using GameDatabase.DataModel;
using Domain.Quests;

public class DatabaseCodesProcessor
{
    [Inject] private readonly IDatabase _database;
    [Inject] private readonly GuiHelper _guiHelper;
    [Inject] private readonly ILootItemFactory _lootItemFactory;

    public bool TryExecuteDatabaseCommand(string command)
    {
        if (!int.TryParse(command, out var code))
            return false;

        var debugCodeData = _database.DebugSettings.Codes.FirstOrDefault(item => item.Code == code);
        if (debugCodeData == null)
            return false;

        List<IProduct> items = new(CreateProducts(debugCodeData.Loot));            
        if (items.Count > 0)
        {
            items.Consume();
            _guiHelper.ShowLootWindow(items);
        }
            
        return true;
    }

    private IEnumerable<IProduct> CreateProducts(LootContent content)
    {
        var loot = new Loot(new LootModel(content), new QuestInfo(0), _lootItemFactory, _database);
        return loot.Items.Select(item => new Product(item.Type, item.Quantity));
    }
}
