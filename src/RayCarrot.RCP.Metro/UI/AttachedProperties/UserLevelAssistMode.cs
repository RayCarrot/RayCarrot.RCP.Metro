﻿namespace RayCarrot.RCP.Metro;

/// <summary>
/// The behavior to use for <see cref="UserLevelAssist"/>
/// </summary>
public enum UserLevelAssistMode
{
    /// <summary>
    /// The element should be collapsed when not meeting the <see cref="UserLevel"/> requirement
    /// </summary>
    Collapse = 0,

    /// <summary>
    /// The element should be disabled when not meeting the <see cref="UserLevel"/> requirement
    /// </summary>
    Disable = 1
}