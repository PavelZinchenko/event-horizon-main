using Combat.Collision.Manager;
using Combat.Component.Unit;
using GameDatabase.Model;
using Services.Audio;
using UnityEngine;

namespace Combat.Collision.Behaviour.Action
{
    public class PlayHitSoundAction : ICollisionAction
    {
        private float _lastSpawnTime;
        private readonly bool _oncePerTarget;
        private readonly float _cooldown;
        private readonly ISoundPlayer _soundPlayer;
        private readonly AudioClipId _audioClipId;

        public PlayHitSoundAction(ISoundPlayer soundPlayer, AudioClipId audioClipId, float cooldown, bool oncePerTarget)
        {
            _cooldown = cooldown;
            _audioClipId = audioClipId;
            _soundPlayer = soundPlayer;
            _oncePerTarget = oncePerTarget;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_oncePerTarget && !collisionData.IsNew) return;

            var time = Time.fixedTime;
            if (time - _lastSpawnTime < _cooldown) return;
            _soundPlayer.Play(_audioClipId);
            _lastSpawnTime = time;
        }

        public void Dispose() { }
    }
}
