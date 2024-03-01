namespace RayCarrot.RCP.Metro.Games.Tools.RuntimeModifications;

public record MemoryRegion(string Name, long GameOffset, long? Length, string? ModuleName, long ProcessOffset, bool IsProcessOffsetAPointer);