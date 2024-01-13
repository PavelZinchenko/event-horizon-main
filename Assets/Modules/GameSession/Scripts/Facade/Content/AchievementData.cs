using Session.Model;

namespace Session.Content
{
	public interface IAchievementData
	{
		bool IsUnlocked(int type);
		void UnlockAchievement(int type);
	}

	public class AchievementData : IAchievementData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public AchievementData(SaveGameData sessionData) => _data = sessionData;

		public bool IsUnlocked(int type) => _data.Achievements.Gained.Contains(type);
		public void UnlockAchievement(int type) => _data.Achievements.Gained.Add(type);
	}
}
