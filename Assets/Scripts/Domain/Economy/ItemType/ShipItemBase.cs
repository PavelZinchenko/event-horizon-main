using System.Text;
using Services.Localization;
using Constructor.Ships;
using GameDatabase.Model;
using UnityEngine;
using Constructor.Extensions;

namespace Economy.ItemType
{
    public abstract class ShipItemBase : IItemType
    {
        public ShipItemBase(ILocalization localization, IShip ship)
        {
            _localization = localization;
            _ship = ship;
        }

        public string Id { get { return "sh" + _ship.Id; } }

        public string Name
        {
            get
            {
                if (_name == null)
                    _name = _localization.GetString(_ship.Name);

                return _name;
            }
        }

        public string Description
        {
            get
            {
                if (_description == null)
                {
                    var sb = new StringBuilder();
                    sb.Append(_localization.GetString("$Level"));
                    sb.Append(_localization.GetString(" "));
                    sb.Append(_localization.GetString(_ship.Experience.Level.ToString()));
                    foreach (var mod in Ship.Model.Modifications)
                    {
                        sb.Append("\n");
                        sb.Append(mod.GetDescription(_localization));
                    }

                    _description = sb.ToString();
                }

                return _description;
            }
        }

        public int Rank => _ship.Experience.Level;

        public SpriteId Icon => _ship.Model.ModelImage;
        public virtual Price Price => Price.Common(_ship.Price());

        public Color Color => Color.white;
        public ItemQuality Quality => _ship.Model.Quality();

        public abstract void Consume(int amount);
        public abstract void Withdraw(int amount);
        public abstract int MaxItemsToConsume { get; }
        public abstract int MaxItemsToWithdraw { get; }

        public IShip Ship => _ship;

        private string _name;
        private string _description;
        private readonly IShip _ship;
        private readonly ILocalization _localization;
    }
}
