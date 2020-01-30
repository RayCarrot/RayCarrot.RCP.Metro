using System;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 3 Direct Play utility
    /// </summary>
    public class R3DirectPlayUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R3DirectPlayUtilityViewModel()
        {
            IsLoadingDirectPlay = true;

            Task.Run(RefreshDirectPlay);
        }

        #endregion

        #region Private Fields

        private bool _isDirectPlayEnabled;

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if DirectPlay is enabled
        /// </summary>
        public bool IsDirectPlayEnabled
        {
            get => _isDirectPlayEnabled;
            set
            {
                if (IsLoadingDirectPlay)
                    return;

                IsLoadingDirectPlay = true;

                Task.Run(() => SetDirectPlayState(value));
            }
        }

        /// <summary>
        /// Indicates if the DirectPlay status is loading
        /// </summary>
        public bool IsLoadingDirectPlay { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the state of DirectPlay
        /// </summary>
        /// <param name="enabled">Indicates if DirectPlay should be enabled</param>
        public void SetDirectPlayState(bool enabled)
        {
            try
            {
                PowerShell.Create().RunAndDispose(x =>
                    x.AddCommand(enabled ? "Enable-WindowsOptionalFeature" : "Disable-WindowsOptionalFeature").
                        AddParameter("-Online").
                        AddParameter("-FeatureName", "DirectPlay").
                        Invoke());

                RefreshDirectPlay();
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting DirectPlay state");
                IsLoadingDirectPlay = false;
            }
        }

        /// <summary>
        /// Refreshes the DirectPlay state
        /// </summary>
        public void RefreshDirectPlay()
        {
            if (!App.IsRunningAsAdmin)
            {
                IsLoadingDirectPlay = false;
                return;
            }
            
            try
            {
                var result = PowerShell.Create().RunAndDispose(x =>
                    x.AddCommand("Get-WindowsOptionalFeature").
                        AddParameter("-Online").
                        AddParameter("-FeatureName", "DirectPlay").
                        Invoke());

                // The state is of type Microsoft.Dism.Commands.FeatureState
                _isDirectPlayEnabled = result.First().Members["State"].Value?.ToString() == "Enabled";
                OnPropertyChanged(nameof(IsDirectPlayEnabled));

                IsLoadingDirectPlay = false;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting DirectPlay info");
                IsLoadingDirectPlay = false;
            }
        }

        #endregion
    }
}