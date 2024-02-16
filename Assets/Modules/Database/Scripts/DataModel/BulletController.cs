using System;
using GameDatabase.Model;
using UnityEngine;

namespace GameDatabase.DataModel
{
    public partial class BulletController
    {
        public virtual float StartingVelocityMultiplier => 1f;
        public virtual bool Continuous => false;
    }
    
    public partial class BulletController_Homing
    {
        public override float StartingVelocityMultiplier => StartingVelocityModifier;
    }
    
    public partial class BulletController_Beam
    {
        public override bool Continuous => true;
    }
}