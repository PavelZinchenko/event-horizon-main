namespace Services.Storage
{
    public interface ISavegameExporter
    {
        public enum Result
        {
            Success,
            Error,
            Cancelled,
            InvalidFormat,
        };

        public delegate void FileExportedCallback(bool success);
        public delegate void FileImportedCallback(Result result);

        void Export(FileExportedCallback callback);
        void Import(ISerializableGameData data, string mod, FileImportedCallback callback);
    }
}
