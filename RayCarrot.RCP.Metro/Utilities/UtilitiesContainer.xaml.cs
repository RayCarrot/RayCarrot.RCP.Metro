using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
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
            Loaded += UtilitiesContainer_Loaded;
        }

        #endregion

        #region Event Handlers

        private void UtilitiesContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);

            if (win == null)
                return;

            win.Closing += (ss, ee) => Utilities?.DisposeAll();
            Loaded -= UtilitiesContainer_Loaded;
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
}