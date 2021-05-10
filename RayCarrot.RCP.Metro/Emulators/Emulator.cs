﻿namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines an emulator used to launch a game
    /// </summary>
    public abstract class Emulator
    {
        /// <summary>
        /// The display name of the emulator
        /// </summary>
        public abstract LocalizedString DisplayName { get; }
        
        /// <summary>
        /// The emulator's game configuration UI
        /// </summary>
        public abstract object GameConfigUI { get; }
    }
}