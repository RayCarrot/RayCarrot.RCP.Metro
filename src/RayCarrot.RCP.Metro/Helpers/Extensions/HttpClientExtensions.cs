using System.Net.Http;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro;

public static class HttpClientExtensions
{
    public static async Task<T> GetDeserializedAsync<T>(this HttpClient httpClient, string url)
    {
        string jsonString = await httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new Exception("The retrieved JSON response was null");
    }
}