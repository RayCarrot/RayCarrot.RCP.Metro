namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines a game. This does not identify a unique <see cref="GameDescriptor"/> as multiple ones may be
/// for the same game! This enum is mostly used to group the descriptors.
/// </summary>
public enum Game
{
    #region Rayman

    /// <summary>
    /// Rayman 1
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman1_Title), GameIconAsset.Rayman1)]
    Rayman1,

    /// <summary>
    /// Rayman Designer
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanDesigner_Title), GameIconAsset.RaymanDesigner)]
    RaymanDesigner,

    /// <summary>
    /// Rayman by his Fans
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanByHisFans_Title), GameIconAsset.RaymanByHisFans)]
    RaymanByHisFans,

    /// <summary>
    /// Rayman 60 Levels
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman60Levels_Title), GameIconAsset.Rayman60Levels)]
    Rayman60Levels,

    /// <summary>
    /// Rayman 2
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman2_Title), GameIconAsset.Rayman2)]
    Rayman2,

    /// <summary>
    /// Rayman M/Arena
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanMArena_Title), GameIconAsset.RaymanM)]
    RaymanMArena,

    /// <summary>
    /// Rayman 3
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman3_Title), GameIconAsset.Rayman3)]
    Rayman3,

    /// <summary>
    /// Rayman Origins
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanOrigins_Title), GameIconAsset.RaymanOrigins)]
    RaymanOrigins,

    /// <summary>
    /// Rayman Legends
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanLegends_Title), GameIconAsset.RaymanLegends)]
    RaymanLegends,

    /// <summary>
    /// Rayman Jungle Run
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanJungleRun_Title), GameIconAsset.RaymanJungleRun)]
    RaymanJungleRun,

    /// <summary>
    /// Rayman Fiesta Run
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanFiestaRun_Title), GameIconAsset.RaymanFiestaRun)]
    RaymanFiestaRun,

    #endregion

    #region Rabbids

    /// <summary>
    /// Rayman Raving Rabbids
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanRavingRabbids_Title), GameIconAsset.RaymanRavingRabbids)]
    RaymanRavingRabbids,

    /// <summary>
    /// Rayman Raving Rabbids 2
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanRavingRabbids2_Title), GameIconAsset.RaymanRavingRabbids2)]
    RaymanRavingRabbids2,

    /// <summary>
    /// Rabbids Go Home
    /// </summary>
    [GameInfo(nameof(Resources.Game_RabbidsGoHome_Title), GameIconAsset.RabbidsGoHome)]
    RabbidsGoHome,

    /// <summary>
    /// Rabbids Big Bang
    /// </summary>
    [GameInfo(nameof(Resources.Game_RabbidsBigBang_Title), GameIconAsset.RabbidsBigBang)]
    RabbidsBigBang,

    /// <summary>
    /// Rabbids Coding
    /// </summary>
    [GameInfo(nameof(Resources.Game_RabbidsCoding_Title), GameIconAsset.RabbidsCoding)]
    RabbidsCoding,

    #endregion

    #region Handheld

    /// <summary>
    /// Rayman 1 (GBC)
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman1_Gbc_Title), GameIconAsset.Rayman1_Gbc)]
    Rayman1_Gbc,

    /// <summary>
    /// Rayman 2 (GBC)
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman2_Gbc_Title), GameIconAsset.Rayman2_Gbc)]
    Rayman2_Gbc,

    /// <summary>
    /// Rayman 3 (GBA)
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman3_Gba_Title), GameIconAsset.Rayman3_Gba)]
    Rayman3_Gba,

    /// <summary>
    /// Rayman Hoodlums' Revenge
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanHoodlumsRevenge_Title), GameIconAsset.RaymanHoodlumsRevenge)]
    RaymanHoodlumsRevenge,

    /// <summary>
    /// Rayman Raving Rabbids (GBA)
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanRavingRabbids_Gba_Title), GameIconAsset.RaymanRavingRabbids)]
    RaymanRavingRabbids_Gba,

    #endregion

    #region Fan-games

    /// <summary>
    /// Rayman The Dark Magician's Reign of Terror
    /// </summary>
    [GameInfo(nameof(Resources.Game_TheDarkMagiciansReignofTerror_Title), GameIconAsset.TheDarkMagiciansReignofTerror)]
    TheDarkMagiciansReignofTerror,

    /// <summary>
    /// Rayman Redemption
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanRedemption_Title), GameIconAsset.RaymanRedemption)]
    RaymanRedemption,

    /// <summary>
    /// Rayman ReDesigner
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanRedesigner_Title), GameIconAsset.RaymanRedesigner)]
    RaymanRedesigner,

    /// <summary>
    /// Rayman Bowling 2
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanBowling2_Title), GameIconAsset.RaymanBowling2)]
    RaymanBowling2,

    /// <summary>
    /// Rayman Garden PLUS
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanGardenPLUS_Title), GameIconAsset.RaymanGardenPLUS)]
    RaymanGardenPLUS,

    /// <summary>
    /// Globox Moment
    /// </summary>
    [GameInfo(nameof(Resources.Game_GloboxMoment_Title), GameIconAsset.GloboxMoment)]
    GloboxMoment,

    /// <summary>
    /// Rayman: The Dreamer's Boundary
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanTheDreamersBoundary_Title), GameIconAsset.RaymanTheDreamersBoundary)]
    RaymanTheDreamersBoundary,

    #endregion

    #region Other

    /// <summary>
    /// Rayman minigames
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman1Minigames_Title), GameIconAsset.Rayman1Minigames)]
    Rayman1Minigames,

    /// <summary>
    /// Rayman Edutainment (Edu/Quiz)
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanEdutainment_Title), GameIconAsset.RaymanEdutainment)]
    RaymanEdutainment,

    /// <summary>
    /// Tonic Trouble
    /// </summary>
    [GameInfo(nameof(Resources.Game_TonicTrouble_Title), GameIconAsset.TonicTrouble)]
    TonicTrouble,

    /// <summary>
    /// Tonic Trouble (GBC)
    /// </summary>
    [GameInfo(nameof(Resources.Game_TonicTrouble_Gbc_Title), GameIconAsset.TonicTrouble_Gbc)]
    TonicTrouble_Gbc,

    /// <summary>
    /// Rayman Dictées
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanDictées_Title), GameIconAsset.RaymanDictées)]
    RaymanDictées,

    /// <summary>
    /// Rayman Premiers Clics
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanPremiersClics_Title), GameIconAsset.RaymanPremiersClics)]
    RaymanPremiersClics,

    /// <summary>
    /// Rayman 3 Print Studio
    /// </summary>
    [GameInfo(nameof(Resources.Game_Rayman3PrintStudio_Title), GameIconAsset.Rayman3PrintStudio)]
    Rayman3PrintStudio,

    /// <summary>
    /// Rayman Activity Center
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanActivityCenter_Title), GameIconAsset.RaymanActivityCenter)]
    RaymanActivityCenter,

    /// <summary>
    /// Rayman Raving Rabbids Activity Center
    /// </summary>
    [GameInfo(nameof(Resources.Game_RaymanRavingRabbidsActivityCenter_Title), GameIconAsset.RaymanRavingRabbidsActivityCenter)]
    RaymanRavingRabbidsActivityCenter,

    #endregion
}