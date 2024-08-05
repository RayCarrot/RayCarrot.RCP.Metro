namespace RayCarrot.RCP.Metro.Ini;

public class Rayman2IniAppData : IniAppData
{
    public const string SectionName = "Rayman2";

    public string GLI_DllFile { get; set; } = String.Empty;
    public string GLI_Dll { get; set; } = String.Empty;
    public string GLI_Driver { get; set; } = String.Empty;
    public string GLI_Device { get; set; } = String.Empty;
    public string GLI_Mode { get; set; } = String.Empty;

    public string Language { get; set; } = String.Empty;

    public string ParticuleRate { get; set; } = String.Empty;

    public override void Load(FileSystemPath filePath)
    {
        GLI_DllFile = GetString(filePath, SectionName, "GLI_DllFile");
        GLI_Dll = GetString(filePath, SectionName, "GLI_Dll");
        GLI_Driver = GetString(filePath, SectionName, "GLI_Driver");
        GLI_Device = GetString(filePath, SectionName, "GLI_Device");
        GLI_Mode = GetString(filePath, SectionName, "GLI_Mode");

        Language = GetString(filePath, SectionName, "Language");

        ParticuleRate = GetString(filePath, SectionName, "ParticuleRate");
    }

    public override void Save(FileSystemPath filePath)
    {
        WriteString(filePath, SectionName, "GLI_DllFile", GLI_DllFile);
        WriteString(filePath, SectionName, "GLI_Dll", GLI_Dll);
        WriteString(filePath, SectionName, "GLI_Driver", GLI_Driver);
        WriteString(filePath, SectionName, "GLI_Device", GLI_Device);
        WriteString(filePath, SectionName, "GLI_Mode", GLI_Mode);

        WriteString(filePath, SectionName, "Language", Language);

        WriteString(filePath, SectionName, "ParticuleRate", ParticuleRate);
    }
}