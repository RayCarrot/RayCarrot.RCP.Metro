using System.ComponentModel;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for FeedbackPrompt.xaml
    /// </summary>
    public partial class FeedbackPrompt : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public FeedbackPrompt()
        {
            InitializeComponent();
            SurveyResult = PopupPromptResult.Ignore;
        }

        #endregion

        #region Event Handlers

        private void ButtonSurvey_Click(object sender, RoutedEventArgs e)
        {
            SurveyResult = PopupPromptResult.Accept;
            Close();
        }

        private void ButtonIgnore_Click(object sender, RoutedEventArgs e)
        {
            SurveyResult = PopupPromptResult.Ignore;
            Close();
        }

        private void FeedbackPrompt_OnClosing(object sender, CancelEventArgs e)
        {
            // Check if the request should not be shown again
            if (SurveyResult == PopupPromptResult.Ignore && DoNotShowAgain.IsChecked == true)
                SurveyResult = PopupPromptResult.DoNotShowAgain;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The survey request result
        /// </summary>
        public PopupPromptResult SurveyResult { get; private set; }

        #endregion
    }
}