﻿using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class GameOptions_ProgressionPageViewModel : GameOptions_BasePageViewModel
    {
        #region Constructor

        public GameOptions_ProgressionPageViewModel(GameProgression_BaseViewModel progressionViewModel) 
            : base(new LocalizedString(() => Resources.Progression_Header), GenericIconKind.GameOptions_Progression)
        {
            ProgressionViewModel = progressionViewModel ?? throw new ArgumentNullException(nameof(progressionViewModel));
        }

        #endregion

        #region Public Properties

        public GameProgression_BaseViewModel ProgressionViewModel { get; }

        #endregion

        #region Protected Methods

        protected override object GetPageUI() => new GameProgression_UI()
        {
            DataContext = ProgressionViewModel
        };

        protected override Task LoadAsync()
        {
            return ProgressionViewModel.LoadDataAsync();
        }

        public override void Dispose()
        {
            // Dispose base
            base.Dispose();

            // Dispose progression data
            ProgressionViewModel?.Dispose();
        }

        #endregion
    }
}