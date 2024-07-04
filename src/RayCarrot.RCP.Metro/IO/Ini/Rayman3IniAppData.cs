namespace RayCarrot.RCP.Metro.Ini;

public class Rayman3IniAppData : IniAppData
{
    public Rayman3IniAppData(FileSystemPath filePath) : base(filePath) { }

    public const string SectionName = "Rayman3";

    public string GLI_Mode { get; set; } = String.Empty;
    public string TexturesFile { get; set; } = String.Empty;
    public int TexturesCompressed { get; set; }
    public int TnL { get; set; }
    public int TriLinear { get; set; }

    public int Camera_HorizontalAxis { get; set; }
    public int Camera_VerticalAxis { get; set; }

    public string Language { get; set; } = String.Empty;

    public int Video_WantedQuality { get; set; }
    public int Video_BPP { get; set; }
    public int Video_AutoAdjustQuality { get; set; }

    public override void Load()
    {
        GLI_Mode = GetString(SectionName, "GLI_Mode", "1 - 640 x 480 x 16");
        TexturesFile = GetString(SectionName, "TexturesFile");
        TexturesCompressed = GetInt(SectionName, "TexturesCompressed");
        TnL = GetInt(SectionName, "TnL");
        TriLinear = GetInt(SectionName, "TriLinear");

        Camera_HorizontalAxis = GetInt(SectionName, "Camera_HorizontalAxis", 2);
        Camera_VerticalAxis = GetInt(SectionName, "Camera_VerticalAxis", 5);

        Language = GetString(SectionName, "Language", "English");

        // These use the Rayman Arena app name in the main game exe due to an oversight
        Video_WantedQuality = GetInt(RaymanArenaIniAppData.SectionName, "Video_WantedQuality");
        Video_BPP = GetInt(RaymanArenaIniAppData.SectionName, "Video_BPP", 32);
        Video_AutoAdjustQuality = GetInt(RaymanArenaIniAppData.SectionName, "Video_AutoAdjustQuality", 1);
    }

    public override void Save()
    {
        WriteString(SectionName, "GLI_Mode", GLI_Mode);
        WriteInt(SectionName, "TexturesCompressed", TexturesCompressed);
        WriteInt(SectionName, "TnL", TnL);
        WriteInt(SectionName, "TriLinear", TriLinear);

        WriteInt(SectionName, "Camera_HorizontalAxis", Camera_HorizontalAxis);
        WriteInt(SectionName, "Camera_VerticalAxis", Camera_VerticalAxis);
        
        WriteString(SectionName, "Language", Language);
        
        WriteInt(RaymanArenaIniAppData.SectionName, "Video_WantedQuality", Video_WantedQuality);
        WriteInt(RaymanArenaIniAppData.SectionName, "Video_BPP", Video_BPP);
        WriteInt(RaymanArenaIniAppData.SectionName, "Video_AutoAdjustQuality", Video_AutoAdjustQuality);
    }
}