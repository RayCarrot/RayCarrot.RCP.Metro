using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            const string name = "LoggerConsole: ";

            try
            {
                if (e.Key != Key.Return)
                    return;

                var tb = sender.CastTo<TextBox>();

                // Get the contents
                var text = tb.Text;

                // Clear the text
                tb.Clear();

                if (text.IsNullOrWhiteSpace())
                    return;

                var commands = text.Split(' ');

                if (commands[0].Equals("get"))
                {
                    var service = GetService(commands[1]);

                    if (service == null)
                        return;

                    var prop = service.GetType().GetProperty(commands[2]);

                    if (prop == null)
                    {
                        RCF.Logger.LogWarningSource($"{name}The property name was not found");
                        return;
                    }

                    RCF.Logger.LogInformationSource(name + prop.GetValue(service));
                }
                else if (commands[0].Equals("get-ud"))
                {
                    var service = RCFData.UserDataCollection.GetUserData(commands[1]);

                    if (service == null)
                        return;

                    var prop = service.GetType().GetProperty(commands[2]);

                    if (prop == null)
                    {
                        RCF.Logger.LogWarningSource($"{name}The property name was not found");
                        return;
                    }

                    RCF.Logger.LogInformationSource(name + prop.GetValue(service));
                }

                object GetService(string typeName)
                {
                    var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.DefinedTypes).FindItem(x => x.FullName == typeName)?.AsType();

                    if (type == null)
                    {
                        RCF.Logger.LogWarningSource($"{name}The type was not found");
                        return null;
                    }

                    var service = RCF.Provider.GetService(type);

                    if (service == null)
                    {
                        RCF.Logger.LogWarningSource($"{name}The service was not found");
                        return null;
                    }

                    return service;
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Log viewer console");
            }
        }

        #endregion
    }
}