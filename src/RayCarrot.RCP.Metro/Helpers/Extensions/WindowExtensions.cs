using System.Reflection;
using System.Windows;

namespace RayCarrot.RCP.Metro;

public static class WindowExtensions
{
    public static bool IsModal(this Window window)
    {
        FieldInfo? fieldInfo = typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic);

        if (fieldInfo == null)
            throw new Exception("Unable to get field info for '_showingAsDialog'");

        return (bool)fieldInfo.GetValue(window);
    }
}