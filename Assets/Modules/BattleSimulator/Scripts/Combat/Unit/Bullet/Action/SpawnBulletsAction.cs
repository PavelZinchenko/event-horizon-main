﻿using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Platform;
using Combat.Component.Systems.Weapons;
using Combat.Component.Ship;
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
	    public SpawnBulletsAction(IBulletFactory factory, int magazine, IBulletSpawnSettings spawnSettings, IBullet parent, ISoundPlayer soundPlayer, AudioClipId audioClip, ConditionType condition)
        {
            Owner = parent;
            _factory = factory;
            _magazine = magazine;
            _spawnSettings = spawnSettings;
            _soundPlayer = soundPlayer;
            _audioClipId = audioClip;
            Condition = condition;
            EnergyPoints = new UnlimitedEnergy();
            Bullets = BulletCompositeDisposable.Create(factory.Stats);
            _body = factory.Stats.IsBoundToCannon ? new BodyWrapper(parent.GetUnitSizedBody()) : new BodyWrapper(parent.Body);
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
            Bullets.Dispose();
            //_soundPlayer.Stop(GetHashCode());
        }

        public UnitType Type => Owner.Type;
        public IBody Body => _body;
        public IUnit Owner { get; }
        public IResourcePoints EnergyPoints { get; private set; }
        public IBulletCompositeDisposable Bullets { get; }
        public float MountAngle => 0;
        public bool IsReady => true;
        public float Cooldown => 0;
        public float AutoAimingAngle => 0;
        public IShip ActiveTarget { get => null; set {} }

		public void Aim(float bulletVelocity, float weaponRange, float relativeEffect) {}
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
