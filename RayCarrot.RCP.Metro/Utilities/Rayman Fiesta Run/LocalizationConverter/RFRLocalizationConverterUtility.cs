namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Fiesta Run localization converter utility
    /// </summary>
    public class RFRLocalizationConverterUtility : BaseUbiArtLocalizationConverterUtility<RFRLocalizationConverterUtilityViewModel>
    {
        #region Public Override Properties

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public override string InfoText => Resources.RFRU_LocalizationConverterHeaderInfo;

        #endregion
    }
}