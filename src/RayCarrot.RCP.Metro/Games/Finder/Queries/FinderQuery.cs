﻿namespace RayCarrot.RCP.Metro.Games.Finder;

public abstract class FinderQuery
{
    public Func<InstallLocation, InstallLocation>? ValidateLocationFunc { get; init; }
    public string? FileName { get; init; }
}