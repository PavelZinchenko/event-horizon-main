using UnityEngine;

namespace Services.Storage
{
    public interface IDataStorage
    {
        void Save(ISerializableGameData data);
        bool TryLoad(ISerializableGameData data, string mod);

        bool TryImportOriginalSave(ISerializableGameData gameData, string mod);
    }
}
