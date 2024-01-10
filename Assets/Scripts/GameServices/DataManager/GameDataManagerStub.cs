using Services.Storage;

namespace GameServices.GameManager
{
    public class GameDataManagerStub : IGameDataManager
    {
		public void RestorePurchases() { }
		public void CreateNewGame() { }

		public void LoadMod(string id = null) { }

		public void SaveGameToCloud(string filename) { }
		public void SaveGameToCloud(ISavedGame game) { }
		public void LoadGameFromCloud(ISavedGame game) { }

		public void LoadGameFromLocalCopy() { }
    }
}
