using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RayCarrot.RCP.Updater
{
    /// <summary>
    /// A base view model
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The current application
        /// </summary>
        public App App => App.Current;
    }
}