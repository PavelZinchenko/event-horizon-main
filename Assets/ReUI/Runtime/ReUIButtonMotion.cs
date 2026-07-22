using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// Legacy compatibility component. UITest1 forced every selectable into a
    /// synthetic visual state here, which overwrote authored button graphics on
    /// most gameplay pages. Beta5 keeps Unity's native Selectable behaviour.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class ReUIButtonMotion : MonoBehaviour
    {
        public void RefreshVisualState()
        {
            // Intentionally empty: retain the authored Selectable transition,
            // alpha and child graphics without a second presentation writer.
        }
    }
}
