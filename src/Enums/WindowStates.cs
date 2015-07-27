using System.Windows;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum WindowStates
    {
        Docked,
        Floating,
        Maximised
    }

    public static partial class EnumExtensions
    {
        public static WindowState ToWindowState(this WindowStates windowState)
        {
            switch (windowState)
            {
                case WindowStates.Maximised:
                    return WindowState.Maximized;

                default: return WindowState.Normal;
            }
        }
    }
}
