using System.Windows;

namespace PixelMatcher.Helpers
{
    internal static class WindowHelper
    {
        public static Window GetParentWindow(FrameworkElement element)
        {
            return Window.GetWindow(element);
        }
    }
}
