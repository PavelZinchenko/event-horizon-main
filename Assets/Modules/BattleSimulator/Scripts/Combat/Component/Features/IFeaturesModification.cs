﻿using Combat.Component.Mods;
using UnityEngine;

namespace Combat.Component.Features
{
    public struct FeaturesData
    {
        public TargetPriority TargetPriority;
        public float ChanceToAvoidDrone;
        public float ChanceToAvoidMissile;
        public Color Color;
        public float Opacity;
        public bool Invulnerable;
        public bool ImmuneToEffects;
    }

    public interface IFeaturesModification : IModification<FeaturesData> {}
}
