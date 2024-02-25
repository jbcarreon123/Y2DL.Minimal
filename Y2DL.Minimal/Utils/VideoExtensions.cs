using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Y2DL.Minimal.Models;

namespace Y2DL.Minimal.Utils;

public static class VideoExtensions
{
    /// <summary>
    /// Checks if it's a YouTube short.
    ///
    /// Note that YouTube shorts has:
    /// Length: &lt;=1m
    /// Size: 9:16 or any portrait video
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if the video is short or not.</returns>
    public static async Task<bool> IsShort(this Video video)
    {
        using (var httpClient = new HttpClient())
        {
            var videoString = await httpClient.GetStringAsync(
                $"https://yt.lemnoslife.com/videos?part=short&id={video.Id}");
            var vid = JsonConvert.DeserializeObject<Videos>(videoString);

            try {
                return vid.Items[0].Short.Available;
            } catch {
                return false;
            }
        }
    }

    /// <summary>
    /// Checks if it's a ongoing livestream.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a livestream that is ongoing or not.</returns>
    public static bool IsLiveStreamOngoing(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ConcurrentViewers is not null;
    }
    
    /// <summary>
    /// Checks if the video is a livestream replay.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a livestream that is a replay or not.</returns>
    public static bool IsLiveStreamReplay(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ConcurrentViewers is null;
    }

    /// <summary>
    /// Checks if the video is a premiere that isn't started.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a premiere that isn't started.</returns>
    public static bool IsPremiereNotStarted(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ScheduledStartTime is not null && video.Snippet.LiveBroadcastContent == "upcoming";
    }

    /// <summary>
    /// Checks if the video is a premiere that is live.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a premiere that is live.</returns>
    public static bool IsPremiereLive(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ScheduledStartTime is not null && video.Snippet.LiveBroadcastContent == "live";
    }

    /// <summary>
    /// Checks if the video is a premiere that is live or is a normal video.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a premiere that is live or if is a normal vid.</returns>
    public static bool IsPremiereLiveOrNormalVideo(this Video video)
    {
        return video.Snippet.LiveBroadcastContent == "live" || video.Snippet.LiveBroadcastContent == "none";
    }
}