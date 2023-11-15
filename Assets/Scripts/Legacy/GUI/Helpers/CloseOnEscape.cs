using Services.Gui;
using UnityEngine;

namespace Gui
{
    [RequireComponent(typeof(IWindow))]
    public class CloseOnEscape : MonoBehaviour
    {
        private void Update()
        {
            if (Window.Enabled && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                Window.Close();
        }

        private IWindow Window { get { return _window ?? (_window = GetComponent<IWindow>()); } }
        private IWindow _window;
    }
}
