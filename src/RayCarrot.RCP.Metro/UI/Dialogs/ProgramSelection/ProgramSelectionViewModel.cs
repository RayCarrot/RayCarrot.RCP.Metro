using System.IO;
using Microsoft.Win32;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a string input dialog
/// </summary>
public class ProgramSelectionViewModel : UserInputViewModel
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private FileSystemPath _programFilePath;
    private ProgramSelectionItemViewModel? _selectedProgram;

    /// <summary>
    /// The program file path
    /// </summary>
    public FileSystemPath ProgramFilePath
    {
        get => _programFilePath;
        set
        {
            _programFilePath = value;
            IsValid = ProgramFilePath.FileExists && ProgramFilePath.FileExtension == new FileExtension(".exe");
        }
    }

    public FileExtension[]? FileExtensions { get; set; }

    /// <summary>
    /// The available programs
    /// </summary>
    public ObservableCollection<ProgramSelectionItemViewModel?>? Programs { get; set; }

    /// <summary>
    /// The currently selected program
    /// </summary>
    public ProgramSelectionItemViewModel? SelectedProgram
    {
        get => _selectedProgram;
        set
        {
            _selectedProgram = value;

            if (value != null)
                ProgramFilePath = value.FilePath;
        }
    }

    public bool IsValid { get; set; }

    private static IEnumerable<string> GetAssociatedPrograms(string ext)
    {
        // Find the associated programs names
        List<string> names = new();

        using (RegistryKey? key = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\OpenWithList"))
        {
            if (key?.GetValue("MRUList") is string mruList)
            {
                foreach (char c in mruList)
                {
                    string name = key.GetValue(c.ToString()).ToString();

                    if (name.ToLower().Contains(".exe"))
                        names.Add(name);
                }
            }
        }

        if (!names.Any())
            return Array.Empty<string>();

        // Get the paths for each name
        List<string> paths = new();

        void addPathsFrom(RegistryKey baseKey)
        {
            foreach (string name in names)
            {
                RegistryKey? key = baseKey.OpenSubKey($@"Software\Classes\Applications\{name}\shell\open\command");

                try
                {
                    // Try get the edit command if the open one is null
                    key ??= baseKey.OpenSubKey($@"Software\Classes\Applications\{name}\shell\edit\command");

                    if (key?.GetValue(String.Empty) is not string path)
                        continue;

                    if (path.IsNullOrWhiteSpace())
                        continue;

                    bool hasQuotes = path[0] == '\"';

                    if (hasQuotes)
                    {
                        // Attempt to get the path by removing the quotes
                        int secondQuoteIndex = path.IndexOf("\"", 2, StringComparison.InvariantCulture);

                        if (secondQuoteIndex == -1)
                            continue;

                        path = path.Substring(1, secondQuoteIndex - 1);
                    }
                    else
                    {
                        int whiteSpaceIndex = path.IndexOf(' ');

                        if (whiteSpaceIndex != -1)
                            path = path.Substring(0, whiteSpaceIndex);
                    }

                    // Add if it exists
                    if (File.Exists(path))
                        paths.Add(path);
                }
                finally
                {
                    key?.Dispose();
                }
            }
        }

        addPathsFrom(Registry.CurrentUser);
        addPathsFrom(Registry.LocalMachine);

        return paths.ToArray();
    }

    public async Task LoadProgramsAsync()
    {
        List<ProgramSelectionItemViewModel?> programs = new();

        // Get associated programs and add those first
        if (FileExtensions != null)
        {
            try
            {
                foreach (FileExtension ext in FileExtensions)
                {
                    foreach (FileSystemPath program in GetAssociatedPrograms(ext.PrimaryFileExtension))
                    {
                        // Add the program if it doesn't exist
                        if (programs.All(x => x?.FilePath != program))
                            programs.Add(new ProgramSelectionItemViewModel(program.RemoveFileExtension().Name, program)
                            {
                                IsRecommended = true,
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Getting associated programs");
            }

            if (programs.Any())
                programs.Add(null);
        }

        FileSystemPath[] paths = 
        {
            Environment.SpecialFolder.StartMenu.GetFolderPath(),
            Environment.SpecialFolder.CommonStartMenu.GetFolderPath()
        };

        // Get programs
        foreach (FileSystemPath path in paths)
        {
            try
            {
                foreach (FileSystemPath shortcutFile in Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories))
                {
                    // Get the target
                    FileSystemPath targetFile = WindowsHelpers.GetShortCutTarget(shortcutFile);

                    // Make sure the file exists
                    if (!targetFile.FileExists)
                    {
                        // This was a workaround which was needed when RCP was being run as a 32-bit program due to the shortcut path
                        // being incorrect when on a 64-bit system
                        /*
                        FileSystemPath workingDir = WindowsHelpers.GetShortCutTargetInfo(shortcutFile).WorkingDirectory;
                        targetFile = workingDir + targetFile.Name;
                        */

                        if (!targetFile.FileExists)
                            continue;
                    }

                    // Ignore if not an exe file
                    if (!targetFile.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    // Ignore uninstall files
                    if (targetFile.Name.IndexOf("uninstall", StringComparison.InvariantCultureIgnoreCase) != -1)
                        continue;

                    // Add the program
                    programs.Add(new ProgramSelectionItemViewModel(shortcutFile.RemoveFileExtension().Name, targetFile));
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Getting start menu programs");
            }
        }

        // Sort
        Programs = new ObservableCollection<ProgramSelectionItemViewModel?>(programs.
            OrderBy(x => x?.IsRecommended switch
            {
                true => 0,
                null => 1,
                false => 2,
            }).
            ThenBy(x => x?.Name));

        // Get icons
        await Task.Run(() =>
        {
            foreach (ProgramSelectionItemViewModel? program in Programs)
                program?.LoadIcon();
        });
    }
}