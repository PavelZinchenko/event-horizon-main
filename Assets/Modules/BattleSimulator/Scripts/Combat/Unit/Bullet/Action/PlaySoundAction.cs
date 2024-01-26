using Combat.Collision;
using GameDatabase.Model;
using Services.Audio;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class PlaySoundAction : IAction
    {
        public PlaySoundAction(ISoundPlayer soundPlayer, float cooldown, AudioClipId audioClipId, ConditionType condition)
        {
            _soundPlayer = soundPlayer;
            _audioClipId = audioClipId;
            _condition = condition;
            Cooldown = cooldown;
        }

        public ConditionType Condition { get { return _condition; } }
        public float Cooldown { get; set; }

        public CollisionEffect Invoke()
        {
            var time = Time.fixedTime;

            if (time < _nextActivation)
                return CollisionEffect.None;
            
            Play();

            _nextActivation = time + Cooldown;
            return CollisionEffect.None;
        }

        public void Dispose()
        {
            if (_audioClipId.Loop)
                _soundPlayer.Stop(GetHashCode());
        }

        private void Play()
        {
            _soundPlayer.Play(_audioClipId, GetHashCode());
        }

        private float _nextActivation;
        private readonly ConditionType _condition;
        private readonly ISoundPlayer _soundPlayer;
        private readonly AudioClipId _audioClipId;
    }
}
