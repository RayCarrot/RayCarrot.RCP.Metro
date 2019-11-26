namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Origins localization converter utility
    /// </summary>
    public class ROLocalizationConverterUtility : BaseUbiArtLocalizationConverterUtility<ROLocalizationConverterUtilityViewModel>
    {
        #region Public Override Properties

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public override string InfoText => Resources.ROU_LocalizationConverterHeaderInfo;

        #endregion
    }
}