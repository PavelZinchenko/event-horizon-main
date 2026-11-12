using Combat.Scene;
using UnityEngine;
using Zenject;

namespace Combat.Integration.Legacy
{
    public sealed class LegacyScenePresentation : IScenePresentation
    {
        [Inject]
        public LegacyScenePresentation(IScene scene)
        {
            _scene = scene;
        }

        public Vector2 ViewPoint => _scene.ViewPoint;
        public Rect ViewRect => _scene.ViewRect;
        public float MapWidth => _scene.Settings.AreaWidth;
        public float MapHeight => _scene.Settings.AreaHeight;

        private readonly IScene _scene;
    }
}
