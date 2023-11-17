using Services.InternetTime;

namespace Domain.Quests
{
    public class TimeDataProvider : ITimeDataProvider
    {
        private readonly GameTime _gameTime;

        public TimeDataProvider(GameTime gameTime)
        {
            _gameTime = gameTime;
        }

        public long TotalPlayTime => _gameTime.TotalPlayTime;
    }
}
