using CA1_SOA_JoshuaDonnelly.Models;
using System.Collections.Generic;
using CA1_SOA_JoshuaDonnelly;
namespace CA1_SOA_JoshuaDonnelly.Tests;

public class Tests
{
    private List<TwitchStreams> streams = new();
    
    [SetUp]
    public void Setup()
    {
        streams = new List<TwitchStreams>
        {
            new TwitchStreams
            {
                GameName = "Valorant",
                ViewerCount = 100
            },
            new TwitchStreams
            {
                GameName = "Valorant",
                ViewerCount = 200
            },
            new TwitchStreams
            {
                GameName = "Counter-Strike",
                ViewerCount = 50
            }
        };
    }

    [Test]
    public void TestTwitchStreamViewerCount()
    {
        var stream = new TwitchStreams
        {
            ViewerCount = 200
        };
        Assert.That(stream.ViewerCount, Is.EqualTo(200));
    }

    [Test]
    public void TestTwitchStreamsUserName()
    {
        var stream = new TwitchStreams
        {
            Title = "Stream One",
            UserName = "supercoolTestStreamer",
            ViewerCount = 200
        };
        Assert.That(stream.UserName, Is.EqualTo("supercoolTestStreamer"));
    }
    [Test]
    public void TestGetTotalViewers()
    {
        int result = GameViewerCalc.GetTotalViewers(
            streams, "Valorant");
        
        Assert.That(result, Is.EqualTo(300));
    }
    [Test]
    public void TestGetTotalViewers0()
    {
        int result = GameViewerCalc.GetTotalViewers(
            streams, "Dota 2");
        
        Assert.That(result, Is.EqualTo(0));
    }
    [Test]
    public void TestGetTotalViewersIgnoreCapital()
    {
        int result = GameViewerCalc.GetTotalViewers(
            streams, "valorant");
        
        Assert.That(result, Is.EqualTo(300));
    }
}