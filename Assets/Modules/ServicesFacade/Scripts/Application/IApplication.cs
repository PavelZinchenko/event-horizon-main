using System.Collections.Generic;
using CommonComponents.Signals;

namespace Services.GameApplication
{
    public interface IApplication
    {
        bool IsActive { get; }
        bool Paused { get; }

        // While true the game never pauses (online duel, phase 7, step 6): pause requests still count, but time runs on,
        // also without focus. The shared battle clock does not stop for one player.
        bool AlwaysRunning { get; set; }

        void Pause(object sender = null);
        void Resume(object sender = null);
    }

	public class GamePausedSignal : SmartWeakSignal<GamePausedSignal, bool> {}
    public class AppActivatedSignal : SmartWeakSignal<AppActivatedSignal, bool> {}

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
