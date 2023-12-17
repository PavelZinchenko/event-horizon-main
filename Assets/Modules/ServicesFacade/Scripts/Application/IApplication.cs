using System.Collections.Generic;
using CommonComponents.Utils;

namespace Services.GameApplication
{
    public interface IApplication
    {
        bool IsActive { get; }
        bool Paused { get; }

        void Pause(object sender = null);
        void Resume(object sender = null);
    }

    public class GamePausedSignal : SmartWeakSignal<bool>
    {
        public class Trigger : TriggerBase { }
    }

    public class AppActivatedSignal : SmartWeakSignal<bool>
    {
        public class Trigger : TriggerBase { }
    }

    public class GamePauseCounter
    {
        public bool Paused => _objects.Count > 0;

        public bool TryPause(object sender = null)
        {
			return _objects.Add(sender) && _objects.Count == 1;
        }

        public bool TryResume(object sender = null)
        {
			return _objects.Remove(sender) && _objects.Count == 0;
		}

		public void Reset()
        {
            _objects.Clear();
        }

		private readonly HashSet<object> _objects = new();
    }
}
