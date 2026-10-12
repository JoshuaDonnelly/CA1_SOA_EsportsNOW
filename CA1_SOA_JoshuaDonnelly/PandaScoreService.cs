using Newtonsoft.Json;
using RestSharp;
using CA1_SOA_JoshuaDonnelly.Models;
using Newtonsoft.Json.Linq;

namespace CA1_SOA_JoshuaDonnelly;

public class PandaScoreService
{
    public static readonly string PANDASCORE_URL = "https://api.pandascore.co/";

    public static async Task<List<EsportsMatches>>
        GetUpcomingMatchesAsync(string apiToken)
    {
        var client = new RestClient(PANDASCORE_URL);
        var request = new RestRequest("matches/upcoming");

        request.AddHeader(
            "Authorization", $"Bearer {apiToken}");

        request.AddParameter("per_page", 10);

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
        {
            var recievedJSON = response.Content;
            
            var jsonMatches = JsonConvert.DeserializeObject<JArray>(recievedJSON);
            
            var matches = new List<EsportsMatches>();

            if (jsonMatches != null)
            {
                foreach (var i in jsonMatches)
                {
                    var opponents = i["opponents"];

                    var match = new EsportsMatches
                    {
                        Game = i["videogame"]?["name"]?.ToString()
                                    ?? "Unknown Game",

                        Tournament = i["tournament"]?["name"]?.ToString()
                                    ?? "Unknown Tournament",

                        TeamOne = opponents[0]?["opponent"]?["name"]?.ToString()
                                    ?? "Unknown Team One",

                        TeamTwo = opponents[1]?["opponent"]?["name"]?.ToString()
                                    ?? "Unknown Team Two",

                        StartTime = i["begin_at"]?.ToString() 
                                    ?? "Unknown Start Time"

                    };
                    matches.Add(match);
                }
            }
            return matches;
        }

        return new List<EsportsMatches>();
    }
}