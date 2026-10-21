using System;
using System.Collections.Generic;
using System.Linq;
using CA1_SOA_JoshuaDonnelly.Models;
namespace CA1_SOA_JoshuaDonnelly.Models;

public class GameViewerCalc
{
    public static int GetTotalViewers(
        List<TwitchStreams> streams, string gameName)
    {
        return streams
            .Where(stream => string.Equals(
                stream.GameName,
                gameName,
                StringComparison.OrdinalIgnoreCase))
            .Sum(stream => stream.ViewerCount);
    }
}