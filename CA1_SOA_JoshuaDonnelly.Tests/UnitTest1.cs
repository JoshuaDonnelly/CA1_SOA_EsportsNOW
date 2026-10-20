using CA1_SOA_JoshuaDonnelly.Models;

namespace CA1_SOA_JoshuaDonnelly.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
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
    
}