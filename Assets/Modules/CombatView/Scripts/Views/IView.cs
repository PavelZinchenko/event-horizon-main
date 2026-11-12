using System;
using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.IView (migration phase 5b, step 2). ApplyHsv stays out: only the ship
    // view uses it, and it comes with the ship prefab (step 4).
    public interface IView : IDisposable
    {
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        float Size { get; set; }
        Color Color { get; set; }
        float Life { get; set; }

        void UpdateView(float elapsedTime);
    }
}
