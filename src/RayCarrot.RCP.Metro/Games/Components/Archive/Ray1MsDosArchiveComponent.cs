using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(BinaryGameModeComponent))]
public class Ray1MsDosArchiveComponent : ArchiveComponent
{
    public Ray1MsDosArchiveComponent() : base(GetArchiveManager, GetArchiveFilePaths, Id) { }

    public new const string Id = "RAY1_DAT";

    private static IArchiveDataManager GetArchiveManager(GameInstallation gameInstallation)
    {
        Ray1Settings ray1Settings = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, Ray1GameModeComponent>().
            GetSettings();

        return new Ray1PCArchiveDataManager(ray1Settings);
    }

    private static IEnumerable<string> GetArchiveFilePaths(GameInstallation gameInstallation)
    {
        Ray1Settings ray1Settings = gameInstallation.
            GetRequiredComponent<BinaryGameModeComponent, Ray1GameModeComponent>().
            GetSettings();

        if (ray1Settings.EngineVersion == Ray1EngineVersion.PC)
            throw new Exception("Getting archive paths for Rayman 1 PC is currently not supported");

        bool isEdu = ray1Settings.EngineVersion == Ray1EngineVersion.PC_Edu;

        yield return @"PCMAP\COMMON.DAT";
        yield return @"PCMAP\SNDD8B.DAT";
        yield return @"PCMAP\SNDH8B.DAT";

        if (!isEdu)
            yield return @"PCMAP\VIGNET.DAT";

        Ray1MsDosData? data = gameInstallation.GetObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);

        if (data == null) 
            yield break;
        
        foreach (Ray1MsDosData.Version version in data.AvailableVersions)
        {
            yield return $@"PCMAP\{version.Id}\SNDSMP.DAT";
            yield return $@"PCMAP\{version.Id}\SPECIAL.DAT";

            if (isEdu)
                yield return $@"PCMAP\{version.Id}\VIGNET.DAT";
        }
    }
}