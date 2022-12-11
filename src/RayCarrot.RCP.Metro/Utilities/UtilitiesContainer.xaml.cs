#nullable disable
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for UtilitiesContainer.xaml
/// </summary>
public partial class UtilitiesContainer : UserControl
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public UtilitiesContainer()
    {
        InitializeComponent();
        DataContextRoot.DataContext = this;
    }

    #endregion

    #region Dependency Properties

    public IEnumerable<UtilityViewModel> Utilities
    {
        get => (IEnumerable<UtilityViewModel>)GetValue(UtilitiesProperty);
        set => SetValue(UtilitiesProperty, value);
    }

    public static readonly DependencyProperty UtilitiesProperty = DependencyProperty.Register(nameof(Utilities), typeof(IEnumerable<UtilityViewModel>), typeof(UtilitiesContainer));

    #endregion
}