namespace RayCarrot.RCP.Metro.Ini;

public class Rayman2IniAppData : IniAppData
{
    public Rayman2IniAppData(FileSystemPath filePath) : base(filePath) { }

    public const string SectionName = "Rayman2";

    public string GLI_DllFile { get; set; } = String.Empty;
    public string GLI_Dll { get; set; } = String.Empty;
    public string GLI_Driver { get; set; } = String.Empty;
    public string GLI_Device { get; set; } = String.Empty;
    public string GLI_Mode { get; set; } = String.Empty;

    public string Language { get; set; } = String.Empty;

    public override void Load()
    {
        GLI_DllFile = GetString(SectionName, "GLI_DllFile");
        GLI_Dll = GetString(SectionName, "GLI_Dll");
        GLI_Driver = GetString(SectionName, "GLI_Driver");
        GLI_Device = GetString(SectionName, "GLI_Device");
        GLI_Mode = GetString(SectionName, "GLI_Mode");

        Language = GetString(SectionName, "Language");
    }

    public override void Save()
    {
        WriteString(SectionName, "GLI_DllFile", GLI_DllFile);
        WriteString(SectionName, "GLI_Dll", GLI_Dll);
        WriteString(SectionName, "GLI_Driver", GLI_Driver);
        WriteString(SectionName, "GLI_Device", GLI_Device);
        WriteString(SectionName, "GLI_Mode", GLI_Mode);

        WriteString(SectionName, "Language", Language);
    }
}