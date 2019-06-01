using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RayCarrot.CarrotFramework;
using RayCarrot.UserData;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : BaseWindow
    {
        #region Constructor

        public LogViewer()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DataContext = new LogViewerViewModel();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public LogViewerViewModel ViewModel => DataContext as LogViewerViewModel;

        #endregion

        #region Event Handlers

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.ScrollToBottom();

            // Scroll to bottom when a new log is added
            SessionLogger.LogAdded += (ss, ee) => Dispatcher.Invoke(() => MainScrollViewer.ScrollToBottom());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.DisplayLog.Select(x => x.Message).JoinItems(Environment.NewLine));
        }

        #endregion
    }
}