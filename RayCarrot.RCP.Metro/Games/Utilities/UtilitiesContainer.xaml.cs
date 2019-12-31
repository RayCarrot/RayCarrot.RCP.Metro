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

            win.Closing += (ss, ee) => Utilities.ForEach(x => x.Dispose());
            Loaded -= UtilitiesContainer_Loaded;
        }

        #endregion

        #region Dependency Properties

        public IEnumerable<RCPUtilityViewModel> Utilities
        {
            get => (IEnumerable<RCPUtilityViewModel>)GetValue(UtilitiesProperty);
            set => SetValue(UtilitiesProperty, value);
        }

        public static readonly DependencyProperty UtilitiesProperty = DependencyProperty.Register(nameof(Utilities), typeof(IEnumerable<RCPUtilityViewModel>), typeof(UtilitiesContainer));

        #endregion
    }
}