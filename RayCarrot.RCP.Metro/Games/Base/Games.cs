using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The games supported by the Rayman Control Panel
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Games
    {
        #region Rayman

        /// <summary>
        /// Rayman 1
        /// </summary>
        Rayman1,

        /// <summary>
        /// Rayman Designer
        /// </summary>
        RaymanDesigner,

        /// <summary>
        /// Rayman by his Fans
        /// </summary>
        RaymanByHisFans,

        /// <summary>
        /// Rayman 60 Levels
        /// </summary>
        Rayman60Levels,

        /// <summary>
        /// Rayman 2
        /// </summary>
        Rayman2,

        /// <summary>
        /// Rayman M
        /// </summary>
        RaymanM,

        /// <summary>
        /// Rayman Arena
        /// </summary>
        RaymanArena,

        /// <summary>
        /// Rayman 3
        /// </summary>
        Rayman3,

        /// <summary>
        /// Rayman Origins
        /// </summary>
        RaymanOrigins,

        /// <summary>
        /// Rayman Legends
        /// </summary>
        RaymanLegends,

        /// <summary>
        /// Rayman Jungle Run
        /// </summary>
        RaymanJungleRun,

        /// <summary>
        /// Rayman Fiesta Run
        /// </summary>
        RaymanFiestaRun,

        #endregion

        #region Rabbids

        /// <summary>
        /// Rayman Raving Rabbids
        /// </summary>
        RaymanRavingRabbids,

        /// <summary>
        /// Rayman Raving Rabbids 2
        /// </summary>
        RaymanRavingRabbids2,

        /// <summary>
        /// Rabbids Go Home
        /// </summary>
        RabbidsGoHome,

        /// <summary>
        /// Rabbids Big Bang
        /// </summary>
        RabbidsBigBang,

        /// <summary>
        /// Rabbids Coding
        /// </summary>
        RabbidsCoding,

        #endregion

        #region Demos

        Demo_Rayman1_1,
        Demo_Rayman1_2,

        Demo_RaymanGold,

        Demo_Rayman2_1,
        Demo_Rayman2_2,

        Demo_RaymanM,

        Demo_Rayman3_1,
        Demo_Rayman3_2,
        Demo_Rayman3_3,
        Demo_Rayman3_4,
        Demo_Rayman3_5,

        #endregion

        #region Other

        /// <summary>
        /// Rayman 1 minigames
        /// </summary>
        Ray1Minigames,

        /// <summary>
        /// Educational DOS Rayman games
        /// </summary>
        EducationalDos,

        /// <summary>
        /// Rayman Dictées
        /// </summary>
        RaymanDictées,

        /// <summary>
        /// Rayman Premiers Clics
        /// </summary>
        RaymanPremiersClics,

        /// <summary>
        /// Rayman 3 Print Studio
        /// </summary>
        PrintStudio,

        /// <summary>
        /// Rayman Activity Center
        /// </summary>
        RaymanActivityCenter,

        /// <summary>
        /// Rayman Raving Rabbids Activity Center
        /// </summary>
        RaymanRavingRabbidsActivityCenter,

        #endregion

        #region Fan-games

        /// <summary>
        /// Rayman The Dark Magician's Reign of Terror
        /// </summary>
        TheDarkMagiciansReignofTerror,

        /// <summary>
        /// Globox Moment
        /// </summary>
        GloboxMoment,

        #endregion
    }
}