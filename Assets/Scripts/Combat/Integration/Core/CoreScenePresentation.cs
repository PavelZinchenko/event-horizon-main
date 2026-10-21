using Combat.Domain;
using UnityEngine;
using Zenject;

namespace Combat.Integration.Core
{
    // Camera framing and map size for the core combat scene.
    // Follows the controlled ship; without one the view stays where it was (the arena center at the start).
    public sealed class CoreScenePresentation : IScenePresentation
    {
        [Inject]
        public CoreScenePresentation(ICombatModel combatModel, CoreCombatAdapter adapter)
        {
            _adapter = adapter;
            MapWidth = MapHeight = combatModel.Rules.BattleMapSize;
            _viewPoint = new Vector2(0.5f * MapWidth, 0.5f * MapHeight);
        }

        public Vector2 ViewPoint
        {
            get
            {
                var ship = _adapter.ControlledShipView;
                if (ship != null && ship.IsActive)
                    _viewPoint = ship.VisualPosition;

                return _viewPoint;
            }
        }

        public Rect ViewRect => new Rect(ViewPoint - 0.5f * ViewSize, ViewSize);

        public float MapWidth { get; }
        public float MapHeight { get; }

        private Vector2 _viewPoint;
        private readonly CoreCombatAdapter _adapter;

        private static readonly Vector2 ViewSize = new Vector2(50f * 16f / 9f, 50f);
    }
}
