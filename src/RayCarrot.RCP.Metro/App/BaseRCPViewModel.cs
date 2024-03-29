﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base view model to use for the Rayman Control Panel
/// </summary>
public abstract class BaseRCPViewModel : BaseViewModel
{
    #region Private Fields

    private AppUserData? _data;

    private AppViewModel? _app;

    #endregion

    #region Public Properties

    /// <summary>
    /// The current app user data
    /// </summary>
    public AppUserData Data => _data ??= Services.Data;

    /// <summary>
    /// The app view model
    /// </summary>
    public AppViewModel App => _app ??= Services.App;

    #endregion
}