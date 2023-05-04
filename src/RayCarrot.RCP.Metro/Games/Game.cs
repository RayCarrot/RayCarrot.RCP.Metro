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
    [GameInfo("Rayman", GameIconAsset.Rayman1)]
    Rayman1,

    /// <summary>
    /// Rayman Designer
    /// </summary>
    [GameInfo("Rayman Designer", GameIconAsset.RaymanDesigner)]
    RaymanDesigner,

    /// <summary>
    /// Rayman by his Fans
    /// </summary>
    [GameInfo("Rayman by his Fans", GameIconAsset.RaymanByHisFans)]
    RaymanByHisFans,

    /// <summary>
    /// Rayman 60 Levels
    /// </summary>
    [GameInfo("Rayman 60 Levels", GameIconAsset.Rayman60Levels)]
    Rayman60Levels,

    /// <summary>
    /// Rayman 2
    /// </summary>
    [GameInfo("Rayman 2", GameIconAsset.Rayman2)]
    Rayman2,

    /// <summary>
    /// Rayman M/Arena
    /// </summary>
    [GameInfo("Rayman M/Arena", GameIconAsset.RaymanM)]
    RaymanMArena,

    /// <summary>
    /// Rayman 3
    /// </summary>
    [GameInfo("Rayman 3", GameIconAsset.Rayman3)]
    Rayman3,

    /// <summary>
    /// Rayman Origins
    /// </summary>
    [GameInfo("Rayman Origins", GameIconAsset.RaymanOrigins)]
    RaymanOrigins,

    /// <summary>
    /// Rayman Legends
    /// </summary>
    [GameInfo("Rayman Legends", GameIconAsset.RaymanLegends)]
    RaymanLegends,

    /// <summary>
    /// Rayman Jungle Run
    /// </summary>
    [GameInfo("Rayman Jungle Run", GameIconAsset.RaymanJungleRun)]
    RaymanJungleRun,

    /// <summary>
    /// Rayman Fiesta Run
    /// </summary>
    [GameInfo("Rayman Fiesta Run", GameIconAsset.RaymanFiestaRun)]
    RaymanFiestaRun,

    #endregion

    #region Rabbids

    /// <summary>
    /// Rayman Raving Rabbids
    /// </summary>
    [GameInfo("Rayman Raving Rabbids", GameIconAsset.RaymanRavingRabbids)]
    RaymanRavingRabbids,

    /// <summary>
    /// Rayman Raving Rabbids 2
    /// </summary>
    [GameInfo("Rayman Raving Rabbids 2", GameIconAsset.RaymanRavingRabbids2)]
    RaymanRavingRabbids2,

    /// <summary>
    /// Rabbids Go Home
    /// </summary>
    [GameInfo("Rabbids Go Home", GameIconAsset.RabbidsGoHome)]
    RabbidsGoHome,

    /// <summary>
    /// Rabbids Big Bang
    /// </summary>
    [GameInfo("Rayman Big Bang", GameIconAsset.RabbidsBigBang)]
    RabbidsBigBang,

    /// <summary>
    /// Rabbids Coding
    /// </summary>
    [GameInfo("Rayman Coding", GameIconAsset.RabbidsCoding)]
    RabbidsCoding,

    #endregion

    #region Handheld

    /// <summary>
    /// Rayman 3 (GBA)
    /// </summary>
    [GameInfo("Rayman 3 (Game Boy Advance)", GameIconAsset.Rayman3_Gba)]
    Rayman3_Gba,

    /// <summary>
    /// Rayman Hoodlums' Revenge
    /// </summary>
    [GameInfo("Rayman Hoodlums' Revenge", GameIconAsset.RaymanHoodlumsRevenge)]
    RaymanHoodlumsRevenge,

    /// <summary>
    /// Rayman Raving Rabbids (GBA)
    /// </summary>
    [GameInfo("Rayman Raving Rabbids (Game Boy Advance)", GameIconAsset.RaymanRavingRabbids)]
    RaymanRavingRabbids_Gba,

    #endregion

    #region Fan-games

    /// <summary>
    /// Rayman The Dark Magician's Reign of Terror
    /// </summary>
    [GameInfo("Rayman: The Dark Magician's Reign of Terror", GameIconAsset.TheDarkMagiciansReignofTerror)]
    TheDarkMagiciansReignofTerror,

    /// <summary>
    /// Rayman Redemption
    /// </summary>
    [GameInfo("Rayman Redemption", GameIconAsset.RaymanRedemption)]
    RaymanRedemption,

    /// <summary>
    /// Rayman ReDesigner
    /// </summary>
    [GameInfo("Rayman ReDesigner", GameIconAsset.RaymanRedesigner)]
    RaymanRedesigner,

    /// <summary>
    /// Rayman Bowling 2
    /// </summary>
    [GameInfo("Rayman Bowling 2", GameIconAsset.RaymanBowling2)]
    RaymanBowling2,

    /// <summary>
    /// Rayman Garden PLUS
    /// </summary>
    [GameInfo("Rayman Garden PLUS", GameIconAsset.RaymanGardenPLUS)]
    RaymanGardenPLUS,

    /// <summary>
    /// Globox Moment
    /// </summary>
    [GameInfo("Globox Moment", GameIconAsset.GloboxMoment)]
    GloboxMoment,

    /// <summary>
    /// Rayman: The Dreamer's Boundary
    /// </summary>
    [GameInfo("Rayman: The Dreamer's Boundary", GameIconAsset.RaymanTheDreamersBoundary)]
    RaymanTheDreamersBoundary,

    #endregion

    #region Other

    /// <summary>
    /// Rayman minigames
    /// </summary>
    [GameInfo("Rayman Minigames", GameIconAsset.Rayman1Minigames)]
    Rayman1Minigames,

    /// <summary>
    /// Rayman Edutainment (Edu/Quiz)
    /// </summary>
    [GameInfo("Rayman Edutainment", GameIconAsset.RaymanEdutainment)]
    RaymanEdutainment,

    /// <summary>
    /// Tonic Trouble
    /// </summary>
    [GameInfo("Tonic Trouble", GameIconAsset.TonicTrouble)]
    TonicTrouble,

    /// <summary>
    /// Rayman Dictées
    /// </summary>
    [GameInfo("Rayman Dictées", GameIconAsset.RaymanDictées)]
    RaymanDictées,

    /// <summary>
    /// Rayman Premiers Clics
    /// </summary>
    [GameInfo("Rayman Premiers Clics", GameIconAsset.RaymanPremiersClics)]
    RaymanPremiersClics,

    /// <summary>
    /// Rayman 3 Print Studio
    /// </summary>
    [GameInfo("Rayman 3 Print Studio", GameIconAsset.Rayman3PrintStudio)]
    Rayman3PrintStudio,

    /// <summary>
    /// Rayman Activity Center
    /// </summary>
    [GameInfo("Rayman Activity Center", GameIconAsset.RaymanActivityCenter)]
    RaymanActivityCenter,

    /// <summary>
    /// Rayman Raving Rabbids Activity Center
    /// </summary>
    [GameInfo("Rayman Raving Rabbids Activity Center", GameIconAsset.RaymanRavingRabbidsActivityCenter)]
    RaymanRavingRabbidsActivityCenter,

    #endregion
}