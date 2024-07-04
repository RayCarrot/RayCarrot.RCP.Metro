namespace RayCarrot.RCP.Metro.Ini;

public abstract class IniAppData
{
    protected IniAppData(FileSystemPath filePath)
    {
        FilePath = filePath;
    }

    public FileSystemPath FilePath { get; }

    protected string GetString(string sectionName, string keyName) =>
        GetString(sectionName, keyName, String.Empty);
    protected string GetString(string sectionName, string keyName, string defaultValue)
    {
        return IniNative.GetString(FilePath, sectionName, keyName, defaultValue);
    }

    protected int GetInt(string sectionName, string keyName) =>
        GetInt(sectionName, keyName, 0);
    protected int GetInt(string sectionName, string keyName, int defaultValue)
    {
        return IniNative.GetInt(FilePath, sectionName, keyName, defaultValue);
    }

    protected void WriteString(string sectionName, string keyName, string value)
    {
        IniNative.WriteString(FilePath, sectionName, keyName, value);
    }

    protected void WriteInt(string sectionName, string keyName, int value)
    {
        IniNative.WriteString(FilePath, sectionName, keyName, value.ToString());
    }

    public abstract void Load();
    public abstract void Save();
}