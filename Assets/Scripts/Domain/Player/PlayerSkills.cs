using System;
using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using Session;
using GameServices.Database;
using Services.Messenger;
using GameModel.Skills;
using Zenject;

namespace GameServices.Player
{
    public sealed class PlayerSkills : GameServiceBase
    {
        [Inject] private readonly PlayerFleet _playerFleet;

        [Inject]
        public PlayerSkills(
            IDatabase database,
            ISessionData session, 
            IMessenger messenger, 
            Skills skills,
            SessionDataLoadedSignal dataLoadedSignal, 
            SessionCreatedSignal sessionCreatedSignal)
            : base(dataLoadedSignal, sessionCreatedSignal)
        {
            _skills = skills;
            _session = session;
            _messenger = messenger;
            _database = database;

            Experience.MaxExperience = Experience.FromLevel(skills.TotalSkills);
        }

        public int AvailablePoints
        {
            get
            {
                GetSkillLevels();
                return _experience.Level - _pointsSpent;
            }
        }

        public int PointsSpent { get { return _pointsSpent; } }

        public bool CanAdd(int id)
        {
            var info = _skills[id];
            if (info.IsEmpty)
                return false;

            return CanAddSkill(info);
        }

        public bool HasSkill(int id)
        {
            if (_session.Upgrades.HasSkill(id))
                return true;

            var info = _skills[id];
            if (info.IsEmpty)
                return false;

            return _skills.IsFree(info.Type);
        }

        public bool TryAdd(int id)
        {
            var info = _skills[id];
            if (info.IsEmpty)
                return false;

            if (!CanAddSkill(info))
                return false;

            var level = GetSkillLevels()[info.Type];
            UnityEngine.Debug.Log(info.Type + ": " + level + " -> " + (level + info.Multilpler));
            GetSkillLevels()[info.Type] = level + info.Multilpler;

            _session.Upgrades.AddSkill(id);
            _pointsSpent++;

            _messenger.Broadcast(EventType.PlayerSkillsChanged);

            return true;
        }

        public void Reset()
        {
            _skillLevels = null;
            _pointsSpent = 0;
            _session.Upgrades.ResetSkills();
        }

        public Experience Experience
        {
            get { return _experience; }
            set
            {
                _experience = value;
                _session.Upgrades.PlayerExperience = value;
                _messenger.Broadcast(EventType.PlayerGainedExperience);
            }
        }

        public float ExperienceMultiplier { get { return 1.0f + GetSkillLevels()[SkillType.ShipExperience] * _database.SkillSettings.ExperienceBonus / 100f; } }
        public float AttackMultiplier { get { return 1.0f + GetSkillLevels()[SkillType.ShipAttack] * _database.SkillSettings.AttackBonus / 100f; } }
        public float DefenseMultiplier { get { return 1.0f + GetSkillLevels()[SkillType.ShipDefense] * _database.SkillSettings.DefenseBonus / 100f; } }
        public float ShieldStrengthBonus { get { return GetSkillLevels()[SkillType.ShieldStrength] * _database.SkillSettings.ShieldStrengthBonus / 100f; } }
        public float ShieldRechargeMultiplier { get { return 1.0f + GetSkillLevels()[SkillType.ShieldRecharge] * _database.SkillSettings.ShieldRechargeBonus / 100f; } }
        public int MainFuelCapacity { get { return 100 + _database.SkillSettings.FuelTankCapacity * GetSkillLevels()[SkillType.MainFuelCapacity]; } }
        public float MainEnginePower { get { return 1f + GetSkillLevels()[SkillType.MainEnginePower] * _database.SkillSettings.MapFlightSpeed / 100f; } }
        public float MainFilghtRange { get { return 1.5f + 1.5f * GetSkillLevels()[SkillType.MainEnginePower] * _database.SkillSettings.MapFlightRange / 100f; } }
        public bool HasRescueUnit { get { return GetSkillLevels()[SkillType.MainRescueUnit] > 0; } }
        public float PlanetaryScanner { get { return 1.0f + GetSkillLevels()[SkillType.PlanetaryScanner] * _database.SkillSettings.ExplorationLootBonus / 100f; } }
        public int SpaceScanner { get { return GetSkillLevels()[SkillType.SpaceScanner]; } }

        public float HeatResistance { get { return GetSkillLevels()[SkillType.HeatDefense] * _database.SkillSettings.HeatDefenseBonus / 100f; } }
        public float EnergyResistance { get { return GetSkillLevels()[SkillType.EnergyDefense] * _database.SkillSettings.EnergyDefenseBonus / 100f; } }
        public float KineticResistance { get { return GetSkillLevels()[SkillType.KineticDefense] * _database.SkillSettings.KineticDefenseBonus / 100f; } }

        public bool HasMasterTrader { get { return GetSkillLevels()[SkillType.MasterTrader] > 0; } }
        public float PriceScale { get { return MathF.Max(0f, 1f - GetSkillLevels()[SkillType.Trading] * _database.SkillSettings.MerchantPriceReduction / 100f); } }

        public int CraftingLevelModifier { get { return -GetSkillLevels()[SkillType.CraftingLevel] * _database.SkillSettings.CraftLevelReduction; } }
        public float CraftingPriceScale { get { return MathF.Max(0, 1f - GetSkillLevels()[SkillType.CraftingPrice] * _database.SkillSettings.CraftPriceReduction / 100f); } }

        public long MaxShipExperience 
        {
            get
            {
                if (_database.SkillSettings.DisableExceedTheLimits) 
                    return Maths.Experience.MaxPlayerExperience;
                if (GetSkillLevels()[SkillType.ExceedTheLimits] == 0)
                    return Maths.Experience.MaxPlayerExperience;

                return Maths.Experience.LevelToExp(_database.SkillSettings.IncreasedLevelLimit);
            }
        }

        public int GetAvailableHangarSlots(GameDatabase.Enums.SizeClass size)
        {
            if (size < GameDatabase.Enums.SizeClass.Frigate || size > GameDatabase.Enums.SizeClass.Titan)
                return 0;

            return AvailableHangarSlots(size) - AvailableHangarSlots(size + 1);
        }

        private int AvailableHangarSlots(GameDatabase.Enums.SizeClass size)
        {
            switch (size)
            {
                case GameDatabase.Enums.SizeClass.Frigate:
                    return 3 + GetSkillLevels()[SkillType.HangarSlot1];
                case GameDatabase.Enums.SizeClass.Destroyer:
                    return 1 + GetSkillLevels()[SkillType.HangarSlot2];
                case GameDatabase.Enums.SizeClass.Cruiser:
                    return GetSkillLevels()[SkillType.HangarSlot3];
                case GameDatabase.Enums.SizeClass.Battleship:
                    return GetSkillLevels()[SkillType.HangarSlot4];
                case GameDatabase.Enums.SizeClass.Titan:
                    return GetSkillLevels()[SkillType.HangarSlot5];
                default:
                    return 0;
            }
        }

        protected override void OnSessionDataLoaded()
        {
            _skillLevels = null;
            _pointsSpent = 0;
            _experience = _session.Upgrades.PlayerExperience;
        }

        protected override void OnSessionCreated()
        {
        }

        private bool CanAddSkill(Skills.SkillInfo skill)
        {
            return skill.Type.IsCommonSkill() && AvailablePoints > 0 && !_session.Upgrades.HasSkill(skill.Id);
        }

        private Dictionary<SkillType, int> GetSkillLevels()
        {
            if (_skillLevels == null)
            {
                _skillLevels = Enum.GetValues(typeof(SkillType)).OfType<SkillType>().ToDictionary(item => item, item => 0);
                foreach (var id in _session.Upgrades.Skills)
                {
                    var info = _skills[id];
                    if (info.IsEmpty)
                        continue;

                    if (!info.Type.IsCommonSkill())
                        continue;

                    _skillLevels[info.Type] += info.Multilpler;
                    _pointsSpent++;
                }

                _messenger.Broadcast(EventType.PlayerSkillsChanged);
            }

            return _skillLevels;
        }

        private Dictionary<SkillType, int> _skillLevels;
        private Experience _experience = 0;
        private int _pointsSpent = 0;
        private readonly Skills _skills;

        private readonly IDatabase _database;
        private readonly ISessionData _session;
        private readonly IMessenger _messenger;
    }
}
