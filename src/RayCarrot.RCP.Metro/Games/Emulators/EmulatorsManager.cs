using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using RayCarrot.RCP.Metro.Games.Emulators.DosBox;

namespace RayCarrot.RCP.Metro.Games.Emulators;

public class EmulatorsManager
{
    #region Constructor

    public EmulatorsManager(AppUserData data, IMessenger messenger)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        EmulatorDescriptors = new EmulatorDescriptor[]
        {
            new DosBoxEmulatorDescriptor(),
        }.ToDictionary(x => x.EmulatorId);
        SortedEmulatorDescriptors = EmulatorDescriptors.Values.OrderBy(x => x).ToArray();
    }

    #endregion

    // TODO-14: Add logging

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Services

    private AppUserData Data { get; }
    private IMessenger Messenger { get; }

    #endregion

    #region Private Properties

    private Dictionary<string, EmulatorDescriptor> EmulatorDescriptors { get; }
    private EmulatorDescriptor[] SortedEmulatorDescriptors { get; }

    #endregion

    #region Emulator Descriptor Methods

    /// <summary>
    /// Gets the available emulator descriptors
    /// </summary>
    /// <returns>The emulator descriptors</returns>
    public IReadOnlyList<EmulatorDescriptor> GetEmulatorDescriptors() => SortedEmulatorDescriptors;

    /// <summary>
    /// Gets an emulator descriptor from the id
    /// </summary>
    /// <param name="emulatorId">The emulator id</param>
    /// <returns>The matching emulator descriptor</returns>
    public EmulatorDescriptor GetEmulatorDescriptor(string emulatorId)
    {
        if (emulatorId == null)
            throw new ArgumentNullException(nameof(emulatorId));

        if (!EmulatorDescriptors.TryGetValue(emulatorId, out EmulatorDescriptor descriptor))
            throw new ArgumentException($"No emulator descriptor found for the provided emulator id {emulatorId}", nameof(emulatorId));

        return descriptor;
    }

    #endregion

    #region Emulator Installation Methods

    public EmulatorInstallation AddEmulator(EmulatorDescriptor descriptor, FileSystemPath installLocation)
    {
        EmulatorInstallation installation = new(descriptor, installLocation);

        Data.Game_EmulatorInstallations.AddSorted(installation);

        // TODO-14: If an emulated game has no emulator selected, default to this if it matches?

        Messenger.Send(new AddedEmulatorsMessage(installation));

        return installation;
    }

    public void RemoveEmulator(EmulatorInstallation emulatorInstallation)
    {
        Data.Game_EmulatorInstallations.Remove(emulatorInstallation);

        // TODO-14: Deselect this from any game which uses this emulator

        Messenger.Send(new RemovedEmulatorsMessage(emulatorInstallation));
    }

    /// <summary>
    /// Gets a collection of the currently installed emulators
    /// </summary>
    /// <returns>The emulator installations</returns>
    public IReadOnlyList<EmulatorInstallation> GetInstalledEmulators()
    {
        // Copy to a list to avoid issues with it being modified when enumerating
        return Data.Game_EmulatorInstallations.ToList();
    }

    /// <summary>
    /// Gets an emulator installation from the installation id
    /// </summary>
    /// <param name="installationId">The emulator installation id</param>
    /// <returns>The matching emulator installation or null if not found</returns>
    public EmulatorInstallation? GetInstalledEmulator(string installationId)
    {
        if (installationId == null)
            throw new ArgumentNullException(nameof(installationId));

        return Data.Game_EmulatorInstallations.FirstOrDefault(x => x.InstallationId == installationId);
    }

    #endregion
}