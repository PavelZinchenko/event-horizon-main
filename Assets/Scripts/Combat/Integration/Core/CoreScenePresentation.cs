using Combat.Domain;
using UnityEngine;
using Zenject;

namespace Combat.Integration.Core
{
    // Camera framing and map size for the core combat scene.
    // Skeleton: a fixed view at the arena center.
    public sealed class CoreScenePresentation : IScenePresentation
    {
        [Inject]
        public CoreScenePresentation(ICombatModel combatModel)
        {
            MapWidth = MapHeight = combatModel.Rules.BattleMapSize;
        }

        public Vector2 ViewPoint => new Vector2(0.5f * MapWidth, 0.5f * MapHeight);
        public Rect ViewRect => new Rect(ViewPoint - 0.5f * ViewSize, ViewSize);

        public float MapWidth { get; }
        public float MapHeight { get; }

        private static readonly Vector2 ViewSize = new Vector2(50f * 16f / 9f, 50f);
    }
}
