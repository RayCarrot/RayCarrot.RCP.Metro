using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogViewer()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DataContext = App.Current.LogViewerViewModel;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public LogViewerViewModel ViewModel => DataContext as LogViewerViewModel;

        #endregion

        #region Public Static Methods

        public static void Open()
        {
            // TODO: Avoid multiple instances running at once

            var logViewer = new LogViewer();
            logViewer.Show();

            // Avoid the log viewer blocking the main window
            logViewer.Owner = null;
            logViewer.ShowInTaskbar = true;
        }

        #endregion

        #region Event Handlers

        private void LogViewer_Loaded(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.ScrollToBottom();

            // TODO: Unsubscribe when closing log viewer
            // Scroll to bottom when a new log is added
            ViewModel.LogItems.CollectionChanged += async (_, ee) =>
            {
                if (ee.Action == NotifyCollectionChangedAction.Add)
                    await Dispatcher.InvokeAsync(() => MainScrollViewer.ScrollToBottom());
            };
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.LogItems.Where(x => x.IsVisible).Select(x => x.LogMessage).JoinItems(Environment.NewLine));
        }

        #endregion
    }
}