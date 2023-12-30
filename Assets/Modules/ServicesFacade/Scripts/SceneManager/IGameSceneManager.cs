using System.Collections.Generic;
using CommonComponents.Utils;

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

	public class SceneBeforeUnloadSignal : SmartWeakSignal<GameScene>
	{
		public class Trigger : TriggerBase { }
	}

	public class SceneLoadedSignal : SmartWeakSignal<GameScene>
	{
	        public class Trigger : TriggerBase { }
	}

	public class SceneManagerStateChangedSignal : SmartWeakSignal<State>
	{
		public class Trigger : TriggerBase { }
	}
}
