using Services.InternetTime;
using Session;

namespace Domain.Quests
{
    public class GameDataProvider : IGameDataProvider
    {
        private readonly ISessionData _session;
        private readonly GameTime _gameTime;

        public GameDataProvider(GameTime gameTime, ISessionData session)
        {
            _gameTime = gameTime;
            _session = session;
        }

        public long TotalPlayTime => _gameTime.TotalPlayTime;
        public bool IsGameStarted => _session.IsGameStarted();
        public int GameSeed => _session.Game.Seed;
        public int Counter => _session.Game.Counter;
    }
}
