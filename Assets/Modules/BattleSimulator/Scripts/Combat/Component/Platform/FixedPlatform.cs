﻿using Combat.Component.Body;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit.HitPoints;
using UnityEngine;

namespace Combat.Component.Platform
{
    public sealed class FixedPlatform : IWeaponPlatform
    {
        public FixedPlatform(IShip ship, IBody body, float cooldown, IAimingSystem aimingSystem = null)
        {
            _body = body;
            _ship = ship;
            _cooldown = cooldown;
            _aimingSystem = aimingSystem;
        }

        public UnitType Type { get { return _ship.Type; } }
        public IBody Body { get { return _body; } }
        public IUnit Owner => _ship;
        public IResourcePoints EnergyPoints { get { return _ship.Stats.Energy; } }
        public bool IsTemporary { get { return false; } }

        public bool IsReady { get { return _timeFromLastShot > _cooldown; } }
        public float Cooldown { get { return Mathf.Clamp01(1f - _timeFromLastShot / _cooldown); } }

        public float MountAngle => _body.Rotation;
        public float AutoAimingAngle => 0;

		public IShip ActiveTarget
		{
			get => _aimingSystem?.ActiveTarget;
			set
			{
				if (_aimingSystem != null)
					_aimingSystem.ActiveTarget = value;
			}
		}

		public void SetView(IView view, UnityEngine.Color color)
        {
            _view = view;
            _color = color;
        }

        public void Aim(float bulletVelocity, float weaponRange, float relativeEffect)
        {
            if (_aimingSystem != null)
                _aimingSystem.Aim(bulletVelocity, weaponRange, relativeEffect);
        }

        public void OnShot()
        {
            _timeFromLastShot = 0;
        }

        public void UpdatePhysics(float elapsedTime)
        {
            _body.UpdatePhysics(elapsedTime);
            _timeFromLastShot += elapsedTime;
        }

        public void UpdateView(float elapsedTime)
        {
            if (_view != null)
            {
                _view.Color = _color * _ship.Features.Color;
                _view.UpdateView(elapsedTime);
            }
        }

        public void Dispose() {}

        private IView _view;
        private Color _color;
        private float _timeFromLastShot;
        private readonly IShip _ship;
        private readonly float _cooldown;
        private readonly IBody _body;
        private readonly IAimingSystem _aimingSystem;
    }
}
