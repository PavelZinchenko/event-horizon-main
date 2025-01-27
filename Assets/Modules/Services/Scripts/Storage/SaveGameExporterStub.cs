using System;

namespace Services.Storage
{
    public class SaveGameExporterStub : ISavegameExporter
    {
        public void Export(ISavegameExporter.FileExportedCallback callback)
        {
            UnityEngine.Debug.LogException(new NotImplementedException());
        }

        public void Import(ISerializableGameData data, string mod, ISavegameExporter.FileImportedCallback callback)
        {
            UnityEngine.Debug.LogException(new NotImplementedException());
        }
    }
}
