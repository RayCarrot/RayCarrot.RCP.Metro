namespace RayCarrot.RCP.Metro.Ini;

public class RaymanArenaIniAppData : IniAppData
{
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

    public override void Load(FileSystemPath filePath)
    {
        GLI_Mode = GetString(filePath, SectionName, "GLI_Mode", "1 - 640 x 480 x 16");
        TexturesFile = GetString(filePath, SectionName, "TexturesFile", "Tex16.cnt");
        TexturesCompressed = GetInt(filePath, SectionName, "TexturesCompressed");
        TnL = GetInt(filePath, SectionName, "TnL");
        TriLinear = GetInt(filePath, SectionName, "TriLinear");

        Language = GetString(filePath, SectionName, "Language", "English");

        Video_WantedQuality = GetInt(filePath, SectionName, "Video_WantedQuality");
        Video_BPP = GetInt(filePath, SectionName, "Video_BPP", 32);
        Video_AutoAdjustQuality = GetInt(filePath, SectionName, "Video_AutoAdjustQuality", 1);

        ModemQuality = GetInt(filePath, SectionName, "ModemQuality", 1);
        UDPPort = GetInt(filePath, SectionName, "UDPPort", -1);
    }

    public override void Save(FileSystemPath filePath)
    {
        WriteString(filePath, SectionName, "GLI_Mode", GLI_Mode);
        WriteString(filePath, SectionName, "TexturesFile", TexturesFile);
        WriteInt(filePath, SectionName, "TexturesCompressed", TexturesCompressed);
        WriteInt(filePath, SectionName, "TnL", TnL);
        WriteInt(filePath, SectionName, "TriLinear", TriLinear);

        WriteString(filePath, SectionName, "Language", Language);

        WriteInt(filePath, SectionName, "Video_WantedQuality", Video_WantedQuality);
        WriteInt(filePath, SectionName, "Video_BPP", Video_BPP);
        WriteInt(filePath, SectionName, "Video_AutoAdjustQuality", Video_AutoAdjustQuality);

        WriteInt(filePath, SectionName, "ModemQuality", ModemQuality);
        WriteInt(filePath, SectionName, "UDPPort", UDPPort);
    }
}