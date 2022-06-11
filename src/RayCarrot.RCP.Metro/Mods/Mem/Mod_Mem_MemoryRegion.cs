namespace RayCarrot.RCP.Metro;

public record Mod_Mem_MemoryRegion(string Name, long GameOffset, long? Length, string? ModuleName, long ProcessOffset, bool IsProcessOffsetAPointer);