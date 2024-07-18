namespace RayCarrot.RCP.Metro.Ini;

public class RaymanMIniAppData : IniAppData
{
    public const string SectionName = "RaymanM";
    public const string DemoSectionName = "Rayman M Nestle Demo";

    public bool IsDemo { get; set; }

    public string GLI_Mode { get; set; } = String.Empty;
    public string TexturesFile { get; set; } = String.Empty;
    public int TexturesCompressed { get; set; }
    public int TnL { get; set; }
    public int TriLinear { get; set; }

    public string Language { get; set; } = String.Empty;

    public int Video_WantedQuality { get; set; }
    public int Video_BPP { get; set; }
    public int Video_AutoAdjustQuality { get; set; }

    public override void Load(FileSystemPath filePath)
    {
        string sectionName = IsDemo ? DemoSectionName : SectionName;

        GLI_Mode = GetString(filePath, sectionName, "GLI_Mode", "1 - 640 x 480 x 16");
        TexturesFile = GetString(filePath, SectionName, "TexturesFile", "Tex16.cnt");
        TexturesCompressed = GetInt(filePath, sectionName, "TexturesCompressed");
        TnL = GetInt(filePath, sectionName, "TnL");
        TriLinear = GetInt(filePath, sectionName, "TriLinear");

        Language = GetString(filePath, sectionName, "Language", "English");

        Video_WantedQuality = GetInt(filePath, sectionName, "Video_WantedQuality");
        Video_BPP = GetInt(filePath, sectionName, "Video_BPP", 32);
        Video_AutoAdjustQuality = GetInt(filePath, sectionName, "Video_AutoAdjustQuality", 1);
    }

    public override void Save(FileSystemPath filePath)
    {
        string sectionName = IsDemo ? DemoSectionName : SectionName;

        WriteString(filePath, sectionName, "GLI_Mode", GLI_Mode);
        WriteString(filePath, SectionName, "TexturesFile", TexturesFile);
        WriteInt(filePath, sectionName, "TexturesCompressed", TexturesCompressed);
        WriteInt(filePath, sectionName, "TnL", TnL);
        WriteInt(filePath, sectionName, "TriLinear", TriLinear);

        WriteString(filePath, sectionName, "Language", Language);

        WriteInt(filePath, sectionName, "Video_WantedQuality", Video_WantedQuality);
        WriteInt(filePath, sectionName, "Video_BPP", Video_BPP);
        WriteInt(filePath, sectionName, "Video_AutoAdjustQuality", Video_AutoAdjustQuality);
    }
}