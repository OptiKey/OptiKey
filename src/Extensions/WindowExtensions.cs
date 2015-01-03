using System.Windows;
using System.Windows.Interop;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class WindowExtensions
    {
        public static System.Windows.Forms.Screen GetScreen(this Window window)
        {
            var screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
            //screen.

            return screen;
        }
    }
}
