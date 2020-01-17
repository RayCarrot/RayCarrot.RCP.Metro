using System;
using System.Globalization;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A string wrapper which changes regenerates itself when the current culture changes
    /// </summary>
    public class LocalizedString : BaseViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="generator">The value generator</param>
        public LocalizedString(Func<string> generator)
        {
            // Set properties
            Generator = generator ?? throw new ArgumentNullException(nameof(generator));
            Value = generator();

            // Subscribe to when the culture changes
            RCFCore.Data.CultureChanged += Data_CultureChanged;
        }

        /// <summary>
        /// The value generator
        /// </summary>
        protected Func<string> Generator { get; }

        /// <summary>
        /// The current string value
        /// </summary>
        public string Value { get; protected set; }

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            Value = Generator();
        }

        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;
        }
    }
}