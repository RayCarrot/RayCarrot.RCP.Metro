using System.IO;
using System.Windows.Input;
using BinarySerializer.UbiArt;

// ReSharper disable StringLiteralTypo

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the Rayman Legends UbiRay utility
/// </summary>
public class Utility_RaymanLegends_UbiRay_ViewModel : BaseRCPViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_RaymanLegends_UbiRay_ViewModel(GameInstallation gameInstallation)
    {
        // Create commands
        ApplyCommand = new AsyncRelayCommand(ApplyAsync);
        RevertCommand = new AsyncRelayCommand(RevertAsync);

        // Set properties
        IPKFilePath = gameInstallation.InstallLocation + "Bundle_PC.ipk";

        try
        {
            // Open the IPK file
            using var fileStream = File.OpenRead(IPKFilePath);

            // Get the first patch info
            var patchInfo = GetPatchInfos(fileStream, IPKFilePath.Name).First();

            // Set the position
            fileStream.Position = patchInfo.Offset;

            // Get the bytes from that position to see if the patch has been applied
            byte[] buffer = new byte[patchInfo.Patch.OriginalBytes.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            IsApplied = !buffer.SequenceEqual(patchInfo.Patch.OriginalBytes);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting if UbiRay patch has been applied");

            IPKFilePath = FileSystemPath.EmptyPath;
        }
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ApplyCommand { get; }
    public ICommand RevertCommand { get; }

    #endregion

    #region Protected Properties

    /// <summary>
    /// Gets the patches
    /// </summary>
    protected Patch[] GetPatches => new Patch[]
    {
        // This makes UbiRay be treated as a normal costume (type 0)
        new Patch("sgscontainer.ckd", 17841, new byte[] { 0 }, new byte[] { 1 }, 30613), 

        // This fixes the character description
        new Patch("costumerayman_ubi.act.ckd", 414, GetBytes(6426), GetBytes(5095), 578), 

        // This fixes the character name
        new Patch("costumerayman_ubi.act.ckd", 418, GetBytes(6425), GetBytes(4876), 578), 
    };

    #endregion

    #region Public Properties

    /// <summary>
    /// The IPK file path
    /// </summary>
    public FileSystemPath IPKFilePath { get; set; }

    /// <summary>
    /// Indicates if the utility has been applied
    /// </summary>
    public bool IsApplied { get; set; }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Gets the bytes from a 32-bit integer using big endian
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The bytes</returns>
    protected byte[] GetBytes(int value) => BitConverter.GetBytes(value).Reverse().ToArray();

    /// <summary>
    /// Gets the patch infos for each <see cref="Patch"/>
    /// </summary>
    /// <param name="ipkStream">The IPK file stream</param>
    /// <param name="name">The IPK name</param>
    /// <returns>The patch infos</returns>
    protected IEnumerable<PatchInfo> GetPatchInfos(Stream ipkStream, string name)
    {
        // Deserialize the IPK file
        using RCPContext context = new(String.Empty);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanLegends, Platform.PC);
        context.AddSettings(settings);
        BundleFile ipk = context.ReadStreamData<BundleFile>(ipkStream, name: name, endian: settings.GetEndian, leaveOpen: true);

        // Enumerate every patch
        foreach (var patchGroup in GetPatches.GroupBy(x => x.FileName))
        {
            // Get the file
            BundleFile_FileEntry? file = ipk.FilePack.Files.FirstOrDefault(x => x.Path.FileName == patchGroup.Key);

            // Make sure we found the file
            if (file == null)
                throw new Exception("Patch file not found");

            // Make sure it's not compressed
            if (file.IsCompressed)
                throw new Exception("The configuration file is compressed and can not be edited");

            // Make sure the file size matches
            if (file.FileSize != patchGroup.First().FileSize)
                throw new Exception($"File size for {file.Path} doesn't match the expected size");

            // Get the offsets
            foreach (Patch patch in patchGroup)
            {
                // Get the offset of the byte in the file to change
                yield return new PatchInfo(patch, (long)(ipk.BootHeader.BaseOffset + file.Offsets.First() + (uint)patch.FileOffset));
            }
        }
    }

    /// <summary>
    /// Patches the IPK file
    /// </summary>
    /// <param name="usePatchedBytes">Indicates if the patched bytes should be used or if else the original ones should be used</param>
    protected void PatchFile(bool usePatchedBytes)
    {
        // Open the file
        using var fileStream = File.Open(IPKFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

        // Enumerate each patch info
        foreach (PatchInfo patchInfo in GetPatchInfos(fileStream, IPKFilePath.Name))
        {
            // Set the position
            fileStream.Position = patchInfo.Offset;

            byte[] bytes = usePatchedBytes ? patchInfo.Patch.PatchedBytes : patchInfo.Patch.OriginalBytes;

            // Modify the bytes
            fileStream.Write(bytes, 0, bytes.Length);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Applies the patch
    /// </summary>
    /// <returns>The task</returns>
    public async Task ApplyAsync()
    {
        try
        {
            // Apply the patch
            PatchFile(true);

            Logger.Info("The Rayman Legends UbiRay utility has been applied");

            IsApplied = true;

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RLU_UbiRay_ApplySuccess);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Applying RL UbiRay patch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex);
        }
    }

    /// <summary>
    /// Reverts the patch
    /// </summary>
    /// <returns>The task</returns>
    public async Task RevertAsync()
    {
        try
        {
            // Apply the patch
            PatchFile(false);

            Logger.Info("The Rayman Legends UbiRay utility has been reverted");

            IsApplied = false;

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.RLU_UbiRay_RevertSuccess);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reverting RL UbiRay patch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex);
        }
    }

    #endregion

    #region Records

    protected record PatchInfo(Patch Patch, long Offset);
    protected record Patch(string FileName, int FileOffset, byte[] PatchedBytes, byte[] OriginalBytes, long FileSize);

    #endregion
}