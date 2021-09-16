using System.Reflection;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    public static class WindowExtensions
    {
        public static bool IsModal(this Window window)
        {
            return (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
        }
    }
}