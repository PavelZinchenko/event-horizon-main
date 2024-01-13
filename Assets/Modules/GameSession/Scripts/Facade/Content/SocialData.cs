using Session.Model;

namespace Session.Content
{
	public interface ISocialData
	{
		int FirstDailyRewardDate { get; set; }
		int LastDailyRewardDate { get; set; }
	}

	public class SocialData : ISocialData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public SocialData(SaveGameData sessionData) => _data = sessionData;

		public int LastDailyRewardDate
		{
			get => _data.Social.LastDailyRewardDate;
			set => _data.Social.LastDailyRewardDate = value;
		}

		public int FirstDailyRewardDate
		{
			get => _data.Social.FirstDailyRewardDate;
			set => _data.Social.FirstDailyRewardDate = value;
		}
	}
}
