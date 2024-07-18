namespace RayCarrot.RCP.Metro.Ini;

public class Rayman3IniAppData : IniAppData
{
    public const string SectionName = "Rayman3";

    public string GLI_Mode { get; set; } = String.Empty;
    public int TexturesCompressed { get; set; }
    public int TnL { get; set; }
    public int TriLinear { get; set; }

    public int Camera_HorizontalAxis { get; set; }
    public int Camera_VerticalAxis { get; set; }

    public string Language { get; set; } = String.Empty;

    public int Video_WantedQuality { get; set; }
    public int Video_BPP { get; set; }
    public int Video_AutoAdjustQuality { get; set; }

    public override void Load(FileSystemPath filePath)
    {
        GLI_Mode = GetString(filePath, SectionName, "GLI_Mode", "1 - 640 x 480 x 16");
        TexturesCompressed = GetInt(filePath, SectionName, "TexturesCompressed");
        TnL = GetInt(filePath, SectionName, "TnL");
        TriLinear = GetInt(filePath, SectionName, "TriLinear");

        Camera_HorizontalAxis = GetInt(filePath, SectionName, "Camera_HorizontalAxis", 2);
        Camera_VerticalAxis = GetInt(filePath, SectionName, "Camera_VerticalAxis", 5);

        Language = GetString(filePath, SectionName, "Language", "English");

        // These use the Rayman Arena app name in the main game exe due to an oversight
        Video_WantedQuality = GetInt(filePath, RaymanArenaIniAppData.SectionName, "Video_WantedQuality");
        Video_BPP = GetInt(filePath, RaymanArenaIniAppData.SectionName, "Video_BPP", 32);
        Video_AutoAdjustQuality = GetInt(filePath, RaymanArenaIniAppData.SectionName, "Video_AutoAdjustQuality", 1);
    }

    public override void Save(FileSystemPath filePath)
    {
        WriteString(filePath, SectionName, "GLI_Mode", GLI_Mode);
        WriteInt(filePath, SectionName, "TexturesCompressed", TexturesCompressed);
        WriteInt(filePath, SectionName, "TnL", TnL);
        WriteInt(filePath, SectionName, "TriLinear", TriLinear);

        WriteInt(filePath, SectionName, "Camera_HorizontalAxis", Camera_HorizontalAxis);
        WriteInt(filePath, SectionName, "Camera_VerticalAxis", Camera_VerticalAxis);
        
        WriteString(filePath, SectionName, "Language", Language);
        
        WriteInt(filePath, RaymanArenaIniAppData.SectionName, "Video_WantedQuality", Video_WantedQuality);
        WriteInt(filePath, RaymanArenaIniAppData.SectionName, "Video_BPP", Video_BPP);
        WriteInt(filePath, RaymanArenaIniAppData.SectionName, "Video_AutoAdjustQuality", Video_AutoAdjustQuality);
    }
}