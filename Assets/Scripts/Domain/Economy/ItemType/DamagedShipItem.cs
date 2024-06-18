using System;
using System.Collections.Generic;
using System.Linq;
using Constructor;
using Constructor.Ships;
using GameDatabase.Model;
using GameDatabase.DataModel;
using GameServices.Player;
using Services.Localization;
using UnityEngine;
using GameDatabase;

namespace Economy.ItemType
{
    public class DamagedShipItem : IItemType
    {
        public DamagedShipItem(IDatabase database, PlayerFleet playerFleet, ILocalization localization, ShipBuild ship, int seed)
        {
            _localization = localization;
            _playerFleet = playerFleet;
            _database = database;
            _build = ship;
            _seed = seed;
        }

        public string Id { get { return "ds" + _build.Id; } }
        public string Name { get { return _localization.GetString(_build.Ship.Name); } }
        public string Description { get { return string.Empty; } }
        public SpriteId Icon => _build.Ship.ModelImage;
        public Price Price { get { return Price.Common(_build.Ship.Layout.CellCount * _build.Ship.Layout.CellCount); } }
        public Color Color { get { return Color.white; } }
        public Currency Currency { get { return Currency.Credits; } }
        public ItemQuality Quality { get { return ItemQuality.Common; } }

        public IEnumerable<IntegratedComponent> GetComponents()
        {
            var freespace = _build.Ship.Layout.CellCount / 4 + 1;
            var random = new System.Random(_seed);

            return LimitBySize(_build.Components.
                Select(item => { var component = ComponentExtensions.FromDatabase(item); component.Locked = false; return component; }).
                RandomElements(1, random.Next(freespace / 5), random), freespace);
        }

        public void Consume(int amount)
        {
            for (int i = 0; i < amount; ++i)
                _playerFleet.Ships.Add(new CommonShip(_build.Ship, GetComponents(), _database));
        }

        private static IEnumerable<IntegratedComponent> LimitBySize(IEnumerable<IntegratedComponent> items, int freespace)
        {
            int totalSpace = 0;
            foreach (var item in items)
            {
                totalSpace += item.Info.Data.Layout.CellCount;
                if (totalSpace > freespace)
                    yield break;

                yield return item;
            }
        }

        public void Withdraw(int amount)
        {
            throw new InvalidOperationException();
        }

        public int MaxItemsToConsume { get { return int.MaxValue; } }
        public int MaxItemsToWithdraw { get { return 0; } }

        private readonly int _seed;
        private readonly ShipBuild _build;
        private readonly PlayerFleet _playerFleet;
        private readonly IDatabase _database;
        private readonly ILocalization _localization;
    }
}
