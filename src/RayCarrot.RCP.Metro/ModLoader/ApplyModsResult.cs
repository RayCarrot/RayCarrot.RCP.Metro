using System.Diagnostics.CodeAnalysis;

namespace RayCarrot.RCP.Metro.ModLoader;

public class ApplyModsResult
{
    public ApplyModsResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Success { get; }
    public string? ErrorMessage { get; }
}