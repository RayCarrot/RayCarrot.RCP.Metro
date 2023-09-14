namespace RayCarrot.RCP.Metro;

public record FileAssociationInfo(string FileExtension, string Id, string Name, Func<System.Drawing.Icon> GetIconFunc, string IconFileName);