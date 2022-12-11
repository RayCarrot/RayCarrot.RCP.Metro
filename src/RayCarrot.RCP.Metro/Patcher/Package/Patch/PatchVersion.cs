namespace RayCarrot.RCP.Metro.Patcher;

public class PatchVersion : ICloneable, IComparable, IComparable<PatchVersion>, IEquatable<PatchVersion>
{
    public PatchVersion(int major, int minor, int revision)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Revision { get; }

    public object Clone() => new PatchVersion(Major, Minor, Revision);

    public int CompareTo(object? obj)
    {
        if (obj == null)
            return 1;

        if (obj is not PatchVersion version)
            throw new ArgumentException($"The argument must be of type {nameof(PatchVersion)}");

        return CompareTo(version);
    }

    public int CompareTo(PatchVersion? version)
    {
        if (version == null)
            return 1;

        if (Major != version.Major)
        {
            if (Major > version.Major)
                return 1;

            return -1;
        }

        if (Minor != version.Minor)
        {
            if (Minor > version.Minor)
                return 1;

            return -1;
        }

        if (Revision != version.Revision)
        {
            if (Revision > version.Revision)
                return 1;

            return -1;
        }

        return 0;
    }

    public override bool Equals(object? obj)
    {
        PatchVersion? version = obj as PatchVersion;

        if (version == null)
            return false;

        return Major == version.Major && Minor == version.Minor && Revision == version.Revision;
    }

    public bool Equals(PatchVersion? obj)
    {
        if (obj == null)
            return false;

        return Major == obj.Major && Minor == obj.Minor && Revision == obj.Revision;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Major;
            hashCode = (hashCode * 397) ^ Minor;
            hashCode = (hashCode * 397) ^ Revision;
            return hashCode;
        }
    }

    public override string ToString() => $"{Major}.{Minor}.{Revision}";

    public static bool operator ==(PatchVersion? v1, PatchVersion? v2)
    {
        return v1?.Equals(v2) ?? v2 is null;
    }

    public static bool operator !=(PatchVersion? v1, PatchVersion? v2)
    {
        return !(v1 == v2);
    }

    public static bool operator <(PatchVersion? v1, PatchVersion? v2)
    {
        if (v1 is null)
            throw new ArgumentNullException(nameof(v1));

        return v1.CompareTo(v2) < 0;
    }

    public static bool operator <=(PatchVersion? v1, PatchVersion? v2)
    {
        if (v1 is null)
            throw new ArgumentNullException(nameof(v1));

        return v1.CompareTo(v2) <= 0;
    }

    public static bool operator >(PatchVersion? v1, PatchVersion? v2)
    {
        return v2 < v1;
    }

    public static bool operator >=(PatchVersion? v1, PatchVersion? v2)
    {
        return v2 <= v1;
    }
}