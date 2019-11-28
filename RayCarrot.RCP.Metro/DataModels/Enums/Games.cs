using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The available games
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

        #endregion

        #region Demo

        //Rayman1_0,
        //Rayman1_1,
        //RaymanGold,
        //Rayman2_0,
        //Rayman2_1,
        //Rayman3_0,
        //Rayman3_1,
        //Rayman3_2,
        //Rayman3_3,
        //Rayman3_4,

        #endregion

        #region Other

        /// <summary>
        /// Educational DOS Rayman games
        /// </summary>
        EducationalDos,

        /// <summary>
        /// Rayman 3 Print Studio
        /// </summary>
        PrintStudio,

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