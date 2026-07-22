using System;

namespace Services.Gui
{
    public interface IGuiManager
    {
        IWindow FindWindow(string id);
        void OpenWindow(string id, Action<WindowExitCode> onCloseAction = null);
        void OpenWindow(string id, WindowArgs args, Action<WindowExitCode> onCloseAction = null);
        void CloseAllWindows(Func<IWindow, bool> predicate = null);
        bool AutoWindowsAllowed { get; set; }
    }
}
