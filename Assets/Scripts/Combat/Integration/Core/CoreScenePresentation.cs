using Combat.Domain;
using UnityEngine;
using Zenject;

namespace Combat.Integration.Core
{
    // Camera framing and map size for the core combat scene. The adapter frames the drawn ships by the legacy rule.
    public sealed class CoreScenePresentation : IScenePresentation
    {
        [Inject]
        public CoreScenePresentation(ICombatModel combatModel, CoreCombatAdapter adapter)
        {
            _adapter = adapter;
            MapWidth = MapHeight = combatModel.Rules.BattleMapSize;
        }

        public Vector2 ViewPoint => _adapter.ViewPoint;
        public Rect ViewRect => _adapter.CameraRect;

        public float MapWidth { get; }
        public float MapHeight { get; }

        private readonly CoreCombatAdapter _adapter;
    }
}
