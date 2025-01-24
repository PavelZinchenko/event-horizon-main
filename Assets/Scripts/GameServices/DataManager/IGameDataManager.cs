using Services.Storage;

namespace GameServices.GameManager
{
    public interface IGameDataManager
    {
        void RestorePurchases();
        void CreateNewGame();

        void LoadMod(string id = null, bool allowReload = false);

        void SaveGameToCloud(string filename);
        void SaveGameToCloud(ISavedGame game);
        void LoadGameFromCloud(ISavedGame game);

        void LoadGameFromLocalCopy();

        void ImportProgress(ISavegameExporter.FileImportedCallback callback);
        void ExportProgress(ISavegameExporter.FileExportedCallback callback);
    }
}
