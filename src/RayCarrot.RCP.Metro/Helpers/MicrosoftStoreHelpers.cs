namespace RayCarrot.RCP.Metro;

public static class MicrosoftStoreHelpers
{
    /// <summary>
    /// Gets the URI to use when opening a product in the store
    /// </summary>
    /// <param name="productId">The id for the product page to open</param>
    /// <returns>The URI</returns>
    public static string GetStorePageURI(string productId)
    {
        // Documentation on the store URI scheme:
        // https://docs.microsoft.com/en-us/windows/uwp/launch-resume/launch-store-app

        return $"ms-windows-store://pdp/?ProductId={productId}";
    }
}