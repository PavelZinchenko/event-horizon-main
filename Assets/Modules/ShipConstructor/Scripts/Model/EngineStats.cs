using GameDatabase.DataModel;
using Utilites;

namespace Constructor.Model
{
    public readonly struct EngineStats
    {
        private const float _propulsionFactor = 100f;
        private const float _turnRateFactor = 200f;
        private const float _velocityFactor = 50f;
        private const float _angularVelocityFactor = 50f;
        private const float _unitsToDegrees = 30f;
        private const float _threshold = 0.0001f;

        public readonly float Propulsion;
        public readonly float Velocity;
        public readonly float TurnRateInUnits;
        public readonly float AngularVelocityInUnits;

        public readonly float TurnRate => TurnRateInUnits * _unitsToDegrees;
        public readonly float AngularVelocity => AngularVelocityInUnits * _unitsToDegrees;

        public readonly float VelocityLimit;
        public readonly float AngularVelocityLimit;

        public bool IsNull => Propulsion <= _threshold && TurnRate <= _threshold;

        public EngineStats(float enginePower, float turnRate, float shipWeight, int shipCellCount, ShipSettings shipSettings)
        {
            var sqrtWeight = MathEx.Sqrt(shipWeight);
            VelocityLimit = shipSettings.MaxVelocity;
            AngularVelocityLimit = shipSettings.MaxAngularVelocity * _unitsToDegrees;
            Velocity = MathEx.Min(shipSettings.MaxVelocity, enginePower * _velocityFactor / (sqrtWeight * shipCellCount));
            AngularVelocityInUnits = MathEx.Min(shipSettings.MaxAngularVelocity, turnRate * _angularVelocityFactor / (sqrtWeight * shipCellCount));
            Propulsion = MathEx.Min(shipSettings.MaxAcceleration, enginePower * _propulsionFactor / (shipWeight * shipCellCount));
            TurnRateInUnits = MathEx.Min(shipSettings.MaxAngularAcceleration, turnRate * _turnRateFactor / (shipWeight * sqrtWeight * shipCellCount));
        }
    }
}
