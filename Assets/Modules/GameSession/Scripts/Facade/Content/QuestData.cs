using System.Linq;
using System.Collections.Generic;
using Session.Model;

namespace Session.Content
{
	public interface IQuestData
	{
		bool HasBeenStarted(int questId);
		bool HasBeenCompleted(int questId);
		long LastCompletionTime(int questId);
		long LastStartTime(int questId);
		bool IsQuestActive(int questId);
		bool IsQuestActive(int questId, int starId);
		QuestStatistics GetQuestStatistics(int questId);
		bool IsActiveOrCompleted(int questId);
		int TotalQuestCount();
		long QuestStartTime(int questId, int starId);
		IEnumerable<QuestInformation> GetActiveQuests();
		IEnumerable<QuestInformation> GetQuestProgress(int questId);
		void SetQuestProgress(int questId, int starId, int seed, int activeNode, long currentTime);
		void CancelQuest(int questId, int starId);
		void SetQuestCompleted(int questId, int starId, bool success, long completionTime);
		int GetFactionRelations(int starId);
		void SetFactionRelations(int starId, int value);
		int GetCharacterRelations(int characterId);
		void SetCharacterRelations(int characterId, int value);
		void Reset();
	}

	public readonly struct QuestInformation
	{
		public QuestInformation(int questId, int starId, int seed, int activeNode)
		{
			QuestId = questId;
			StarId = starId;
			Seed = seed;
			ActiveNode = activeNode;
		}

		public readonly int QuestId;
		public readonly int StarId;
		public readonly int Seed;
		public readonly int ActiveNode;
	}

	public class QuestData : IQuestData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public QuestData(SaveGameData sessionData) => _data = sessionData;

		public bool HasBeenStarted(int questId) => _data.Quests.Statistics.ContainsKey(questId) || _data.Quests.Progress.ContainsKey(questId);
		public bool HasBeenCompleted(int questId) => _data.Quests.Statistics.TryGetValue(questId, out var stats) && stats.CompletionCount > 0;
		public long LastCompletionTime(int questId) => _data.Quests.Statistics.TryGetValue(questId, out var stats) ? stats.LastCompletionTime : 0;
		public long LastStartTime(int questId) => _data.Quests.Statistics.TryGetValue(questId, out var stats) ? stats.LastStartTime : 0;
		public bool IsQuestActive(int questId) => _data.Quests.Progress.ContainsKey(questId);
		public bool IsQuestActive(int questId, int starId) => _data.Quests.Progress.TryGetValue(questId, out var progress) && progress.StarQuestsMap.ContainsKey(starId);
		public QuestStatistics GetQuestStatistics(int questId) => _data.Quests.Statistics.TryGetValue(questId, out var stats) ? stats : new QuestStatistics();

		public bool IsActiveOrCompleted(int questId)
		{
			if (_data.Quests.Progress.ContainsKey(questId)) return true;
			if (!_data.Quests.Statistics.TryGetValue(questId, out var stats)) return false;
			return stats.CompletionCount > 0;
		}

		public int TotalQuestCount()
		{
			var total = 0;
			foreach (var statistic in _data.Quests.Statistics.Values)
				total += statistic.CompletionCount + statistic.FailureCount;

			foreach (var progress in _data.Quests.Progress.Values)
				total += progress.StarQuestsMap.Count;

			return total;
		}

		public long QuestStartTime(int questId, int starId)
		{
			if (!_data.Quests.Progress.TryGetValue(questId, out var progress)) return 0;
			if (!progress.StarQuestsMap.TryGetValue(starId, out var questProgress)) return 0;
			return questProgress.StartTime;
		}


		public IEnumerable<QuestInformation> GetActiveQuests()
		{
			foreach (var progress in _data.Quests.Progress)
				foreach (var data in progress.Value.StarQuestsMap)
					yield return new QuestInformation(progress.Key, data.Key, data.Value.Seed, data.Value.ActiveNode);
		}

		public IEnumerable<QuestInformation> GetQuestProgress(int questId)
		{
			if (!_data.Quests.Progress.TryGetValue(questId, out var progress)) return Enumerable.Empty<QuestInformation>();
			return progress.StarQuestsMap.Select(data => new QuestInformation(
				questId, data.Key, data.Value.Seed, data.Value.ActiveNode));
		}

		public void SetQuestProgress(int questId, int starId, int seed, int activeNode, long currentTime)
		{
			if (!_data.Quests.Progress.TryGetValue(questId, out var progress))
			{
				progress = new StarQuestMap(_data.Quests);
				_data.Quests.Progress.Add(questId, progress);
			}

			var startTime = currentTime;
			if (progress.StarQuestsMap.TryGetValue(starId, out var data))
			{
				if (data.Seed == seed && data.ActiveNode == activeNode) return;
				startTime = data.StartTime;
			}
			else
			{
				if (!_data.Quests.Statistics.TryGetValue(questId, out var stats))
					stats = new QuestStatistics();

				_data.Quests.Statistics.SetValue(questId, new QuestStatistics(
					stats.CompletionCount, stats.FailureCount, currentTime, stats.LastCompletionTime));
			}

			progress.StarQuestsMap.SetValue(starId, new QuestProgress(seed, activeNode, startTime));
		}

		public void CancelQuest(int questId, int starId)
		{
			if (!_data.Quests.Progress.TryGetValue(questId, out var progress)) return;
			progress.StarQuestsMap.Remove(starId);
			if (progress.StarQuestsMap.Count == 0) _data.Quests.Progress.Remove(questId);
		}

		public void SetQuestCompleted(int questId, int starId, bool success, long completionTime)
		{
			var stats = GetQuestStatistics(questId);
			var completionCount = stats.CompletionCount;
			var failureCount = stats.FailureCount;
			var lastCompletionTime = stats.LastCompletionTime;

			if (success)
				completionCount++;
			else
				failureCount++;

			if (completionTime > lastCompletionTime)
				lastCompletionTime = completionTime;

			_data.Quests.Statistics.SetValue(questId, new QuestStatistics(completionCount, failureCount, stats.LastStartTime, lastCompletionTime));

			CancelQuest(questId, starId);
		}

		public int GetFactionRelations(int starId) => _data.Quests.FactionRelations.TryGetValue(starId, out int value) ? value : 0;
		public void SetFactionRelations(int starId, int value) => _data.Quests.FactionRelations.SetValue(starId, UnityEngine.Mathf.Clamp(value, -100, 100));
		public int GetCharacterRelations(int characterId) => _data.Quests.CharacterRelations.TryGetValue(characterId, out int value) ? value : 0;
		public void SetCharacterRelations(int characterId, int value) => _data.Quests.CharacterRelations.SetValue(characterId, UnityEngine.Mathf.Clamp(value, -100, 100));

		public void Reset()
		{
			_data.Quests.Progress.Clear();
			_data.Quests.Statistics.Clear();
			_data.Quests.FactionRelations.Clear();
			_data.Quests.CharacterRelations.Clear();
		}
	}
}
