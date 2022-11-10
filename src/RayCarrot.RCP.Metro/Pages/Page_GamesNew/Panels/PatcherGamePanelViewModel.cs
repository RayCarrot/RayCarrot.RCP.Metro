using System;
using System.Linq;
using System.Threading.Tasks;
using BinarySerializer;
using NLog;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class PatcherGamePanelViewModel : GamePanelViewModel
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GameDisplay_Patcher;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Patcher_Title));

    public LocalizedString? InfoText { get; set; }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsyncImpl(GameInstallation gameInstallation)
    {
        // Get applied patches
        using Context context = new RCPContext(String.Empty);
        PatchLibrary library = new(gameInstallation.InstallLocation, Services.File);
        PatchLibraryFile? libraryFile = null;

        try
        {
            libraryFile = await Task.Run(() => context.ReadFileData<PatchLibraryFile>(library.LibraryFilePath));
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading patch library");
            // TODO-UPDATE: Enter some sort of error state if this fails?
        }

        // TODO-UPDATE: Localize
        InfoText = $"{libraryFile?.Patches.Count(x => x.IsEnabled) ?? 0} patches applied";
    }

    #endregion
}