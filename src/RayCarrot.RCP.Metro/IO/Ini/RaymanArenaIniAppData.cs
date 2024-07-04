namespace RayCarrot.RCP.Metro.Ini;

public class RaymanArenaIniAppData : IniAppData
{
    public RaymanArenaIniAppData(FileSystemPath filePath) : base(filePath) { }

    public const string SectionName = "Rayman Arena";

    public string GLI_Mode { get; set; } = String.Empty;
    public string TexturesFile { get; set; } = String.Empty;
    public int TexturesCompressed { get; set; }
    public int TnL { get; set; }
    public int TriLinear { get; set; }

    public string Language { get; set; } = String.Empty;

    public int Video_WantedQuality { get; set; }
    public int Video_BPP { get; set; }
    public int Video_AutoAdjustQuality { get; set; }

    public int ModemQuality { get; set; }
    public int UDPPort { get; set; }

    public override void Load()
    {
        GLI_Mode = GetString(SectionName, "GLI_Mode", "1 - 640 x 480 x 16");
        TexturesFile = GetString(SectionName, "TexturesFile");
        TexturesCompressed = GetInt(SectionName, "TexturesCompressed");
        TnL = GetInt(SectionName, "TnL");
        TriLinear = GetInt(SectionName, "TriLinear");

        Language = GetString(SectionName, "Language", "English");

        Video_WantedQuality = GetInt(SectionName, "Video_WantedQuality");
        Video_BPP = GetInt(SectionName, "Video_BPP", 32);
        Video_AutoAdjustQuality = GetInt(SectionName, "Video_AutoAdjustQuality", 1);

        ModemQuality = GetInt(SectionName, "ModemQuality", 1);
        UDPPort = GetInt(SectionName, "UDPPort", -1);
    }

    public override void Save()
    {
        WriteString(SectionName, "GLI_Mode", GLI_Mode);
        WriteInt(SectionName, "TexturesCompressed", TexturesCompressed);
        WriteInt(SectionName, "TnL", TnL);
        WriteInt(SectionName, "TriLinear", TriLinear);

        WriteString(SectionName, "Language", Language);

        WriteInt(SectionName, "Video_WantedQuality", Video_WantedQuality);
        WriteInt(SectionName, "Video_BPP", Video_BPP);
        WriteInt(SectionName, "Video_AutoAdjustQuality", Video_AutoAdjustQuality);

        WriteInt(SectionName, "ModemQuality", ModemQuality);
        WriteInt(SectionName, "UDPPort", UDPPort);
    }
}