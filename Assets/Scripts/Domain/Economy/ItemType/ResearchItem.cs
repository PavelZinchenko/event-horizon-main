using GameDatabase.DataModel;
using GameDatabase.Model;
using GameServices.Research;
using Services.Localization;
using UnityEngine;
using Zenject;

namespace Economy.ItemType
{
    public class ResearchItem : IItemType
    {
        [Inject]
        public ResearchItem(ILocalization localization, Research research, Faction faction)
        {
            _research = research;
            _localization = localization;
            _faction = faction;
        }

		public string Id => "r" + _faction.Id.Value;
		public string Name => _localization.GetString("$AlienTechnology", _localization.GetString(_faction.Name));
		public string Description => string.Empty;
        public SpriteId Icon => new("Textures/GUI/tech_icon", SpriteId.Type.Default);
		public Color Color => _faction.Color;
		public Price Price => Price.Premium(1);
		public ItemQuality Quality => ItemQuality.Common;

		public void Consume(int amount)
        {
            _research.AddResearchPoints(_faction, amount);
        }

        public void Withdraw(int amount)
        {
            _research.AddResearchPoints(_faction, -amount);
        }

		public int MaxItemsToConsume => int.MaxValue;

		public int MaxItemsToWithdraw => _research.GetAvailablePoints(_faction);

		private readonly Faction _faction;
        private readonly ILocalization _localization;
        private readonly Research _research;
    }
}
