using CA1_SOA_JoshuaDonnelly.Models;
using RestSharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;


namespace CA1_SOA_JoshuaDonnelly;

public class TwitchStreamService
{
    private const string TwitchAuthUrl = "https://id.twitch.tv/";
    private const string TwitchApiId = "https://api.twitch.tv/helix";

    // Reads credentials without hardcoding keys, will return a list of Twitch Streams objects
    public static async Task<List<TwitchStreams>> GetLiveStreamsAsync(IConfiguration config)
    {
        // 1. Capture auth token to be used in fetching streams
        
        // Read keys from user-secrets (my config)
        string clientId = config["Twitch:ClientId"] ?? "";
        string clientSecret = config["Twitch:ClientSecret"] ?? "";

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("Twitch client or secret is missing");
        }
        
        // Create client to request auth token
        var authClient = new RestClient(TwitchAuthUrl);
        // Creates a POST request to oauth endpoint
        var tokenRequest = new RestRequest("oauth2/token", Method.Post);

        tokenRequest.AddParameter("client_id", clientId);
        tokenRequest.AddParameter("client_secret", clientSecret);
        // Token request holds credentials, so the user wont have to login
        tokenRequest.AddParameter("grant_type", "client_credentials");
        
        // Sends the request and awaits for Twitch's response
        var tokenResponse = await authClient.ExecuteAsync(tokenRequest);
        
        // Capture a non-successful request
        if (!tokenResponse.IsSuccessful || string.IsNullOrWhiteSpace(tokenResponse.Content))
        {
            throw new InvalidOperationException($"Twitch token request failed {tokenResponse}");
        }
        
        // Parses the response into a JObject, so we can access the individual fields through property names
        var tokenJson = JObject.Parse(tokenResponse.Content);
        
        string accessToken = tokenJson["access_token"]?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException($"Missing access token {tokenJson}");
        }
        
        // 2. Retrieve Twitch Streams using captured auth token
        
        // Creates a restclient to the twitch's main api (different from the endpoint used for auth token)
        var apiClient = new RestClient(TwitchApiId);
        
        // This endpoint returns currently live streams
        var streamsRequest = new RestRequest("streams");
        
        streamsRequest.AddHeader("Client-Id", clientId);
        streamsRequest.AddHeader("Authorization", $"Bearer {accessToken}");

        var streamsResponse = await apiClient.ExecuteAsync(streamsRequest);

        if (!streamsResponse.IsSuccessful || string.IsNullOrWhiteSpace(streamsResponse.Content))
        {
            throw new InvalidOperationException($"Stream response failed {streamsResponse.Content}");
        }
        // Parse response to JObject
        var streamsJson = JObject.Parse(streamsResponse.Content);
        
        var streamData = streamsJson["data"] as JArray;
        
        // Create empty streams list to be used in our razor file
        var streams = new List<TwitchStreams>();
        
        // Checks if the expected data is present, if not simply return to avoid an infinite loop
        if (streamData == null)
        {
            return streams;
        }

        foreach (var i in streamData)
        {
            // Add streams to the streams list as a part of the object we declared in models.
            // The property names on the left are the types that belong to that model,
            // and the properties inside [] belong to the api response
            streams.Add(new TwitchStreams
            {
                UserName = i["user_name"]?.ToString() ?? "Unknown",
                GameName = i["game_name"]?.ToString() ?? "Unknown",
                Title = i["title"]?.ToString() ?? "No Title",
                ViewerCount = i["viewer_count"]?.Value<int>() ?? 0,
                ThumbnailUrl = i["thumbnail_url"]?.ToString() ?? "",
            });
        }

        return streams;
    }
}