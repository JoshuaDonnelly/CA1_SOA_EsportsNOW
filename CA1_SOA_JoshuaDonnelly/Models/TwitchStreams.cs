namespace CA1_SOA_JoshuaDonnelly.Models;

public class TwitchStreams
{
    public string UserName { get; set; } = "";
    public string GameName { get; set; } = "";
    public string Title { get; set; } = "";
    public int ViewerCount { get; set; }
    public string ThumbnailUrl { get; set; } = "";
    
    public string UserLogin  { get; set; } = "";
}