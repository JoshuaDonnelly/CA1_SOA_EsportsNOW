using CA1_SOA_JoshuaDonnelly.Models;
using RestSharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;


namespace CA1_SOA_JoshuaDonnelly;

public class TwitchStreamService
{
    // Url's for twitch's auth and stream data endpoints.
    private const string TwitchAuthUrl = "https://id.twitch.tv/";
    private const string TwitchApiId = "https://api.twitch.tv/helix";

    // Reads credentials without hardcoding keys, will return a string needed to access twitch's main endpoint
    public static async Task<string> GetAccessTokenAsync(IConfiguration config)
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
        return accessToken;
    }
    
    // 2. Retrieve Game ID to search twitch's endpoint for the appropriate category
    // Is private as this method is only a helper to be used within this class
    private static async Task<string?> GetGameIdAsync(
        RestClient apiClient, 
        string clientId, 
        string accessToken,
        string gameName
        )
    {
        // GET request to category endpoint
        var request = new RestRequest("search/categories");
        
        request.AddParameter("query", gameName);
        // Get first 10 categories back, as we can inspect for more than one category 
        request.AddQueryParameter("first", "10");
        
        request.AddHeader("Client-Id", clientId);
        request.AddHeader("Authorization", $"Bearer {accessToken}");
        
        var response = await apiClient.ExecuteAsync(request);

        if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
        {
            throw new InvalidOperationException($"Twitch game id {gameName} request failed {response.Content}");
        }
        
        var gameIdJson = JObject.Parse(response.Content);
        var categories = gameIdJson["data"] as JArray;

        if (categories == null)
        {
            return null;
        }
        // Cycle through the categories and starting for the first entry, compare its name to our inputted game name (case-insensitive)
        var category = categories.FirstOrDefault(i => 
            
            string.Equals(i["name"]?.ToString() ?? "Unknown", 
                gameName, 
                StringComparison.OrdinalIgnoreCase
            ));
        return category?["id"]?.ToString();
    }
    // 3. Using the established restclient, accessToken, game name and the game's Twitch category ID,
    // return a list of TwitchStreams objects to be used in rendering. This gets streams for one game.
    public static async Task<List<TwitchStreams>> GetStreamsForGameAsync (
        RestClient client, string clientId, string accessToken, string gameName, string gameId)
    {
        
    // This GET request returns currently live, live streams
    var streamsRequest = new RestRequest("streams");
    
    // Filter results by category ID
    streamsRequest.AddQueryParameter("game_id", gameId);
    streamsRequest.AddQueryParameter("first", "10");
    
    streamsRequest.AddHeader("Client-Id", clientId);
    streamsRequest.AddHeader("Authorization", $"Bearer {accessToken}");

    var streamsResponse = await client.ExecuteAsync(streamsRequest);

    if (!streamsResponse.IsSuccessful || string.IsNullOrWhiteSpace(streamsResponse.Content))
    {
        throw new InvalidOperationException($"Stream response failed {streamsResponse.Content}");
    }
    // Parse response to JObject
    var streamsJson = JObject.Parse(streamsResponse.Content);
    
    var streamData = streamsJson["data"] as JArray;
    
    // Create empty streams list to be used in our razor file
    var streams = new List<TwitchStreams>();
    
    // Checks if the expected data is present, if not simply return 
    if (streamData == null)
    {
        return streams;
    }

    foreach (var i in streamData)
    {
        // Add streams to the streams list as a part of the object we declared in models.
        // The property names on the left are the types that belong to that model,
        // and the properties inside [] belong to the api response. This results
        // in a strongly typed list
        streams.Add(new TwitchStreams
        {
            UserName = i["user_name"]?.ToString() ?? "Unknown",
            GameName = i["game_name"]?.ToString() ?? "Unknown",
            Title = i["title"]?.ToString() ?? "No Title",
            ViewerCount = i["viewer_count"]?.Value<int>() ?? 0,
            ThumbnailUrl = (i["thumbnail_url"]?.ToString() ?? "")
                .Replace("{width}", "320")
                .Replace("{height}", "180"),
            UserLogin = i["user_login"]?.ToString() ?? ""
        });
    }

    return streams;
}
        
    // Final Step: This method will be called by the home page, and will accept game names for PandaScore. This handles
    // streams for multiple games. This returns live streams for any games that have matching twitch categories
    public static async Task<List<TwitchStreams>> GetStreamsForGamesAsync(
        IConfiguration config,
        IEnumerable<string> gameNames)
    {
        string clientId = config["Twitch:ClientId"] ?? "";
        string token = await GetAccessTokenAsync(config);

        var client = new RestClient(TwitchApiId);
        
        // Remove blank and duplicate game names before searching Twitch. This avoids making multiple requests for the
        // same game
        var uniqueGames = gameNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            // Distinct removes dupes and ignores case
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        // Create a list of TwitchStreams objects that will hold streams for all games
        var allStreams = new List<TwitchStreams>();

        foreach (var gameName in uniqueGames)
        {
            // Search Twitch for category ID matching a given game
            string? gameId = await GetGameIdAsync(
                client, clientId, token, gameName
            );

            if (string.IsNullOrWhiteSpace(gameId))
            {
                Console.WriteLine($"No twitch category found for {gameName}");
                continue;
            }

            var gameStreams = await GetStreamsForGameAsync(client, clientId, token, gameName, gameId);
            
            allStreams.AddRange(gameStreams);
        }
        return allStreams;
        
    }
}