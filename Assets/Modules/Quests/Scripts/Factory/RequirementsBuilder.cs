using System;
using System.Collections.Generic;
using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class RequirementsBuilder : IRequirementFactory<IRequirements>
    {
        public RequirementsBuilder(
            QuestInfo questInfo, 
            ILootCache lootCache,
            IQuestBuilderContext context)
        {
            _questInfo = questInfo;
            _lootCache = lootCache;
            _context = context;
        }

        public IRequirements Build(Requirement requirement)
        {
            return requirement.Create(this);
        }

        public IRequirements Create(Requirement_Empty content)
        {
            return EmptyRequirements.Instance;
        }

        public IRequirements Create(Requirement_Any content)
        {
            return Boolean(content.Requirements, BooleanRequirements.Operation.Any);
        }

        public IRequirements Create(Requirement_All content)
        {
            return Boolean(content.Requirements, BooleanRequirements.Operation.All);
        }

        public IRequirements Create(Requirement_None content)
        {
            return Boolean(content.Requirements, BooleanRequirements.Operation.None);
        }

        public IRequirements Create(Requirement_PlayerPosition content)
        {
            var minDistance = UnityEngine.Mathf.Clamp(content.MinValue, 0, 1000);
            var maxDistance = content.MaxValue >= minDistance ? content.MaxValue : int.MaxValue;

            return new CurrentStarRequirements(minDistance, maxDistance, content.AllowUnsafeStars, _context.PlayerDataProvider);
        }

        public IRequirements Create(Requirement_RandomStarSystem content)
        {
            var random = new System.Random(_questInfo.Seed);
            var minDistance = UnityEngine.Mathf.Clamp(content.MinValue, 1, 1000);
            var maxDistance = UnityEngine.Mathf.Clamp(content.MaxValue, minDistance, 1000);
            var distance = maxDistance > minDistance ? minDistance + random.Next(maxDistance - minDistance + 1) : minDistance;
            var starId = _context.StarMapDataProvider.RandomStarAtDistance(_questInfo.StarId, distance, random);
            return new StarRequirements(starId, content.AllowUnsafeStars, _context.PlayerDataProvider);
        }

        public IRequirements Create(Requirement_AggressiveOccupants content)
        {
            return new OccupantsAttackingRequirements(_context.PlayerDataProvider);
        }

        public IRequirements Create(Requirement_QuestCompleted content)
        {
            return new QuestRequirement(content.Quest, QuestRequirement.RequiredStatus.Completed, _context.QuestDataProvider);
        }

        public IRequirements Create(Requirement_QuestActive content)
        {
            return new QuestRequirement(content.Quest, QuestRequirement.RequiredStatus.Active, _context.QuestDataProvider);
        }

        public IRequirements Create(Requirement_CharacterRelations content)
        {
            return new CharacterRelationsRequirement(content.Character, content.MinValue, content.MaxValue, _context.CharacterDataProvider);
        }

        public IRequirements Create(Requirement_FactionRelations content)
        {
            return new FactionReputationRequirement(_questInfo.StarId, content.MinValue, content.MaxValue, _context.StarMapDataProvider);
        }

        public IRequirements Create(Requirement_FactionStarbasePower content)
        {
            return new StarbasePowerRequirement(_questInfo.StarId, content.MinValue, content.MaxValue, _context.StarMapDataProvider);
        }

        public IRequirements Create(Requirement_StarbaseCaptured content)
        {
            return new StarbaseCaprturedRequirement(_questInfo.StarId, _context.StarMapDataProvider);
        }

        public IRequirements Create(Requirement_Faction content)
        {
            return new FactionRequirements(content.Faction, _context.PlayerDataProvider);
        }

        public IRequirements Create(Requirement_HaveQuestItem content)
        {
            return new ArtifactRequirement(content.QuestItem, content.Amount, _context.InventoryDataProvider);
        }

        public IRequirements Create(Requirement_HaveItem content)
        {
            return new ItemsRequirements(_lootCache.Get(new LootModel(content.Loot), _questInfo));
        }

        public IRequirements Create(Requirement_HaveItemById content)
        {
            return new ItemsRequirements(_lootCache.Get(content.Loot, _questInfo));
        }

        public IRequirements Create(Requirement_TimeSinceQuestStart content)
        {
            return new TimeSinceQuestStart(_questInfo.QuestId, _questInfo.StarId, _context.QuestDataProvider, _context.GameDataProvider, TimeSpan.TicksPerMinute * (content.Hours*60 + content.Minutes));
        }

        public IRequirements Create(Requirement_TimeSinceLastCompletion content)
        {
            return new TimeSinceLastCompletion(_questInfo.QuestId, _context.QuestDataProvider, _context.GameDataProvider, TimeSpan.TicksPerMinute * (content.Hours * 60 + content.Minutes));
        }

        public IRequirements Create(Requirement_ComeToOrigin content)
        {
            return new StarRequirements(_questInfo.StarId, content.AllowUnsafeStars, _context.PlayerDataProvider);
        }

        private IRequirements Boolean(IEnumerable<Requirement> requirements, BooleanRequirements.Operation operation)
        {
            var result = new BooleanRequirements(operation);
            if (requirements == null)
                return result;

            foreach (var item in requirements)
                result.Add(item.Create(this));

            return result;
        }

        private readonly QuestInfo _questInfo;
        private readonly ILootCache _lootCache;
        private readonly IQuestBuilderContext _context;
    }
}
