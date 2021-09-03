using RayCarrot.UI;
using System;
using System.Globalization;

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
            CultureChangedWeakEventManager.AddHandler(Services.InstanceData, Data_CultureChanged);
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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Value;
        }

        public void Dispose()
        {
            CultureChangedWeakEventManager.RemoveHandler(Services.InstanceData, Data_CultureChanged);
        }
    }
}