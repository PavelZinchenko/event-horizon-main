using System.Collections.Generic;
using CommonComponents.Signals;

namespace GameServices.SceneManager
{
    public enum State
    {
        Ready,
        Loading,
    }

    public interface IGameSceneManager
    {
        State State { get; }
        void Load(IEnumerable<GameScene> scenes);
    }

	public class SceneBeforeUnloadSignal : SmartWeakSignal<SceneBeforeUnloadSignal, GameScene> {}
	public class SceneLoadedSignal : SmartWeakSignal<SceneLoadedSignal, GameScene> {}
	public class SceneManagerStateChangedSignal : SmartWeakSignal<SceneManagerStateChangedSignal, State> {}
}
