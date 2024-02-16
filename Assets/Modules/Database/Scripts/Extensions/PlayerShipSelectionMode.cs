using GameDatabase.Enums;

namespace GameDatabase.Extensions
{
    public static class PlayerShipSelectionModeExtensions
    {
        public static bool CanChooseShip(this PlayerShipSelectionMode mode)
        {
            switch (mode)
            {
                case PlayerShipSelectionMode.Default:
                case PlayerShipSelectionMode.NoRetreats:
                case PlayerShipSelectionMode.OnlyOneShip:
                    return true;
                default:
                    return false;
            }
        }
    }
}
