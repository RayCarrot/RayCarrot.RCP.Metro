using System;
using System.ComponentModel;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A <see cref="Window"/> with view model support
    /// </summary>
    /// <typeparam name="VM">The type of view model to use</typeparam>
    public class VMWindow<VM> : Window
        where VM : class, INotifyPropertyChanged, new()
    {
        #region Constructors

        public VMWindow()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            ViewModel = new VM();

            Loaded += VMUserControl_Loaded;
        }

        public VMWindow(VM viewModel)
        {
            ViewModel = viewModel;

            Loaded += VMUserControl_Loaded;
        }

        #endregion

        #region Protected Properties

        protected VM ViewModel
        {
            get => DataContext as VM;
            set => DataContext = value;
        }

        #endregion

        #region Event Handlers

        private void VMUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Closed += (ss, ee) => (ViewModel as IDisposable)?.Dispose();
        }

        #endregion
    }
}