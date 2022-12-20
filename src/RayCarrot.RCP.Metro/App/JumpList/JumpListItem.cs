namespace RayCarrot.RCP.Metro;

/// <summary>
/// A saved item in the app's jump list
/// </summary>
/// <param name="GameInstallationId">The installation id for the game installation this item belongs to</param>
/// <param name="ItemId">The unique item id for this item. This will usually be the same as the installation id,
/// but can be different if a game has multiple jump list items.</param>
public record JumpListItem(string GameInstallationId, string ItemId);