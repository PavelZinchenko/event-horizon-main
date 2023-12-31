﻿using UnityEngine;

namespace Combat.Component.View
{
    public class RollingSpriteView : SpriteView
    {
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _startSize = 1.0f;
        [SerializeField] private float _endSize = 1.0f;
        [SerializeField] private bool _random;

        public override void UpdateView(float elapsedTime)
        {
            _extraRotation += 360*_speed*elapsedTime;

            Rotation += _extraRotation;
            base.UpdateView(elapsedTime);
            Rotation -= _extraRotation;
        }

        public override void SetMargins(float margins) 
        {
            if (_startSize < margins) _startSize = margins;
            if (_endSize < margins) _endSize = margins;
        }

        protected override void UpdateLife(float life)
        {
            base.UpdateLife(life);
            if (!Mathf.Approximately(_endSize, _startSize))
                Scale = Mathf.Lerp(_endSize, _startSize, life);
        }

        protected override void OnGameObjectCreated()
        {
            base.OnGameObjectCreated();
            var random = new System.Random(GetHashCode());

            _speed = _random ? random.NextFloatSigned()*_rotationSpeed : _rotationSpeed;
            _extraRotation = random.Next(360);
        }

        private float _speed;
        private float _extraRotation;
    }
}
