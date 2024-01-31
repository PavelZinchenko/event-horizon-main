using System;
using System.Collections.Generic;
using Session.Model;

namespace Session.Content
{
	public interface IUpgradesData
	{
		long PlayerExperience { get; set; }
		void AddSkill(int id);
		bool HasSkill(int id);
		void ResetSkills();
		int ResetCounter { get; }
		IEnumerable<int> Skills { get; }
	}

	public class UpgradesData : IUpgradesData, ISessionDataContent
	{
		private readonly SaveGameData _data;
		private readonly PlayerSkillsResetSignal.Trigger _playerSkillsResetTrigger;

		public UpgradesData(SaveGameData sessionData, PlayerSkillsResetSignal.Trigger playerSkillsResetTrigger)
		{
			_data = sessionData;
			_playerSkillsResetTrigger = playerSkillsResetTrigger;
		}

		public long PlayerExperience
		{
			get => _data.Upgrades.PlayerExperience;
			set => _data.Upgrades.PlayerExperience = value;
		}

		public void AddSkill(int id) => _data.Upgrades.Skills.Add(id);
		public bool HasSkill(int id) => _data.Upgrades.Skills.Contains(id);

		public int ResetCounter => _data.Upgrades.ResetCounter;

		public void ResetSkills()
		{
			if (_data.Upgrades.Skills.Count == 0) return;

			_data.Upgrades.Skills.Clear();
			_data.Upgrades.ResetCounter = Math.Min(_data.Upgrades.ResetCounter + 1, 10);
			_playerSkillsResetTrigger.Fire();
		}

		public IEnumerable<int> Skills => _data.Upgrades.Skills;
	}
}
