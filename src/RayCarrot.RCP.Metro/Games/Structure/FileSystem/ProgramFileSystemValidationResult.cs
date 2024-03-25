using System.Diagnostics.CodeAnalysis;

namespace RayCarrot.RCP.Metro.Games.Structure;

public record ProgramFileSystemValidationResult(
    [property: MemberNotNullWhen(false, nameof(ProgramFileSystemValidationResult.InvalidPaths))] bool IsValid, 
    IList<ProgramPath>? InvalidPaths = null);