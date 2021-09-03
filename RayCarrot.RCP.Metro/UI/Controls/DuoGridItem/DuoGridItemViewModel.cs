﻿using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A duo grid item view model
    /// </summary>
    public class DuoGridItemViewModel : BaseViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="header">The header</param>
        /// <param name="text">The text to display</param>
        /// <param name="minUserLevel">The minimum user level for this item</param>
        public DuoGridItemViewModel(string header, string text, UserLevel minUserLevel = UserLevel.Normal)
        {
            Header = header;
            Text = text;
            MinUserLevel = minUserLevel;
        }

        /// <summary>
        /// The header
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// The text to display
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The minimum user level for this item
        /// </summary>
        public UserLevel MinUserLevel { get; set; }
    }
}