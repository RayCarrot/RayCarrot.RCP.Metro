namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends localization converter utility
    /// </summary>
    public class RLLocalizationConverterUtility : BaseUbiArtLocalizationConverterUtility<RLLocalizationConverterUtilityViewModel>
    {
        #region Public Override Properties

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public override string InfoText => Resources.RLU_LocalizationConverterHeaderInfo;

        #endregion
    }
}