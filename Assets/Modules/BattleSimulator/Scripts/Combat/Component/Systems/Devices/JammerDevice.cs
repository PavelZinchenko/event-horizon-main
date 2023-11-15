using Combat.Component.Features;
using GameDatabase.Model;

namespace Combat.Component.Systems.Devices
{
    public class JammerDevice : SystemBase, IDevice, IFeaturesModification
    {
        public JammerDevice(TargetPriority targetPriority)
            : base(-1, SpriteId.Empty)
        {
            _targetPriority = targetPriority;
        }

        public override bool CanBeActivated => false;
        public override IFeaturesModification FeaturesModification => this;
        public bool TryApplyModification(ref FeaturesData data)
        {
            data.TargetPriority = _targetPriority;
            return true;
        }

        public void Deactivate() { }

        protected override void OnUpdatePhysics(float elapsedTime) { }
        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnDispose() { }

        private readonly TargetPriority _targetPriority;
    }
}
