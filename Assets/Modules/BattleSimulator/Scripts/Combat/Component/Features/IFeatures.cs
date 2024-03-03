using System;
using Combat.Component.Collider;
using Combat.Component.Mods;
using Combat.Component.View;
using UnityEngine;

namespace Combat.Component.Features
{
    public enum TargetPriority { None, Low, Normal, High }
    
    [Flags]
    public enum CamouflageType 
    {
        None = 0,
        Drone = 1,
        Missile = 2
    }

    public interface IFeatures : IDisposable
    {
        TargetPriority TargetPriority { get; }
        bool ImmuneToEffects { get; }
        float ChanceToAvoidDrone { get; }
        float ChanceToAvoidMissile { get; }
        Color Color { get; }
        float Opacity { get; }

        void UpdatePhysics(float elapsedTime, ICollider collider);
        void UpdateView(float elapsedTime, IView view);

        Modifications<FeaturesData> Modifications { get; }
    }
}
