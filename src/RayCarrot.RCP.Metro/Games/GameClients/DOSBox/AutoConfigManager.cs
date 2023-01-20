using System.IO;
using System.Text;
using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

/// <summary>
/// Manager for auto DOSBox config file
/// </summary>
public class AutoConfigManager
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="path">The config file path</param>
    public AutoConfigManager(FileSystemPath path)
    {
        FilePath = path;
        FirstLines = $"[ipx]{Environment.NewLine}# ipx -- Enable ipx over UDP/IP emulation.{Environment.NewLine}ipx=false{Environment.NewLine}{Environment.NewLine}[autoexec]{Environment.NewLine}@echo off";
    }

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// The first lines of the config file
    /// </summary>
    private string FirstLines { get; }

    /// <summary>
    /// The config file path
    /// </summary>
    private FileSystemPath FilePath { get; }

    /// <summary>
    /// The line separator for the custom commands
    /// </summary>
    private const string CustomCommandsSeparator = "#Custom commands:";

    /// <summary>
    /// The start of a config file
    /// </summary>
    private const string ConfigLineStart = "CONFIG -set ";

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates the configuration file if it does not already exist with a valid configuration
    /// </summary>
    /// <param name="gameInstallation">The game installation for which to create the file</param>
    public void Create(GameInstallation gameInstallation)
    {
        Logger.Trace("Recreating DosBox config file {0}", FilePath);

        // Add this file as a created one to the game installation
        gameInstallation.ModifyObject<AddedGameFiles>(GameDataKey.RCP_AddedFiles, x => x.Files.Add(FilePath));

        // Check if the file exists and is valid
        if (FilePath.FileExists && File.ReadAllText(FilePath).StartsWith(FirstLines))
        {
            Logger.Trace("The DosBox config file is valid and no further action is needed");
            return;
        }

        Directory.CreateDirectory(FilePath.Parent);

        // Create the file with default content
        File.WriteAllText(FilePath, FirstLines);

        Logger.Info("The DosBox config file was recreated");
    }

    /// <summary>
    /// Reads the file and returns the data
    /// </summary>
    /// <returns>The file data</returns>
    public AutoConfigData ReadFile()
    {
        // Read the file content
        string content = File.ReadAllText(FilePath);

        // Remove the first lines
        content = content.Substring(FirstLines.Length);

        // Split into lines
        string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // Create the output
        AutoConfigData output = new();

        // Keep track of if we've reached the custom commands
        bool inCustomCommands = false;

        // Keep track if we're in a section
        bool inSection = false;

        // Check each line
        foreach (string line in lines)
        {
            // Flag when the custom commands are reached
            if (!inCustomCommands && line == CustomCommandsSeparator)
            {
                inCustomCommands = true;
                continue;
            }

            // If we've reached the custom commands, check if the line has a section name
            if (!inSection && inCustomCommands && line.StartsWith("["))
            {
                inSection = true;
                continue;
            }

            // Ignore section names
            if (line.StartsWith("["))
                continue;

            // Ignore if it's empty
            if (line.IsNullOrWhiteSpace())
                continue;

            if (inSection)
            {
                // Get the values
                string[] values = line.Split('=');

                // Make sure we have two values
                if (values.Length != 2)
                    continue;

                // Add the values
                output.Configuration.Add(values[0], values[1]);
            }
            else if (inCustomCommands)
            {
                output.CustomLines.Add(line);
            }
            else
            {
                // Ignore if it's not a valid config line
                if (!line.StartsWith(ConfigLineStart))
                    continue;

                // Get the values
                string[] values = line.Substring(ConfigLineStart.Length).Trim('\"').Split('=');

                // Make sure we have two values
                if (values.Length != 2)
                    continue;

                // Add the values
                output.Configuration.Add(values[0], values[1]);
            }
        }

        return output;
    }

    /// <summary>
    /// Writes the specified data to the file
    /// </summary>
    /// <param name="data">The data to write</param>
    public void WriteFile(AutoConfigData data)
    {
        StringBuilder stringBuilder = new StringBuilder();

        // Add the first lines
        stringBuilder.AppendLine(FirstLines);

        // Add the custom commands separator
        stringBuilder.AppendLine(CustomCommandsSeparator);

        // Add the custom commands
        foreach (string customLine in data.CustomLines)
            stringBuilder.AppendLine(customLine);

        // Add the sections
        foreach (var sections in data.SectionNames)
        {
            // Add the section name
            stringBuilder.AppendLine($"[{sections.Key}]");

            // Add each command
            foreach (var key in sections.Value)
            {
                // Make sure the key has been added
                if (data.Configuration.ContainsKey(key))
                    // Add the command
                    stringBuilder.AppendLine($"{key}={data.Configuration[key]}");
            }

            // Add empty line
            stringBuilder.AppendLine();
        }

        // Write the text to the file
        File.WriteAllText(FilePath, stringBuilder.ToString());
    }

    #endregion
}