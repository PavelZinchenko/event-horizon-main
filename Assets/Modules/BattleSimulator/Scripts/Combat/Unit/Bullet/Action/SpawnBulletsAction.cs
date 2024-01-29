using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Platform;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Factory;
using Combat.Unit.HitPoints;
using GameDatabase.Model;
using Services.Audio;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class SpawnBulletsAction : IAction, IWeaponPlatform
    {
	    public SpawnBulletsAction(IBulletFactory factory, int magazine, IBulletSpawnSettings spawnSettings, IUnit parent, ISoundPlayer soundPlayer, AudioClipId audioClip, ConditionType condition)
        {
            Type = parent.Type;
            _body = new BodyWrapper(parent.Body);
            _factory = factory;
            _magazine = magazine;
            _spawnSettings = spawnSettings;
            _soundPlayer = soundPlayer;
            _audioClipId = audioClip;
            Condition = condition;
            EnergyPoints = new UnlimitedEnergy();
        }

        public ConditionType Condition { get; private set; }

        public CollisionEffect Invoke()
        {
            if(_spawnSettings == null)
            {
                if (_magazine <= 1)
                    _factory.Create(this, 0, 0, Vector2.zero);
                else
                {
                    for (var i = 0; i < _magazine; ++i)
                        _factory.Create(this, 0, Random.Range(0, 360), Vector2.zero);
                }
            }
            else
            {
                for (var i = 0; i < _magazine; ++i)
                    _factory.Create(this, 0, _spawnSettings.GetRotation(i), _spawnSettings.GetOffset(i));
            }

            if (_audioClipId) _soundPlayer.Play(_audioClipId, GetHashCode());

            return CollisionEffect.None;
        }

        public void Dispose()
        {
            //_soundPlayer.Stop(GetHashCode());
        }

        public UnitType Type { get; private set; }
        public IBody Body { get { return _body; } }
        public IResourcePoints EnergyPoints { get; private set; }
        public bool IsTemporary { get { return true; } }
        public float FixedRotation { get { return 0; } }
        public bool IsReady { get { return true; } }
        public float Cooldown { get { return 0; } }
        public float AutoAimingAngle { get { return 0; } }
		public IUnit ActiveTarget { get => null; set {} }

		public void Aim(float bulletVelocity, float weaponRange, bool relative) {}
        public void OnShot() {}
        public void SetView(IView view, Color color) { }

        public void UpdatePhysics(float elapsedTime) {}
        public void UpdateView(float elapsedTime) {}

        private readonly AudioClipId _audioClipId;
        private readonly IBulletFactory _factory;
        private readonly IBulletSpawnSettings _spawnSettings;
        private readonly int _magazine;
        private readonly BodyWrapper _body;
        private readonly ISoundPlayer _soundPlayer;
    }
}
