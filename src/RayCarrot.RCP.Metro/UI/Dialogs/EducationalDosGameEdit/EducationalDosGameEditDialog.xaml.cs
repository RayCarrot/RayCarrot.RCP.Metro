#nullable disable
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for EducationalDosGameEditDialog.xaml
/// </summary>
public partial class EducationalDosGameEditDialog : WindowContentControl, IDialogWindowControl<EducationalDosGameEditViewModel, EducationalDosGameEditResult>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="vm">The view model</param>
    public EducationalDosGameEditDialog(EducationalDosGameEditViewModel vm)
    {
        InitializeComponent();
        ViewModel = vm;
        DataContext = ViewModel;
        CanceledByUser = true;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the dialog was canceled by the user, default is true
    /// </summary>
    public bool CanceledByUser { get; set; }

    /// <summary>
    /// The view model
    /// </summary>
    public EducationalDosGameEditViewModel ViewModel { get; }

    #endregion

    #region Protected Methods

    protected override void WindowAttached()
    {
        base.WindowAttached();

        WindowInstance.Icon = GenericIconKind.Window_EducationalDosGameEdit;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current result for the dialog
    /// </summary>
    /// <returns>The result</returns>
    public EducationalDosGameEditResult GetResult()
    {
        return new EducationalDosGameEditResult()
        {
            CanceledByUser = CanceledByUser,
            Name = ViewModel.Name,
            LaunchMode = ViewModel.LaunchMode,
            MountPath = ViewModel.MountPath
        };
    }

    #endregion

    #region Event Handlers

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        CanceledByUser = false;

        // Close the dialog
        WindowInstance.Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Close the dialog
        WindowInstance.Close();
    }

    #endregion
}