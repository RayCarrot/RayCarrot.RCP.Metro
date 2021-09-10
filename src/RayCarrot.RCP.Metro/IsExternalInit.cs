using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    // Required to access certain C# 9.0 features (such as records) on older runtimes
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {

    }
}