using System.IO;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1.PC;

namespace RayCarrot.RCP.Metro;

public class Utility_Decoders_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    public Utility_Decoders_ViewModel()
    {
        Types = new ObservableCollection<Utility_Decoders_TypeViewModel>()
        {
            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_R12SavHeader)), 
                encoder: new SaveEncoder(), 
                getFileFilter: () => new FileFilterItemCollection()
                {
                    new("*.sav", "SAV"),
                    new("*.cfg", "CFG"),
                }.CombineAll("SAV").ToString(),
                gameSearchPredicates: new[]
                {
                    GameSearch.Create(Game.Rayman1, GamePlatform.Win32),
                    GameSearch.Create(Game.Rayman2, GamePlatform.Win32),
                }),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_TTSnaHeader)), 
                encoder: new TTSNADataEncoder(),
                getFileFilter: () => new FileFilterItemCollection()
                {
                    new("*.sna", "SNA"),
                    new("*.dsb", "DSB"),
                }.CombineAll("Tonic Trouble").ToString(),
                gameSearchPredicates: GameSearch.Create(Game.TonicTrouble, GamePlatform.Win32)),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_R2SnaHeader)), 
                encoder: new R2SNADataEncoder(),
                getFileFilter: () => new FileFilterItemCollection()
                {
                    new("*.sna", "SNA"),
                    new("*.dsb", "DSB"),
                }.CombineAll("Rayman 2").ToString(),
                gameSearchPredicates: GameSearch.Create(Game.Rayman2, GamePlatform.Win32)),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_R3SaveHeader)), 
                encoder: new R3SaveEncoder(),
                getFileFilter: () => new FileFilterItem("*.sav", "SAV").ToString(),
                gameSearchPredicates: GameSearch.Create(Game.Rayman3, GamePlatform.Win32)),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Format_RRRSaveHeader)), 
                encoder: new RRR_SaveEncoder(),
                getFileFilter: () => new FileFilterItem("*.sav", "SAV").ToString(),
                gameSearchPredicates: GameSearch.Create(Game.RaymanRavingRabbids, GamePlatform.Win32)),
        };
        SelectedType = Types.First();

        DecodeCommand = new AsyncRelayCommand(DecodeAsync);
        EncodeCommand = new AsyncRelayCommand(EncodeAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands
    
    public ICommand DecodeCommand { get; }
    public ICommand EncodeCommand { get; }

    #endregion

    #region Public Properties

    public ObservableCollection<Utility_Decoders_TypeViewModel> Types { get; }
    public Utility_Decoders_TypeViewModel SelectedType { get; set; }

    public bool IsLoading { get; set; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Processes a file
    /// </summary>
    /// <param name="inputFiles">The input files to process</param>
    /// <param name="outputDir">The output directory</param>
    /// <param name="shouldDecode">True if the files should be decoded, or false to encode them</param>
    private void ProcessFile(IEnumerable<FileSystemPath> inputFiles, FileSystemPath outputDir, bool shouldDecode)
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            // Get the encoder
            IStreamEncoder encoder = SelectedType.Encoder;

            // Process every file
            foreach (FileSystemPath file in inputFiles)
            {
                // Open the input file
                using FileStream inputStream = File.OpenRead(file);

                // Open and create the destination file
                using FileStream outputStream = File.OpenWrite((outputDir + file.Name).GetNonExistingFileName());

                // Process the file data
                if (shouldDecode)
                    encoder.DecodeStream(inputStream, outputStream);
                else
                    encoder.EncodeStream(inputStream, outputStream);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Public Methods

    public async Task DecodeAsync()
    {
        // Allow the user to select the files
        FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = Resources.Utilities_Decoder_DecodeFileSelectionHeader,
            DefaultDirectory = Services.Games.FindInstalledGame(SelectedType.GameSearchPredicates)?.InstallLocation.Directory.FullPath,
            ExtensionFilter = SelectedType.GetFileFilter(),
            MultiSelection = true
        });

        if (fileResult.CanceledByUser)
            return;

        // Allow the user to select the destination directory
        DirectoryBrowserResult destinationResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = Resources.Browse_DestinationHeader,
        });

        if (destinationResult.CanceledByUser)
            return;

        try
        {
            // Process the files
            await Task.Run(() => ProcessFile(fileResult.SelectedFiles, destinationResult.SelectedDirectory, true));

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Decoder_DecodeSuccess);
        }
        catch (NotImplementedException ex)
        {
            Logger.Debug(ex, "Decoding files");

            await Services.MessageUI.DisplayMessageAsync(Resources.NotImplemented, MessageType.Error);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Decoding files");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Decoder_DecodeError);
        }
    }

    public async Task EncodeAsync()
    {
        // Allow the user to select the files
        FileBrowserResult fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = Resources.Utilities_Decoder_EncodeFileSelectionHeader,
            DefaultDirectory = Services.Games.FindInstalledGame(SelectedType.GameSearchPredicates)?.InstallLocation.Directory.FullPath,
            ExtensionFilter = SelectedType.GetFileFilter(),
            MultiSelection = true
        });

        if (fileResult.CanceledByUser)
            return;

        // Allow the user to select the destination directory
        DirectoryBrowserResult destinationResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = Resources.Browse_DestinationHeader,
        });

        if (destinationResult.CanceledByUser)
            return;

        try
        {
            // Process the files
            await Task.Run(() => ProcessFile(fileResult.SelectedFiles, destinationResult.SelectedDirectory, false));

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Decoder_EncodeSuccess);
        }
        catch (NotImplementedException ex)
        {
            Logger.Debug(ex, "Encoding files");

            await Services.MessageUI.DisplayMessageAsync(Resources.NotImplemented, MessageType.Error);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Encoding files");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Decoder_EncodeError);
        }
    }

    public void Dispose()
    {
        Types.DisposeAll();
    }

    #endregion
}