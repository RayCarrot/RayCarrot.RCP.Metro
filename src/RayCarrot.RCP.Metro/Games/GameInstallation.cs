using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

// TODO-14: Move to and use in app user data
public class GameInstallation
{
    #region Constructors

    public GameInstallation(Games game, string installLocation) : this(game, installLocation, new Dictionary<string, string>()) { }

    [JsonConstructor]
    public GameInstallation(Games game, string installLocation, Dictionary<string, string> additionalData)
    {
        Game = game;
        InstallLocation = installLocation;
        AdditionalData = additionalData;
        GameInfo = Game.GetGameInfo();
    }

    #endregion

    #region Public Properties

    public Games Game { get; }
    public FileSystemPath InstallLocation { get; }
    public Dictionary<string, string> AdditionalData { get; }

    // TODO-14: Eventually change this
    [JsonIgnore]
    public GameInfo GameInfo { get; }

    // TODO-14: Eventually we might want to get rid of the Games enum, so use this ID as much as possible for now
    [JsonIgnore]
    public string ID => Game.ToString();

    #endregion
}