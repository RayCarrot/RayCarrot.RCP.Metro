using System;
using System.Diagnostics;
using System.Windows;

namespace RayCarrot.RCP.Updater
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : BaseWindow
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ErrorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with error details
        /// </summary>
        /// <param name="debugMessage">A debug message explaining the reason for the shutdown</param>
        /// <param name="ex">The exception which caused the shutdown</param>
        public ErrorWindow(string debugMessage, Exception ex = null)
        {
            InitializeComponent();

            DebugMessage = debugMessage;
            ErrorException = ex;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The exception which caused the shutdown
        /// </summary>
        private Exception ErrorException { get; }

        /// <summary>
        /// A debug message explaining the reason for the shutdown
        /// </summary>
        private string DebugMessage { get; }

        #endregion

        #region Event Handlers

        private void ErrorInfoButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Debug message: {DebugMessage}{Environment.NewLine}{Environment.NewLine}Stage: {App.Current.Stage}{Environment.NewLine}{Environment.NewLine}{ErrorException}");
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("http://raycarrot.ylemnova.com/")?.Dispose();
            Close();
        }

        #endregion
    }
}