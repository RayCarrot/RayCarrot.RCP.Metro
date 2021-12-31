﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using BinarySerializer.Ray1;
using NLog;
using RayCarrot.IO;

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
                encoder: new PC_SaveEncoder(), 
                getFileFilter: () => new FileFilterItemCollection()
                {
                    new FileFilterItem("*.sav", "SAV"),
                    new FileFilterItem("*.cfg", "CFG"),
                }.CombineAll("SAV").ToString(),
                game: Games.Rayman1),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_TTSnaHeader)), 
                encoder: new TTSNADataEncoder(),
                getFileFilter: () => new FileFilterItemCollection()
                {
                    new FileFilterItem("*.sna", "SNA"),
                    new FileFilterItem("*.dsb", "DSB"),
                }.CombineAll("Tonic Trouble").ToString()),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_R2SnaHeader)), 
                encoder: new R2SNADataEncoder(),
                getFileFilter: () => new FileFilterItemCollection()
                {
                    new FileFilterItem("*.sna", "SNA"),
                    new FileFilterItem("*.dsb", "DSB"),
                }.CombineAll("Rayman 2").ToString(),
                game: Games.Rayman2),

            new Utility_Decoders_TypeViewModel(
                name: new ResourceLocString(nameof(Resources.Utilities_Decoder_R3SaveHeader)), 
                encoder: new R3SaveEncoder(),
                getFileFilter: () => new FileFilterItem("*.sav", "SAV").ToString(),
                game: Games.Rayman3),

            // TODO-UPDATE: Localize
            new Utility_Decoders_TypeViewModel(
                name: new ConstLocString("Rayman Raving Rabbids Save Files (.sav)"), 
                encoder: new RRR_SaveEncoder(),
                getFileFilter: () => new FileFilterItem("*.sav", "SAV").ToString(),
                game: Games.RaymanRavingRabbids),
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
            DefaultDirectory = SelectedType.Game?.GetInstallDir(false).FullPath,
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
            DefaultDirectory = SelectedType.Game?.GetInstallDir(false).FullPath,
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