using UnityEngine;

namespace Gui.Animation
{
    public class RightEdgeSlideAnimation : WindowAnimationBase
    {
        [SerializeField] private int _windowWidth = 600;

        protected override void ShowElement(bool visible)
        {
            RootElement.style.right = visible ? 0 : -_windowWidth;
        }
    }
}
