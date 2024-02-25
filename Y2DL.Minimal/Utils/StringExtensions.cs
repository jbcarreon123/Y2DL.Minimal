using System.Text.RegularExpressions;
using SmartFormat;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.SmartFormatters;

namespace Y2DL.Minimal.Utils;

public static class StringExtensions
{
    public static bool IsNullOrWhitespace(this string? input)
    {
        return string.IsNullOrWhiteSpace(input);
    }

    public static string Limit(this string input, int maxLength)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        if (maxLength <= 0) throw new ArgumentException("maxLength must be greater than zero.");

        if (input.Length <= maxLength) return input;

        return input.Substring(0, maxLength - 3) + "...";
    }

    public static string Format(this string input, YoutubeChannel channel) {
        Smart.Default.AddExtensions(new LimitFormatter());
        Smart.Default.AddExtensions(new ToSnowflakeFormatter());
        try {
            return Smart.Format(input, channel);
        } catch {
            return input;
        }
    }

    public static (string? Id, string Type) GetYouTubeIdAndType(this string input)
    {
        var youtubeShortRegex = new Regex(@"youtu\.be/([a-zA-Z0-9_-]+)");
        var youtubeLongRegex = new Regex(@"v=([a-zA-Z0-9_-]+)");
        var playlistRegex = new Regex(@"list=([a-zA-Z0-9_-]+)");
        var channelRegex = new Regex(@"channel/([a-zA-Z0-9_-]+)");
        var handleRegex = new Regex(@"/(@[a-zA-Z0-9_-]+)");

        Match match;

        if ((match = youtubeShortRegex.Match(input)).Success)
        {
            return (match.Groups[1].Value, "Video");
        }

        if ((match = youtubeLongRegex.Match(input)).Success)
        {
            return (match.Groups[1].Value, "Video");
        }

        if ((match = playlistRegex.Match(input)).Success)
        {
            return (match.Groups[1].Value, "Playlist");
        }

        if ((match = channelRegex.Match(input)).Success)
        {
            return (match.Groups[1].Value, "Channel");
        }

        if ((match = handleRegex.Match(input)).Success)
        {
            return (match.Groups[1].Value, "Handle");
        }

        return (null, "Unknown");
    }
    
    public static string ConvertPascalToSpaces(this string input)
    {
        return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
    }
}