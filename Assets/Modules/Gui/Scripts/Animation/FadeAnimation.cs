using UnityEngine;

namespace Gui.Animation
{
    public class FadeAnimation : WindowAnimationBase
    {
        protected override void ShowElement(bool visible)
        {
            RootElement.style.opacity = visible ? 1f : 0f;
        }
    }
}
