using System.Drawing;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Y2DL.Minimal.Models;
using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using Y2DL.Minimal.SmartFormatters;
using System.Text.RegularExpressions;

namespace Y2DL.Minimal.Utils;

public static class YTCommentUtils {
    public static string ToYouTubeLink(this TimeSpan timestamp, string videoId)
    {
        int totalSeconds = (int)timestamp.TotalSeconds;
        return $"[{timestamp}](https://www.youtube.com/watch?v={videoId}&t={totalSeconds})";
    }

    public static string ConvertTimeStringToMarkdownLink(this string timeString, string videoId)
    {
        TimeSpan timestamp = ParseTimeString(timeString);
        return timestamp.ToYouTubeLink(videoId);
    }

    private static TimeSpan ParseTimeString(string timeString)
    {
        Regex regex = new Regex(@"^((?<hours>\d+):)?(?<minutes>\d+):(?<seconds>\d+)$|^((?<minutes_single>\d+):(?<seconds_single>\d+))$");
        Match match = regex.Match(timeString);

        if (!match.Success)
        {
            throw new ArgumentException("Invalid time format. Please use HH:mm:ss, mm:ss, or m:ss.");
        }

        int hours = match.Groups["hours"].Success ? int.Parse(match.Groups["hours"].Value) : 0;
        int minutes = match.Groups["minutes"].Success ? int.Parse(match.Groups["minutes"].Value) : 0;
        int seconds = match.Groups["seconds"].Success ? int.Parse(match.Groups["seconds"].Value) : 0;

        int minutesSingle = match.Groups["minutes_single"].Success ? int.Parse(match.Groups["minutes_single"].Value) : 0;
        int secondsSingle = match.Groups["seconds_single"].Success ? int.Parse(match.Groups["seconds_single"].Value) : 0;

        if (minutesSingle > 0)
        {
            // If using m:ss format
            minutes = minutesSingle;
            seconds = secondsSingle;
        }

        return new TimeSpan(hours, minutes, seconds);
    }

    public static string ProcessAndConvertTimestamps(this string input, string videoId)
    {
        Regex timeRegex = new Regex(@"(?<timestamp>\d{1,2}:\d{2}(?::\d{2})?)");

        var matches = timeRegex.Matches(input);
        var convertedString = input;

        foreach (Match match in matches)
        {
            string timestamp = match.Groups["timestamp"].Value;
            try
            {
                string youtubeLink = ConvertTimeStringToMarkdownLink(timestamp, videoId);
                convertedString = convertedString.Replace(timestamp, youtubeLink);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        return convertedString;
    }
}