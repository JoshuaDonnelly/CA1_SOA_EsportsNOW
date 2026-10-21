using System;
using System.Globalization;
namespace CA1_SOA_JoshuaDonnelly;

public class CountdownMatchHelper
{
    public static string GetCountdown(string startTime)
    {
        if (!DateTime.TryParseExact(
                startTime,
                "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime matchTime
            ))
        {
            return "Start Time Unavailable";
        }
        TimeSpan remaining = matchTime - DateTimeOffset.UtcNow;
        if (remaining.TotalSeconds < 0)
        {
            return "Already Started";
        }
        if (remaining.TotalMinutes <= 20)
        {
            return "Starting Soon";
        }

        
        return $"Starts in {remaining.Days}d " +
               $"{remaining.Hours:D2}h " +
               $"{remaining.Minutes:D2}m " +
               $"{remaining.Seconds:D2}s";
    }
}