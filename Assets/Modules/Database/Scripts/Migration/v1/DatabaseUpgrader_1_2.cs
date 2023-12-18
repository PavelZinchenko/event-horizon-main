using System.Collections.Generic;
using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
		partial void Migrate_1_2()
		{
			UnityEngine.Debug.LogWarning("Database migration: v1.1 -> v1.2");

			var lootId = 0;
			foreach (var item in Content.LootList)
				if (item.Id > lootId) 
					lootId = item.Id;
			lootId++;

			var itemList = new List<LootItemSerializable>();
			itemList.Add(CreateLootItem(Enums.LootItemType.Stars, 0, 20));
			itemList.Add(CreateLootItem(Enums.LootItemType.Money, 0, 100));
			CreateLootItems(itemList, new[] { (9, 5), (67, 5), (78, 3), (110, 1), (114, 1) }, Content.ComponentList, Enums.LootItemType.Component);
			CreateLootItems(itemList, new[] { (3, 1), (6, 1) }, Content.SatelliteList, Enums.LootItemType.Satellite);

			var loot = new LootSerializable() { Id = lootId };
			loot.Loot = new LootContentSerializable
			{
				Type = Enums.LootItemType.AllItems,
				Items = itemList.ToArray()
			};

			Content.LootList.Add(loot);
			Content.GalaxySettings.StartingInvenory = lootId;
		}

		private static void CreateLootItems<T>(
			List<LootItemSerializable> loot, 
			(int id, int amount)[] items, 
			List<T> allItems, 
			Enums.LootItemType itemType) 
			where T : SerializableItem
		{
			var ids = new HashSet<int>();
			foreach (var item in allItems)
				ids.Add(item.Id);

			foreach (var item in items)
				if (ids.Contains(item.id))
					loot.Add(CreateLootItem(itemType, item.id, item.amount));
		}

		private static LootItemSerializable CreateLootItem(Enums.LootItemType type, int id, int amount)
		{
			return new LootItemSerializable
			{
				Loot =
				{
					Type = type,
					ItemId = id,
					MinAmount = amount,
					MaxAmount = amount,
				}
			};
		}
    }
}
