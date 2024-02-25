using Google.Apis.YouTube.v3.Data;

namespace Y2DL.Minimal.Utils;

public static class ChannelSnippetExtensions
{
    public static string Url(this Channel channel)
    {
        return channel.Snippet.CustomUrl.IsNullOrWhitespace()
            ? $"https://youtube.com/channel/{channel.Id}"
            : $"{channel.Snippet.CustomUrl}";
    }

    public static string IdUrl(this string channelId)
    {
        return "https://youtube.com/channel/" + channelId;
    }
}