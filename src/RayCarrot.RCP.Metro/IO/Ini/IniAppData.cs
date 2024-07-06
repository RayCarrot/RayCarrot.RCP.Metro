namespace RayCarrot.RCP.Metro.Ini;

public abstract class IniAppData
{
    protected string GetString(FileSystemPath filePath, string sectionName, string keyName) =>
        GetString(filePath, sectionName, keyName, String.Empty);
    protected string GetString(FileSystemPath filePath, string sectionName, string keyName, string defaultValue)
    {
        return IniNative.GetString(filePath, sectionName, keyName, defaultValue);
    }

    protected int GetInt(FileSystemPath filePath, string sectionName, string keyName) =>
        GetInt(filePath, sectionName, keyName, 0);
    protected int GetInt(FileSystemPath filePath, string sectionName, string keyName, int defaultValue)
    {
        return IniNative.GetInt(filePath, sectionName, keyName, defaultValue);
    }

    protected void WriteString(FileSystemPath filePath, string sectionName, string keyName, string value)
    {
        IniNative.WriteString(filePath, sectionName, keyName, value);
    }

    protected void WriteInt(FileSystemPath filePath, string sectionName, string keyName, int value)
    {
        IniNative.WriteString(filePath, sectionName, keyName, value.ToString());
    }

    public abstract void Load(FileSystemPath filePath);
    public abstract void Save(FileSystemPath filePath);
}