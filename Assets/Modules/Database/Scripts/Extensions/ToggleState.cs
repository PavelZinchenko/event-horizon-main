using GameDatabase.Enums;

namespace GameDatabase.Extensions
{
    public static class ToggleStateExtensions
    {
        public static bool IsEnabled(this ToggleState state, bool defaultValue)
        {
            switch (state)
            {
                case ToggleState.Enabled:
                    return true;
                case ToggleState.Default:
                    return defaultValue;
                default:
                    return false;
            }
        }
    }
}
