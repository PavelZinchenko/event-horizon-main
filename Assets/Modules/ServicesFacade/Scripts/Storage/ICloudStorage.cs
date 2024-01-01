using System;
using System.Collections.Generic;
using CommonComponents.Signals;

namespace Services.Storage
{
    public enum CloudStorageStatus
    {
        NotReady,
        Idle,
        Synchronizing,
        Loading,
        Saving,
    }

    public interface ISavedGame
    {
        string Name { get; }
        DateTime ModificationTime { get; }

        void Save(ISerializableGameData data);
        void Load(ISerializableGameData data, string mod);
        void Delete();
    }

    public interface ICloudStorage
    {
        CloudStorageStatus Status { get; }
        IEnumerable<ISavedGame> Games { get; }

        void Synchronize();
        void Save(string filename, ISerializableGameData data);

        bool TryLoadFromCopy(ISerializableGameData data, string mod);

        string LastErrorMessage { get; }
    }

    public class CloudStorageStatusChangedSignal : SmartWeakSignal<CloudStorageStatusChangedSignal, CloudStorageStatus> {}
    public class CloudLoadingCompletedSignal : SmartWeakSignal<CloudLoadingCompletedSignal> {}
    public class CloudSavingCompletedSignal : SmartWeakSignal<CloudSavingCompletedSignal> {}
    public class CloudOperationFailedSignal : SmartWeakSignal<CloudOperationFailedSignal, string> {}
    public class CloudSavedGamesReceivedSignal : SmartWeakSignal<CloudSavedGamesReceivedSignal> {}
}
