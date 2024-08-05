using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Components;

public class SetupRayman2DemoAppDataOnGameAddedComponent : OnGameAddedComponent
{
    public SetupRayman2DemoAppDataOnGameAddedComponent() : base(SetupRayman2DemoAppData) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static void SetupRayman2DemoAppData(GameInstallation gameInstallation)
    {
        try
        {
            UbisoftIniAppDataManager<Rayman2IniAppData> appDataManager = new(AppFilePaths.UbiIniPath);

            appDataManager.Load();

            if (appDataManager.AppData.GLI_DllFile == String.Empty &&
                appDataManager.AppData.GLI_Dll == String.Empty &&
                appDataManager.AppData.GLI_Driver == String.Empty &&
                appDataManager.AppData.GLI_Device == String.Empty)
            {
                appDataManager.AppData.GLI_DllFile = "GliDX6";
                appDataManager.AppData.GLI_Dll = "DirectX6";
                appDataManager.AppData.GLI_Driver = "display";
                appDataManager.AppData.GLI_Device = "Direct3D HAL";
            }

            if (appDataManager.AppData.GLI_Mode == String.Empty)
                appDataManager.AppData.GLI_Mode = "1 - 800 x 600 x 16";

            if (appDataManager.AppData.Language == String.Empty)
                appDataManager.AppData.Language = "English";

            if (appDataManager.AppData.ParticuleRate == String.Empty)
                appDataManager.AppData.ParticuleRate = "High";

            appDataManager.Save();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Setting up Rayman 2 demo app data");
        }
    }
}