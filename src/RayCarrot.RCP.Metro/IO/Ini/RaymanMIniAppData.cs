namespace RayCarrot.RCP.Metro.Ini;

public class RaymanMIniAppData : IniAppData
{
    public RaymanMIniAppData(FileSystemPath filePath, bool isDemo) : base(filePath)
    {
        IsDemo = isDemo;
    }

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

    public override void Load()
    {
        string sectionName = IsDemo ? DemoSectionName : SectionName;

        GLI_Mode = GetString(sectionName, "GLI_Mode", "1 - 640 x 480 x 16");
        TexturesFile = GetString(SectionName, "TexturesFile");
        TexturesCompressed = GetInt(sectionName, "TexturesCompressed");
        TnL = GetInt(sectionName, "TnL");
        TriLinear = GetInt(sectionName, "TriLinear");

        Language = GetString(sectionName, "Language", "English");

        Video_WantedQuality = GetInt(sectionName, "Video_WantedQuality");
        Video_BPP = GetInt(sectionName, "Video_BPP", 32);
        Video_AutoAdjustQuality = GetInt(sectionName, "Video_AutoAdjustQuality", 1);
    }

    public override void Save()
    {
        string sectionName = IsDemo ? DemoSectionName : SectionName;

        WriteString(sectionName, "GLI_Mode", GLI_Mode);
        WriteInt(sectionName, "TexturesCompressed", TexturesCompressed);
        WriteInt(sectionName, "TnL", TnL);
        WriteInt(sectionName, "TriLinear", TriLinear);

        WriteString(sectionName, "Language", Language);

        WriteInt(sectionName, "Video_WantedQuality", Video_WantedQuality);
        WriteInt(sectionName, "Video_BPP", Video_BPP);
        WriteInt(sectionName, "Video_AutoAdjustQuality", Video_AutoAdjustQuality);
    }
}