using System.Collections.Generic;
using System.Linq;

namespace Services.Storage
{
    public class EmptyCloudStorage : ICloudStorage
    {
        public CloudStorageStatus Status => CloudStorageStatus.NotReady;
        public IEnumerable<ISavedGame> Games => Enumerable.Empty<ISavedGame>();

        public string LastErrorMessage => string.Empty;

        public void Synchronize() {}
        public void Save(string filename, ISerializableGameData data) {}

        public bool TryLoadFromCopy(ISerializableGameData data, string mod) => false;
    }
}
