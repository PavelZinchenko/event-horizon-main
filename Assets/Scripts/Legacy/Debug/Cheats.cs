using System;
using System.Collections;
using Constructor;
using GameServices.Database;
using GameServices.Player;
using Maths;
using Session;
using Constructor.Ships;
using System.Collections.Generic;
using Economy.ItemType;
using Economy.Products;
using GameServices.GameManager;
using GameServices.Gui;
using GameServices.Multiplayer;
using Services.Account;
using Services.Unity;
using UnityEngine;
using Zenject;
using Research = GameServices.Research.Research;
using Status = Services.Account.Status;
using System.Linq;
using Database.Legacy;
using Economy;
using Galaxy;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameServices.LevelManager;
using GameDatabase.Extensions;
using Session.Content;
using Helpers = GameModel.Serialization.Helpers;

public class Cheats
{
    [Inject] private readonly PlayerFleet _playerFleet;
    [Inject] private readonly PlayerInventory _playerInventory;
    [Inject] private readonly PlayerResources _playerResources;
    [Inject] private readonly PlayerSkills _playerSkills;
    [Inject] private readonly Research _research;
    [Inject] private readonly ITechnologies _technologies;
    [Inject] private readonly ISessionData _session;
    [Inject] private readonly IGameDataManager _gameDataManager;
    [Inject] private readonly SessionDataLoadedSignal.Trigger _dataLoadedTrigger;
    [Inject] private readonly SessionCreatedSignal.Trigger _sesionCreatedTrigger;
    [Inject] private readonly IAccount _account;
    [Inject] private readonly ICoroutineManager _coroutineManager;
    [Inject] private readonly GuiHelper _guiHelper;
    [Inject] private readonly ItemTypeFactory _itemTypeFactory;
    [Inject] private readonly MotherShip _motherShip;
    [Inject] private readonly StarMap _starMap;
    [Inject] private readonly IDatabase _database;
    [Inject] private readonly ILevelLoader _levelLoader;
    //[Inject] private readonly ItemTypeFactory _itemTypeFactory;

    public bool TryExecuteCommand(string command, int hash)
	{
		#if UNITY_EDITOR
		if (command == "123")
		{
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(49))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(19))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(80))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(78))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(65))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(85))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(99))) { Experience = Maths.Experience.FromLevel(50) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(25))) { Experience = Maths.Experience.FromLevel(50) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(28))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(7))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(16))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(10))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(31))) { Experience = Maths.Experience.FromLevel(100) });
		    _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(22))) { Experience = Maths.Experience.FromLevel(100) });

            foreach (var item in _database.ComponentList.CommonAndRare())
                _playerInventory.Components.Add(new ComponentInfo(item), 25);

      //      _playerResources.Money += 200000;
      //      _playerResources.Stars += 50;

		    foreach (var faction in _database.FactionList.WithTechTree())
			    _research.AddResearchPoints(faction, 50);

		    _playerSkills.Experience = GameModel.Skills.Experience.FromLevel(_playerSkills.Experience.Level + 50);
        }
        else if (command == "345")
        {
            _playerResources.Tokens += 1000;
        }
		else if (command == "000")
		{
			_playerResources.Fuel += 1000;
		}
        else if (command == "666")
        {
            var experience = Experience.FromLevel(200);
            for (var i = 0; i < 3; ++i)
            {
                _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(LegacyShipBuildNames.GetId("MyInvader1"))) { Experience = experience });
                _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(LegacyShipBuildNames.GetId("MyInvader2"))) { Experience = experience });
                _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(LegacyShipBuildNames.GetId("MyInvader3"))) { Experience = experience });
                _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(LegacyShipBuildNames.GetId("MyInvader4"))) { Experience = experience });
            }
        }
        else if (command == "667")
        {
            _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(265))));
            _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(266))));
            _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(267))));
            _playerFleet.Ships.Add(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(235))));
        }
        else if (command == "999")
        {
            var random = new System.Random();
            for (var i = 0; i < 100; ++i)
            {
                _playerInventory.Components.Add(ComponentInfo.CreateRandomModification(_database.GetComponent(LegacyComponentNames.GetId("Nanofiber")), random, ModificationQuality.N3, ModificationQuality.N3));
            }
        }
        #endif

        if (command == "000")
        {
            if (_account.Status != Status.Connected)
                _guiHelper.ShowMessage("Not logged in");
            else
                _guiHelper.ShowMessage("DisplayName: " + _account.DisplayName + "\nId: " + _account.Id);
            return true;
        }
        else if (command.Length == 5)
        {
#if UNITY_EDITOR
            var random = new System.Random();
            var items = new List<IProduct>
            {
                //new Product(_itemTypeFactory.CreatePurchasedStarsItem(), 123),
                //new Product(_itemTypeFactory.CreateSupporterPackItem()),
                new Product(_itemTypeFactory.CreateCurrencyItem(Currency.Stars), 1000),
                new Product(_itemTypeFactory.CreateCurrencyItem(Currency.Credits), 1000000),
                //new Product(_itemTypeFactory.CreateShipItem(new CommonShip(Ships.Get("fns2")))),
                //new Product(_itemTypeFactory.CreateShipItem(new CommonShip(Ships.Get("f0s4")).OfLevel(20))),
                //new Product(_itemTypeFactory.CreateShipItem(new CommonShip(Ships.Get("fas4")).OfLevel(20))),

                //new Product(_itemTypeFactory.CreatePurchasedStarsItem(), 200),
                //new Product(_itemTypeFactory.CreateShipItem(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(94))))), // easter egg
                new Product(_itemTypeFactory.CreateMarketShipItem(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(262))))), // veletz
                //new Product(_itemTypeFactory.CreateShipItem(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(38))).OfLevel(25))),

                //new Product(_itemTypeFactory.CreateComponentItem(ComponentInfo.CreateRandomModification(_database.GetComponent(LegacyComponentNames.GetId("XmasBomb_M_1")), random, ModificationQuality.P3)), 10)
            };

            _coroutineManager.StartCoroutine(SaveItemsToServer(command, items));
#else
            _coroutineManager.StartCoroutine(LoadItemsFromServer(command));
#endif
            return true;
        }

        var id = Security.DebugCommands.Decode(command, hash);
		
		switch (id)
		{
		case 0:
		    _session.Quests.Reset();
            break;
		case 1:
            var center = _motherShip.CurrentStar.Position;
            var stars = _starMap.GetVisibleStars(center - Vector2.one * 20f, center + Vector2.one * 20f);
            foreach (var item in stars)
                item.SetVisited();
            break;
		case 2:
		    foreach (var item in _database.QuestItemList.Where(item => item.Price > 0))
		        _playerResources.AddResource(item.Id, 100000);
			break;
		case 3:
			if (_session.Resources.Stars < 0)
                _session.Resources.Stars += 100;
			break;
		case 4:
		    _playerSkills.Experience = GameModel.Skills.Experience.FromLevel(_playerSkills.Experience.Level + 20);
			break;
		case 20:
		    foreach (var item in _database.QuestItemList.Where(item => item.Price == 0))
		        _playerResources.AddResource(item.Id, 1);
			break;
		case 19:
            foreach (var item in _database.ComponentList.Common())
                _playerInventory.Components.Add(new ComponentInfo(item), 10);
            break;
		case 5:
			_gameDataManager.LoadGameFromLocalCopy();
			break;
		case 6:
			foreach (var ship in _database.ShipBuildList.ValidForPlayer())
                _playerFleet.Ships.Add(new CommonShip(ship));
			break;
		case 7:
			foreach (var ship in _database.ShipBuildList.ValidForPlayer())
                _playerFleet.Ships.Add(new CommonShip(ship));
			break;
		case 8:
			foreach (var ship in _database.ShipBuildList.ValidForPlayer())
                _playerFleet.Ships.Add(new CommonShip(ship));
			break;
		case 9:
			foreach (var ship in _playerFleet.ActiveShipGroup.Ships)
				ship.SetLevel(ship.Experience.Level + 10);
			break;
		case 10:
		    foreach (var faction in _database.FactionList.WithTechTree())
				_research.AddResearchPoints(faction, 100);
			break;
		case 11:
			_session.Game.Regenerate();
            _session.StarMap.Reset();
            _session.Regions.Reset();
            _dataLoadedTrigger.Fire();
            _sesionCreatedTrigger.Fire();
			break;
		case 12:
		    _playerResources.Stars += 100000000;
			break;
		case 13:
			foreach (var ship in _playerFleet.ActiveShipGroup.Ships)
				ship.SetLevel(100);
			break;
		case 14:
			_playerResources.Money += 1000000;
			break;
		case 15:
			_playerResources.Fuel += 1000;
			break;
		case 16:
			foreach (var item in _database.ComponentList.Where(item => item.Availability == Availability.None || item.Availability == Availability.Special))
                _playerInventory.Components.Add(new ComponentInfo(item), 10);
			break;
		case 17:
			foreach (var item in _database.SatelliteList)
				_playerInventory.Satellites.Add(item, 10);
			break;
		case 18:
			foreach (var ship in _database.ShipBuildList.Where(build => build.Ship.ShipType == ShipType.Drone))
			{
				var gameShip = new CommonShip(ship);
				foreach (var item in gameShip.Components)
					item.Locked = false;
                _playerFleet.Ships.Add(gameShip);
			}
			break;
		default:
			return false;
		}

		return true;
	}

    private IEnumerator SaveItemsToServer(string command, IEnumerable<IProduct> items)
    {
        var serialized = FleetWebSerializer.EncodeUrlBase64(Convert.ToBase64String(SerializeItems(items).ToArray()));

        var www = Security.RequestFactory.CreateSetItemsRequest(command, serialized);
        yield return www;

        _guiHelper.ShowMessage("Response: " + (string.IsNullOrEmpty(www.text) ? "OK" : www.text));
    }

    private IEnumerator LoadItemsFromServer(string command)
    {
        var www = Security.RequestFactory.CreateGetItemsRequest(command);
        yield return www;

        _coroutineManager.StartActionOnNextUpdate(OnItemsReceived, www.text);
    }

    private void OnItemsReceived(string data)
    {
        try
        {
            var items = DeserializeItems(System.Convert.FromBase64String(FleetWebSerializer.DecodeUrlBase64(data)));
            foreach (var item in items)
                item.Consume();

            _guiHelper.ShowLootWindow(items);
        }
        catch (Exception e)
        {
            _guiHelper.ShowMessage("Error: " + data);
        }
    }

    private enum ItemType
    {
        Empty = 0,
        Stars = 1,
        Money = 2,
        Ship = 3,
        Component = 4,
        PurchasedStars = 5,
        SupporterPack1 = 6,
    }

    private IEnumerable<IProduct> DeserializeItems(byte[] data)
    {
        var index = 0;

        while (true)
        {
            var id = (ItemType)Helpers.DeserializeInt(data, ref index);
            switch (id)
            {
                case ItemType.Empty:
                    yield break;
                case ItemType.Stars:
                    {
                        var amount = Helpers.DeserializeInt(data, ref index);
                        yield return new Product(_itemTypeFactory.CreateCurrencyItem(Currency.Stars), amount);
                    }
                    break;
                case ItemType.Money:
                    {
                        var amount = Helpers.DeserializeInt(data, ref index);
                        yield return new Product(_itemTypeFactory.CreateCurrencyItem(Currency.Credits), amount);
                    }
                    break;
                case ItemType.Ship:
                    {
                        var ship = ShipDataExtensions.FromShipData(_database, ShipData.Deserialize(data, ref index));
                        yield return new Product(_itemTypeFactory.CreateMarketShipItem(ship));
                    }
                    break;
                case ItemType.Component:
                    {
                        var component = ComponentInfo.FromInt64(_database, Helpers.DeserializeInt(data, ref index));
                        var amount = Helpers.DeserializeInt(data, ref index);
                        yield return new Product(_itemTypeFactory.CreateComponentItem(component), amount);
                    }
                    break;
                case ItemType.PurchasedStars:
                    {
                        var amount = Helpers.DeserializeInt(data, ref index);
                        yield return new Product(_itemTypeFactory.CreatePurchasedStarsItem(), amount);
                    }
                    break;
                case ItemType.SupporterPack1:
                    {
                        yield return new Product(_itemTypeFactory.CreateSupporterPackItem(), 1);
                    }
                    break;
            }
        }
    }

    private IEnumerable<byte> SerializeItems(IEnumerable<IProduct> items)
    {
        foreach (var item in items)
        {
            if (item.Type is StarsItem)
            {
                foreach (var value in Helpers.Serialize((int)ItemType.Stars))
                    yield return value;

                foreach (var value in Helpers.Serialize(item.Quantity))
                    yield return value;
            }
            else if (item.Type is MoneyItem)
            {
                foreach (var value in Helpers.Serialize((int)ItemType.Money))
                    yield return value;

                foreach (var value in Helpers.Serialize(item.Quantity))
                    yield return value;
            }
            else if (item.Type is ShipItemBase)
            {
                foreach (var value in Helpers.Serialize((int)ItemType.Ship))
                    yield return value;

                var ship = ((ShipItemBase)item.Type).Ship;
                foreach (var value in ship.ToShipData().Serialize())
                    yield return value;
            }
            else if (item.Type is ComponentItem)
            {
                foreach (var value in Helpers.Serialize((int)ItemType.Component))
                    yield return value;

                var component = ((ComponentItem)item.Type).Component;
                foreach (var value in Helpers.Serialize(component.SerializeToInt64()))
                    yield return value;

                foreach (var value in Helpers.Serialize(item.Quantity))
                    yield return value;
            }
            else if (item.Type is PurchasedStarsItem)
            {
                foreach (var value in Helpers.Serialize((int)ItemType.PurchasedStars))
                    yield return value;

                foreach (var value in Helpers.Serialize(item.Quantity))
                    yield return value;
            }
            else if (item.Type is SupporterPackItem)
            {
                foreach (var value in Helpers.Serialize((int)ItemType.SupporterPack1))
                    yield return value;
            }
        }

        foreach (var value in Helpers.Serialize((int)ItemType.Empty))
            yield return value;
    }
}