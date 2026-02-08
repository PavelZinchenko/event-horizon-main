using System.Collections.Generic;
using System.Linq;
using Constructor;
using Economy;
using Economy.Products;
using GameModel;
using GameModel.Quests;
using GameServices.Economy;
using GameServices.Player;
using Model.Military;
using GameDatabase.Enums;

namespace Combat.Domain
{
    public class CombatReward : IReward
    {
        public CombatReward(CombatModel combatModel, PlayerSkills playerSkills, LootGenerator lootGenerator, Galaxy.Star currentStar)
        {
            var scenario = (combatModel as CombatModel)?.Scenario ?? CombatScenario.Default;
            var isVictory = combatModel.IsVictory();

            if (combatModel.IsLootAllowed())
            {
                foreach (var item in CreateItems(combatModel, lootGenerator, currentStar, scenario, isVictory))
                {
                    IProduct product;
                    if (_items.TryGetValue(item.Type.Id, out product))
                        _items[item.Type.Id] = CommonProduct.Create(item.Type, item.Quantity + product.Quantity);
                    else
                        _items.Add(item.Type.Id, item);
                }
            }

            PlayerExperience = ExperienceData.Empty;
            if (combatModel.IsExpAllowed())
            {
                var expMultiplier = playerSkills.ExperienceMultiplier * (scenario == CombatScenario.Survival && isVictory ? 3f : 1f);
                foreach (var item in combatModel.PlayerExperience)
                {
                    var exp = (long) (item.Value*expMultiplier);
                    if (exp <= 0)
                        continue;

                    _experience.Add(new ExperienceData(item.Key, exp));
                }

                var totalExp = Experience.Sum(item => item.ExperienceAfter - item.ExperienceBefore);
                PlayerExperience = new ExperienceData(playerSkills.Experience,
                    GameModel.Skills.Experience.ConvertCombatExperience(totalExp, playerSkills.Experience.Level));
            }
        }

        public IEnumerable<IProduct> Items { get { return _items.Values; } }
        public IEnumerable<ExperienceData> Experience { get { return _experience; } }
        public ExperienceData PlayerExperience { get; private set; }

        private IEnumerable<IProduct> CreateItems(CombatModel combatModel, LootGenerator lootGenerator, Galaxy.Star currentStar, CombatScenario scenario, bool isVictory)
        {
            if (combatModel.SpecialRewards != null)
                foreach (var item in combatModel.SpecialRewards)
                    yield return item;

            if (combatModel.Rules.DisableRandomLoot)
                yield break;

            var rewards = lootGenerator.GetCommonReward(combatModel.EnemyFleet.Ships.Where(item => item.Status == ShipStatus.Destroyed).Select(item => item.ShipData),
                currentStar.Level, currentStar.Region.Faction, currentStar.Id);
            foreach (var item in rewards)
                yield return item;

            if (scenario == CombatScenario.Survival && isVictory)
            {
                var random = new System.Random(currentStar.Id + 74123);

                yield return Price.Premium(random.Next(10, 16)).GetProduct(lootGenerator.Factory);

                var count = random.Next(2, 6);
                foreach (var component in lootGenerator.GetRandomComponents(currentStar.Level, count, random.Next(), true, ComponentQuality.P1, ComponentQuality.P3))
                    yield return CommonProduct.Create(component);
            }
        }

        private readonly Dictionary<string, IProduct> _items = new Dictionary<string, IProduct>();
        private readonly List<ExperienceData> _experience = new List<ExperienceData>();
    }
}
