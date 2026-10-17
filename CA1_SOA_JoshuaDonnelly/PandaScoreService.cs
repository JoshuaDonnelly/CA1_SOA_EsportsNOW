using Newtonsoft.Json;
using RestSharp;
using CA1_SOA_JoshuaDonnelly.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace CA1_SOA_JoshuaDonnelly;

public class PandaScoreService
{
    public static readonly string PANDASCORE_URL = "https://api.pandascore.co/";

    public static readonly List<string> SupportedGames = new()
    {
        "Counter-Strike",
        "League Of Legends",
        "Rainbow Six Siege",
        "Valorant",
        "Dota 2"
    };

    public static async Task<List<EsportsMatches>>
        GetUpcomingMatchesAsync(IConfiguration config)
    {
        var client = new RestClient(PANDASCORE_URL);
        var request = new RestRequest("matches/upcoming");
        var apiToken = config["PandaScore:ApiToken"] ?? "";

        request.AddHeader(
            "Authorization", $"Bearer {apiToken}");

        request.AddParameter("per_page", 20);

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
        {
            var recievedJSON = response.Content;
            Console.WriteLine(recievedJSON);
            
            var jsonMatches = JsonConvert.DeserializeObject<JArray>(recievedJSON);
            
            var matches = new List<EsportsMatches>();

            if (jsonMatches != null)
            {
                foreach (var i in jsonMatches)
                {
                    var opponents = i["opponents"] as JArray;
                    
                    string teamOne = "Unknown Team One";
                    string teamTwo = "Unknown Team Two";

                    string game = "Unknown Game";
                    
                    game = i["videogame"]?["name"]?.ToString() ?? "Unknown Game";

                    if (game.Equals("LoL", StringComparison.OrdinalIgnoreCase))
                    {
                        game = "League Of Legends";
                    }
                    else if (game.Equals("Rainbow 6 Siege", StringComparison.OrdinalIgnoreCase))
                    {
                        game = "Rainbow Six Siege";
                    }
                    
                    

                    var teamOneImage = "No Image";
                    var teamTwoImage = "No Image";
                    
                    if (opponents != null && opponents.Count > 0)
                    {
                        teamOne = opponents[0]?["opponent"]?["name"]?.ToString()
                            ?? "Unknown Team One";
                        
                        teamOneImage = opponents[0]?["opponent"]?["image_url"]?.ToString();
                    }

                    if (opponents != null && opponents.Count > 1)
                    {
                        teamTwo = opponents[1]?["opponent"]?["name"]?.ToString()
                            ?? "Unknown Team Two";

                        teamTwoImage = opponents[1]?["opponent"]?["image_url"]?.ToString()
                            ?? "No image";
                    }

                    if (SupportedGames != null && SupportedGames.Contains(game, StringComparer.OrdinalIgnoreCase))
                    {
                        var match = new EsportsMatches
                        {

                            Game = game,

                            Tournament = i["tournament"]?["name"]?.ToString()
                                         ?? "Unknown Tournament",

                            TeamOne = teamOne,
                            TeamTwo = teamTwo,

                            TeamOneImage = teamOneImage,
                            TeamTwoImage = teamTwoImage,

                            StartTime = i["begin_at"]?.ToString()
                                        ?? "Unknown Start Time",

                        };
                    matches.Add(match);
                    }
                }
            }
            return matches;
        }

        return new List<EsportsMatches>();
    }
}