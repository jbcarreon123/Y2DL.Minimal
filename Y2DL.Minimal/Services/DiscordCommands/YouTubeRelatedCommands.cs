using Discord;
using Discord.Interactions;
using SmartFormat;
using System.Diagnostics;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.SmartFormatters;
using Embed = Discord.Embed;

namespace Y2DL.Minimal.Services.DiscordCommands;

[Group("ytinfoformat", "Show YouTube channel/video info but you are the one that formats the output")]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
public class YouTubeRelatedCommandsFormat : InteractionModuleBase<SocketInteractionContext>
{
    private readonly YoutubeService _youtubeService;

    public YouTubeRelatedCommandsFormat(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }

    [SlashCommand("channel", "Show YouTube channel info")]
    public async Task YtChannel([Summary(description: "The Channel ID")] string channelId, [Summary(description: "The format of the output")] string format)
    {
        await DeferAsync();
        var channels = await _youtubeService.GetChannelsAsync(channelId, false);
        if (channels.Count() > 0)
        {
            var channel = channels[0];
            Smart.Default.AddExtensions(new LimitFormatter());
            Smart.Default.AddExtensions(new ToSnowflakeFormatter());
            var output = Smart.Format(format, channel);
            await FollowupAsync(output);
        }
        else
        {
            await FollowupAsync("Oh no! That channel does not exist!");
        }
    }

    [SlashCommand("video", "Show YouTube video info")]
    public async Task YtVideo([Summary(description: "The Video ID")] string videoId, [Summary(description: "The format of the output")] string format)
    {
        await DeferAsync();
        var videos = await _youtubeService.GetVideosAsync(videoId);
        if (videos.Count() > 0)
        {
            var video = videos[0];
            Smart.Default.AddExtensions(new LimitFormatter());
            Smart.Default.AddExtensions(new ToSnowflakeFormatter());
            var output = Smart.Format(format, video);
            await FollowupAsync(output);
        }
        else
        {
            await FollowupAsync("Oh no! That video does not exist!");
        }
    }
}

[Group("ytinfobulk", "Show YouTube channel/video info in bulk")]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
public class YouTubeRelatedCommandsBulk : InteractionModuleBase<SocketInteractionContext>
{
    private readonly YoutubeService _youtubeService;

    public YouTubeRelatedCommandsBulk(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }

    [SlashCommand("videos", "Show info of multiple videos")]
    public async Task YtVideos([Summary(description: "Comma-seperated list of video IDs")] string videoIds = "", [Summary(description: "Playlist ID or channel ID")] string playlistId = "")
    {
        if (videoIds == "" && playlistId == "")
        {
            await RespondAsync("You need to specify the video IDs on a comma-seperated list form, or a channel ID, or a playlist ID.");
            return;
        }

        await DeferAsync();
        var stp = new Stopwatch();
        stp.Start();

        List<Google.Apis.YouTube.v3.Data.Video>? videos;

        if (playlistId.Length > 0)
        {
            var vids = await _youtubeService.GetPlaylistItemsAsync(playlistId);
            var (channelId, latestVideos) = vids[0];
            videos = await _youtubeService.GetVideosAsync(latestVideos.Select(x => x.Snippet.ResourceId.VideoId).ToList());
        } else {
            videos = await _youtubeService.GetVideosAsync(videoIds.Split(',').ToList());
        }

        var embedFields = new List<EmbedFieldBuilder>();
        foreach (var video in videos)
        {
            var r = await ReturnYouTubeDislike.GetDislike(video.Id);
            embedFields.Add(new EmbedFieldBuilder()
            {
                Name = video.Snippet.Title + $" (by {video.Snippet.ChannelTitle})",
                Value = @$"
[Go to video](https://youtu.be/{video.Id})
Views: **{video.Statistics.ViewCount}**
Likes: **{video.Statistics.LikeCount}**
Dislikes: **{r.Dislikes}**
Comments: **{video.Statistics.CommentCount}**
",
                IsInline = true
            });
        }
        var embedFlds = embedFields.Chunk(25);
        var embeds = new List<Embed>();
        foreach (var embedFld in embedFlds)
        {
            embeds.Add(new EmbedBuilder()
            {
                Fields = embedFld.ToList()
            }.Build()
            );
        }
        stp.Stop();
        embeds.Insert(0, new EmbedBuilder()
        {
            Title = "Results of `BULK SHOW VIDEO INFO`",
            Description = $"Took around `{stp.Elapsed.TotalMilliseconds}`ms"
        }.Build());

        await FollowupAsync(embeds: embeds.ToArray());
    }

    [SlashCommand("channels", "Show info of multiple YouTube channels")]
    public async Task YtChannels([Summary(description: "Comma-seperated list of channel IDs")] string channelIds)
    {
        await DeferAsync();
        var stp = new Stopwatch();
        stp.Start();
        var channels = await _youtubeService.GetChannelsAsync(channelIds.Split(',').ToList());
        var embedFields = new List<EmbedFieldBuilder>();
        foreach (var channel in channels)
        {
            embedFields.Add(new EmbedFieldBuilder()
            {
                Name = channel.Name + " (@" + channel.Handle + ")",
                Value = @$"
[Go to channel]({channel.ChannelUrl})
Channel ID: **{channel.Id}**
Subscribers: **{channel.Statistics.FormattedSubscribers}**
Videos: **{channel.Statistics.FormattedVideos}**
Views: **{channel.Statistics.FormattedViews}**
",
                IsInline = true
            });
        }
        var embedFlds = embedFields.Chunk(25);
        var embeds = new List<Embed>();
        foreach (var embedFld in embedFlds)
        {
            embeds.Add(new EmbedBuilder()
            {
                Fields = embedFld.ToList()
            }.Build()
            );
        }
        stp.Stop();
        embeds.Insert(0, new EmbedBuilder()
        {
            Title = "Results of `BULK SHOW CHANNEL INFO`",
            Description = $"Took around `{stp.Elapsed.TotalMilliseconds}`ms"
        }.Build());

        await FollowupAsync(embeds: embeds.ToArray());
    }
}

[Group("ytinfo", "Show YouTube channel/video/playlist info")]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
public class YouTubeRelatedCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly YoutubeService _youtubeService;
    
    public YouTubeRelatedCommands(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }
    
    [SlashCommand("channel", "Show YouTube channel info")]
    public async Task YtChannel([Summary(description: "The Channel ID")] string channelId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetChannelAsync(channelId));
    }

    [SlashCommand("playlist", "Show YouTube playlist info")]
    public async Task YtPlaylist([Summary(description: "The Playlist ID")] string playlistId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetPlaylistAsync(playlistId));
    }

    [SlashCommand("video", "Show YouTube video info")]
    public async Task YtVideo([Summary(description: "The Video ID")] string videoId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetVideoAsync(videoId));
    }

    [SlashCommand("comment", "Show YouTube video comments")]
    public async Task YtVideoComment([Summary(description: "The Video ID")] string videoId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetCommentsAsync(videoId));
    }
}