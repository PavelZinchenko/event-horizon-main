using GameServices;
using Services.GameApplication;
using UnityEngine;
using Zenject;

namespace Gui.Combat
{
    public class PauseGameHelper : MonoBehaviour
    {
        [Inject] private readonly IApplication _application;

        public void PauseGame()
        {
            _application.Pause(this);
        }

        public void ResumeGame()
        {
            _application.Resume(this);
        }
    }
}
