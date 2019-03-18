using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Shell;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Rayman3Utilities.xaml
    /// </summary>
    public partial class Rayman3Utilities : VMUserControl<Rayman3UtilitiesViewModel>
    {
        public Rayman3Utilities()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// View model for the Rayman 3 utilities
    /// </summary>
    public class Rayman3UtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman3UtilitiesViewModel()
        {
            // Create the commands
            EnableDirectPlayCommand = new AsyncRelayCommand(EnableDirectPlayAsync);
            DisableDirectPlayCommand = new AsyncRelayCommand(DisableDirectPlayAsync);
        }

        #endregion

        #region Commands

        public ICommand EnableDirectPlayCommand { get; }

        public ICommand DisableDirectPlayCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables DirectPlay
        /// </summary>
        /// <returns>The task</returns>
        public async Task EnableDirectPlayAsync()
        {
            try
            {
                WindowsHelpers.RunCommandPromptScript("dism /online /enable-feature:DirectPlay", true);

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("DirectPlay has been enabled.", "Action complete");
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying R3 DirectPlay enable patch");
                await RCF.MessageUI.DisplayMessageAsync("DirectPlay could not be enabled.", "Error", MessageType.Error);
            }
        }

        /// <summary>
        /// Disables DirectPlay
        /// </summary>
        /// <returns>The task</returns>
        public async Task DisableDirectPlayAsync()
        {
            try
            {
                WindowsHelpers.RunCommandPromptScript("dism /online /disable-feature:DirectPlay", true);

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("DirectPlay has been disabled.", "Action complete");
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying R3 DirectPlay disable patch");
                await RCF.MessageUI.DisplayMessageAsync("DirectPlay could not be disabled.", "Error", MessageType.Error);
            }
        }

        #endregion
    }
}