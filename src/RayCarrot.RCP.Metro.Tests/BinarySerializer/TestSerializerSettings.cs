using System.Text;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Tests;

public class TestSerializerSettings : ISerializerSettings
{
    public Encoding DefaultStringEncoding => Encoding.UTF8;
    public Endian DefaultEndianness => Endian.Little;
    public bool CreateBackupOnWrite => false;
    public bool SavePointersForRelocation => false;
    public bool IgnoreCacheOnRead => false;
    public bool LogAlignIfNotNull => false;
    public PointerSize? LoggingPointerSize => PointerSize.Pointer32;
    public bool AutoInitReadMap => false;
    public bool AutoExportReadMap => false;
}