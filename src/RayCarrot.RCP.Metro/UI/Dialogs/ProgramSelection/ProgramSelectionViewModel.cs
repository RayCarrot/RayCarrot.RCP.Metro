using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a string input dialog
/// </summary>
public class ProgramSelectionViewModel : UserInputViewModel
{
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

    /// <summary>
    /// The available programs
    /// </summary>
    public ObservableCollection<ProgramSelectionItemViewModel>? Programs { get; set; }

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

    public async Task LoadProgramsAsync()
    {
        List<ProgramSelectionItemViewModel> programs = new();

        FileSystemPath[] paths = 
        {
            Environment.SpecialFolder.StartMenu.GetFolderPath(),
            Environment.SpecialFolder.CommonStartMenu.GetFolderPath()
        };

        // Get programs
        foreach (FileSystemPath path in paths)
        {
            // TODO-UPDATE: Try/catch
            foreach (FileSystemPath shortcutFile in Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories))
            {
                // Get the target
                FileSystemPath targetFile = WindowsHelpers.GetShortCutTarget(shortcutFile);

                // Make sure the file exists
                if (!targetFile.FileExists)
                    continue;

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

        // Sort
        Programs = new ObservableCollection<ProgramSelectionItemViewModel>(programs.OrderBy(x => x.Name));

        // Get icons
        await Task.Run(() =>
        {
            foreach (ProgramSelectionItemViewModel program in Programs)
                program.LoadIcon();
        });
    }
}