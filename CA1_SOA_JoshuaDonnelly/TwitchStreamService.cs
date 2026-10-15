using CA1_SOA_JoshuaDonnelly.Models;
using RestSharp;

namespace CA1_SOA_JoshuaDonnelly;

public class TwitchStreamService
{
    private const string TwitchAuthUrl = "https://id.twitch.tv/";
    private const string TwitchClientId = "https://api.twitch.tv/helix";

    public static async Task<List<TwitchStreams>> GetLiveStreamsAsync(IConfiguration config)
    {
        string clientId = config["Twitch:ClientId"] ?? "";
        string clientSecret = config["Twitch:ClientSecret"] ?? "";

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("Twitch client or secret is missing");
        }

        var authClient = new RestClient(TwitchAuthUrl);
        var tokenRequest = new RestClient("oauth2/token");

    }
}