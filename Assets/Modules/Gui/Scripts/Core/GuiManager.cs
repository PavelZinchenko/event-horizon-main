using System;
using System.Collections.Generic;
using System.Linq;
using GameServices.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonComponents.Signals;
using Zenject;

namespace Services.Gui
{
    public class GuiManager : IInitializable, IDisposable, ITickable, IGuiManager
    {
        [Inject]
        public GuiManager(
            SceneLoadedSignal sceneLoadedSignal,
            SceneBeforeUnloadSignal sceneBeforeUnloadSignal,
            WindowOpenedSignal windowOpenedSignal, 
            WindowClosedSignal windowClosedSignal,
            WindowDestroyedSignal windowDestroyedSignal,
            EscapeKeyPressedSignal.Trigger escapeKeyPressedTrigger)
        {
            _sceneLoadedSignal = sceneLoadedSignal;
            _sceneBeforeUnloadSignal = sceneBeforeUnloadSignal;
            _windowOpenedSignal = windowOpenedSignal;
            _windowClosedSignal = windowClosedSignal;
            _windowDestroyedSignal = windowDestroyedSignal;
            _windowDestroyedSignal.Event += OnWindowDestroyed;
            _escapeKeyPressedTrigger = escapeKeyPressedTrigger;

            _sceneLoadedSignal.Event += OnSceneLoaded;
            _sceneBeforeUnloadSignal.Event += OnSceneBeforeUnload;
            _windowOpenedSignal.Event += OnWindowOpened;
            _windowClosedSignal.Event += OnWindowClosed;
        }

        public void Initialize()
        {
            OnWindowsLoaded(GetAllWindows());
        }

        public void Dispose()
        {
            Cleanup();
        }

        public IWindow FindWindow(string id)
        {
            IWindow window;
            return _windows.TryGetValue(id, out window) ? window : null;
        }

        public void OpenWindow(string id, Action<WindowExitCode> onCloseAction = null)
        {
            IWindow window;
            if (!_windows.TryGetValue(id, out window))
                throw new ArgumentException();

            if (onCloseAction != null)
            {
                if (!_onCloseActions.ContainsKey(window))
                    _onCloseActions.Add(window, onCloseAction);
                else
                    Debug.LogWarning("Window already opened with another onclose action - " + id);
            }

            window.Open();
        }

        public void OpenWindow(string id, WindowArgs args, Action<WindowExitCode> onCloseAction = null)
		{
		    IWindow window;
		    if (!_windows.TryGetValue(id, out window))
		        throw new ArgumentException();

            if (onCloseAction != null)
				_onCloseActions.Add(window, onCloseAction);

			window.Open(args);
		}

        public bool AutoWindowsAllowed
        {
            get { return _autoWindowsAllowed; }
            set
            {
                if (_autoWindowsAllowed == value)
                    return;

                _autoWindowsAllowed = value;
                if (_autoWindowsAllowed)
                    TryOpenShowWhenPossibleWindows();
            }
        }

        public void CloseAllWindows()
        {
            var temp = AutoWindowsAllowed;

            AutoWindowsAllowed = false;

            while (_activeWindows.Any())
                _activeWindows.First().Close();

            AutoWindowsAllowed = temp;
        }

        private void OnWindowOpened(string id)
        {
            //UnityEngine.Debug.Log("Window opened: " + id);

            IWindow window;
            if (!_windows.TryGetValue(id, out window))
                throw new InvalidOperationException("invalid window id: " + id);

            var windowsToClose = new List<IWindow>();
            var enabled = true;

            foreach (var item in _activeWindows)
            {
				if (item == window) continue;

                if (window.Class.CantBeOpenedDueTo(item.Class))
                {
                    //UnityEngine.Debug.Log("Window cant be opened: " + window.Id + " due to " + item.Id);
                    window.Close();
                    return;
                }

                if (item.Class.MustBeClosedDueTo(window.Class))
                {
                    //UnityEngine.Debug.Log("Window must be closed: " + item.Id + " due to " + window.Id);
                    windowsToClose.Add(item);
                    continue;
                }

                if (item.Class.MustBeDisabledDueTo(window.Class) && item.Enabled)
                {
                    //UnityEngine.Debug.Log("Window must be disabled: " + item.Id + " due to " + window.Id);
                    item.Enabled = false;
                }
                if (window.Class.MustBeDisabledDueTo(item.Class))
                {
                    //UnityEngine.Debug.Log("Window must be disabled: " + window.Id + " due to " + item.Id);
                    enabled = false;
                }
            }

            window.Enabled = enabled;
            _activeWindows.Add(window);

            foreach (var item in windowsToClose)
                item.Close();
        }

        private void OnWindowClosed(string id, WindowExitCode exitCode)
        {
            //UnityEngine.Debug.Log("Window closed: " + id);

            IWindow window;
            if (!_windows.TryGetValue(id, out window))
                //throw new InvalidOperationException();
                return;

            _activeWindows.Remove(window);

            UpdateActiveWindows();

            Action<WindowExitCode> action;
            if (_onCloseActions.TryGetValue(window, out action))
            {
                _onCloseActions.Remove(window);
                action.Invoke(exitCode);
            }

            if (window.Class.MustBeOpenedAutomatically() && exitCode != WindowExitCode.Ok)
                ShowWindowWhenPossible(window);

            TryOpenShowWhenPossibleWindows();
        }

        private void ShowWindowWhenPossible(IWindow window)
        {
            _showWhenPossibleWindows.RemoveWhere(item => item.Class.MustBeClosedDueTo(window.Class));
            _showWhenPossibleWindows.Add(window);
        }

        private bool CanOpenShowWhenPossibleWindow(IWindow window)
        {
            foreach (var item in _activeWindows)
            {
                if (window.Class.CantBeOpenedDueTo(item.Class))
                    return false;
                if (window.Class.MustBeClosedDueTo(item.Class))
                    return false;
            }

            return true;
        }

        private void TryOpenShowWhenPossibleWindows()
        {
            if (!AutoWindowsAllowed)
                return;

            for (var i = 0; i < 100; ++i)
            {
                var window = _showWhenPossibleWindows.FirstOrDefault(CanOpenShowWhenPossibleWindow);
                if (window == null)
                    return;

                _showWhenPossibleWindows.Remove(window);

                if (!window.IsVisible)
                    window.Open();
            }

            throw new InvalidOperationException();
        }

        private void UpdateActiveWindows()
        {
            foreach (var first in _activeWindows)
            {
                var enabled = !_activeWindows.Where(second => first != second).
                    Any(second => first.Class.MustBeDisabledDueTo(second.Class));

                if (first.Enabled != enabled)
                    first.Enabled = enabled;
            }
        }

        private void OnSceneLoaded(GameScene scene)
        {
            OnWindowsLoaded(GetWindowsInScene(scene));
        }

        private void OnSceneBeforeUnload(GameScene scene)
        {
            OnWindowsUnloading(GetWindowsInScene(scene));
        }

        private void OnWindowsLoaded(IEnumerable<IWindow> windows)
        {
            foreach (var window in windows)
            {
                if (_windows.ContainsKey(window.Id))
                {
                    UnityEngine.Debug.Log("Window already exists - " + window.Id);
                    Debug.Break();
                }

                _windows.Add(window.Id, window);

                if (window.IsVisible)
                {
                    //UnityEngine.Debug.Log("Active window found: " + window.Id);
                    _activeWindows.Add(window);
                }
                else
                {
                    //UnityEngine.Debug.Log("Window found: " + window.Id);
                }
            }
        }

        private void OnWindowsUnloading(IEnumerable<IWindow> windows)
        {
            foreach (var window in windows)
            {
                _windows.Remove(window.Id);
                _activeWindows.Remove(window);
                _showWhenPossibleWindows.Remove(window);

                Action<WindowExitCode> action;
                if (_onCloseActions.TryGetValue(window, out action))
                {
                    _onCloseActions.Remove(window);
                    action.Invoke(WindowExitCode.Cancel);
                }
            }
        }

        private IEnumerable<IWindow> GetWindowsInScene(GameScene scene) => GetWindowsInScene(SceneManager.GetSceneByName(scene.ToSceneName()));

		private IEnumerable<IWindow> GetWindowsInScene(Scene scene)
		{
			if (!scene.IsValid() || !scene.isLoaded) yield break;

			var windows = new List<IWindow>();
			foreach (var gameObject in scene.GetRootGameObjects())
			{
				gameObject.GetComponentsInChildren<IWindow>(true, windows);
				foreach (var window in windows)
					yield return window;
			}
		}

		private IEnumerable<IWindow> GetAllWindows()
        {
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				foreach (var window in GetWindowsInScene(scene))
					yield return window;
			}
        }

        private void OnWindowDestroyed(IWindow window)
        {
            if (window == null) return;

            IWindow oldWindow;
            if (!_windows.TryGetValue(window.Id, out oldWindow) || oldWindow != window) return;

            _windows.Remove(window.Id);
            _activeWindows.Remove(window);
            _showWhenPossibleWindows.Remove(window);
            _onCloseActions.Remove(window);
        }

        private void Cleanup()
        {
            _windows.Clear();
            _activeWindows.Clear();
            _showWhenPossibleWindows.Clear();

            var actions = _onCloseActions.Values.ToArray();
            _onCloseActions.Clear();

            foreach (var item in actions)
                item.Invoke(WindowExitCode.Cancel);
        }

        private bool _autoWindowsAllowed = true;
        private readonly HashSet<IWindow> _showWhenPossibleWindows = new HashSet<IWindow>();
        private readonly Dictionary<string, IWindow> _windows = new Dictionary<string, IWindow>();
        private readonly HashSet<IWindow> _activeWindows = new HashSet<IWindow>();
        private readonly Dictionary<IWindow, Action<WindowExitCode>> _onCloseActions = new Dictionary<IWindow, Action<WindowExitCode>>();

        private readonly SceneLoadedSignal _sceneLoadedSignal;
        private readonly SceneBeforeUnloadSignal _sceneBeforeUnloadSignal;
        private readonly WindowOpenedSignal _windowOpenedSignal;
        private readonly WindowClosedSignal _windowClosedSignal;
        private readonly WindowDestroyedSignal _windowDestroyedSignal;
        private readonly EscapeKeyPressedSignal.Trigger _escapeKeyPressedTrigger;

        public void Tick()
        {
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                IWindow windowToClose = null;
                foreach (var window in _activeWindows)
                {
                    if (window.EscapeAction == EscapeKeyAction.Block)
                        return;
                    if (window.EscapeAction == EscapeKeyAction.Close && window.Enabled)
                        if (windowToClose == null || window.Class.HasHigherClosePriority(windowToClose.Class))
                            windowToClose = window;
                }

                if (windowToClose != null)
                    windowToClose.Close();
                else
                    _escapeKeyPressedTrigger.Fire();
            }
        }
    }

    public class WindowOpenedSignal : SmartWeakSignal<WindowOpenedSignal, string> {}
    public class WindowClosedSignal : SmartWeakSignal<WindowClosedSignal, string, WindowExitCode> {}
    public class WindowDestroyedSignal : SmartWeakSignal<WindowDestroyedSignal, IWindow> {}
    public class EscapeKeyPressedSignal : SmartWeakSignal<EscapeKeyPressedSignal> {}
}
