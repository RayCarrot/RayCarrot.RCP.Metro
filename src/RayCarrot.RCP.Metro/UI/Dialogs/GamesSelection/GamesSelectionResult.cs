﻿using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// User input result for a games selection
    /// </summary>
    public class GamesSelectionResult : UserInputResult
    {
        /// <summary>
        /// The selected game type
        /// </summary>
        public IReadOnlyCollection<Games> SelectedGames { get; set; }
    }
}