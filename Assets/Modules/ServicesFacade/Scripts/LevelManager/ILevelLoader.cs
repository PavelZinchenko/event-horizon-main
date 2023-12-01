using Zenject;
using Utils;

namespace GameServices.LevelManager
{
    public interface ILevelLoader
    {
        void LoadLevel(LevelName level, System.Action onCompleted = null, System.Action<DiContainer> installBindingsAction = null);
        void ReloadLevel(System.Action onCompleted = null, System.Action<DiContainer> installBindingsAction = null);
        bool IsLoading { get; }
        LevelName Current { get; }
    }


    public class SceneBeforeUnloadSignal : SmartWeakSignal
    {
        public class Trigger : TriggerBase { }
    }

    public class SceneLoadedSignal : SmartWeakSignal
    {
        public class Trigger : TriggerBase { }
    }
}
