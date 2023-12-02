using System;
using Services.Audio;
using CommonComponents.Utils;

namespace Services.Assets
{
    public enum Status
    {
        Idle,
        Busy,
    }

    public interface IAssetLoader
    {
        void LoadMusicBundle(Action<IMusicPlaylist> onCompleteAction);
        Status Status { get; }
    }

    public class AssetLoaderStatusChangedSignal : SmartWeakSignal<Status>
    {
        public class Trigger : TriggerBase { }
    }
}
