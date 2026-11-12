using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.ComplexView (migration phase 5b, step 5): passes the life and colour to
    // its views (the rocket body, engine and trail).
    public class ComplexView : BaseView
    {
        [SerializeField] private BaseView[] _views;

        public override void UpdateView(float elapsedTime)
        {
            base.UpdateView(elapsedTime);
            foreach (var view in _views)
                view.UpdateView(elapsedTime);
        }

        public override void Dispose()
        {
            foreach (var view in _views)
                view.Dispose();
        }

        protected override void OnGameObjectCreated() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void UpdateLife(float life)
        {
            foreach (var view in _views)
                view.Life = life;
        }

        protected override void UpdatePosition(Vector2 position) {}
        protected override void UpdateRotation(float rotation) {}
        protected override void UpdateSize(float size) {}

        protected override void UpdateColor(Color color)
        {
            foreach (var view in _views)
                view.Color = color;
        }
    }
}
