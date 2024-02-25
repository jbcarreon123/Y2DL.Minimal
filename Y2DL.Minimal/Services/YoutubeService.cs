using System.ServiceModel.Syndication;
using System.Xml;
using Discord;
using Google;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Serilog;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.ServiceInterfaces;
using Y2DL.Minimal.Utils;
using Embed = Y2DL.Minimal.Models.Embed;

namespace Y2DL.Minimal.Services;

/// <summary>
/// The class for your YouTube channel info needs.
/// <seealso cref="IYoutubeService"/>
/// </summary>
public class YoutubeService : IYoutubeService
{
    private List<YouTubeService> _youTubeServices = new();
    private YouTubeService _youTubeService;
    private readonly Config _config;

    public YoutubeService(YouTubeService youTubeService, Config config)
    {
        _config = config;
        _youTubeService = youTubeService;
        _youTubeServices.Add(youTubeService);
    }

    public async Task<Listable<YoutubeChannel>> GetChannelsAsync(Listable<string> channelIds, bool minimal = false)
    {
        List<YoutubeChannel> youtubeChannels = new List<YoutubeChannel>();

        try
        {
            youtubeChannels.AddRange(await GetChannelAsync(channelIds, minimal));
        }
        catch (GoogleApiException e)
        {
            Log.Error(e, "Google API thrown an exception, possibly API quota exceeded");
            return null;
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
        }

        return youtubeChannels;
    }

    private async Task<List<CommentThread>?> GetCommentThreadsOnVideoAsync(string videoId) {
        try {
            var req = _youTubeService.CommentThreads.List("snippet,replies");
            req.VideoId = videoId;
            req.MaxResults = 25;
            var resp = await req.ExecuteAsync();

            return resp.Items.ToList();
        } catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
            
            return null;
        }
    }
    private async Task<List<Comment>?> GetCommentAsync(Repeatable<string> commentIds) {
        try {
            var req = _youTubeService.Comments.List("snippet");
            req.Id = commentIds;
            req.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;
            var resp = await req.ExecuteAsync();

            return resp.Items.ToList();
        } catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
            
            return null;
        }
    }

    private async Task<Listable<YoutubeChannel>?> GetChannelAsync(Listable<string> channelIds, bool minimal = false)
    {
        try
        {
            Listable<YoutubeChannel> youtubeChannels = new Listable<YoutubeChannel>();

            var listRequest = _youTubeService.Channels.List("snippet,contentDetails,statistics");
            listRequest.Id = channelIds;
            var channelResponse = await listRequest.ExecuteAsync();

            List<Video> videos = new List<Video>();
            try
            {
                var video = await GetLatestVideoForChannelsAsync(channelIds);
                videos.AddRange(video);
            }
            catch (Exception e)
            {
                Log.Warning(e, "An error occured while getting videos");
            }

            foreach (var channel in channelResponse.Items)
            {
                var ytChannel = new YoutubeChannel()
                {
                    Name = channel.Snippet.Title,
                    ChannelUrl = channel.Url(),
                    Id = channel.Id,
                    Description = channel.Snippet.Description,
                    Handle = channel.Snippet.CustomUrl,
                    ChannelAvatarUrl = channel.Snippet.Thumbnails.High.Url,
                    Statistics = new Statistics()
                    {
                        Views = channel.Statistics.ViewCount.ToUlong() ?? 0,
                        Subscribers = channel.Statistics.SubscriberCount.ToUlong() ?? 0,
                        Videos = channel.Statistics.VideoCount.ToUlong() ?? 0
                    }
                };

                if (minimal == false)
                {
                    Video vid = new Video();
                    try
                    {
                        vid = videos.First(x => x.Snippet.ChannelId == channel.Id);
                    }
                    catch (Exception e)
                    {
                        if (_youTubeServices.Count is 1)
                        {
                            Log.Error(e, "An error occured while getting videos");
                        }

                        vid = new Video()
                        {
                            Id = "",
                            Snippet = new VideoSnippet()
                            {
                                Description = "",
                                Title = "",
                                PublishedAtDateTimeOffset = DateTimeOffset.MinValue,
                                Thumbnails = new ThumbnailDetails()
                                {
                                    Maxres = new Google.Apis.YouTube.v3.Data.Thumbnail()
                                    {
                                        Url = ""
                                    }
                                }
                            },
                            ContentDetails = new VideoContentDetails()
                            {
                                Duration = "PT0M0S"
                            },
                            Statistics = new VideoStatistics()
                            {
                                CommentCount = 0,
                                LikeCount = 0,
                                ViewCount = 0
                            }
                        };
                    }

                    try
                    {
                        ytChannel.LatestVideo = new LatestVideo()
                        {
                            Type = await vid.IsShort() ? VideoType.Short : vid.IsLiveStreamOngoing() || vid.IsLiveStreamReplay() ? VideoType.Stream : VideoType.Video,
                            Id = vid.Id ?? "",
                            ChannelId = vid.Snippet.ChannelId,
                            Description = vid.Snippet.Description ?? "or an error occured while getting latest video.",
                            Title = vid.Snippet.Title ?? "No videos",
                            Thumbnail = vid.Snippet.Thumbnails.Maxres.Url ?? "",
                            Url = $"https://youtu.be/{vid.Id}" ?? "",
                            PublishedAt = vid.Snippet.PublishedAtDateTimeOffset ?? DateTimeOffset.MinValue,
                            Statistics = new Statistics()
                            {
                                Views = vid.Statistics.ViewCount.ToUlong() ?? 0,
                                Comments = vid.Statistics.CommentCount.ToUlong() ?? 0,
                                Likes = vid.Statistics.LikeCount.ToUlong() ?? 0
                            },
                            Duration = vid.ContentDetails.Duration.ToTimeSpan().ToFormattedString(),
                            Video = vid,
                            DeArrow = await DeArrow.GetVideo(vid.Id),
                            RYD = await ReturnYouTubeDislike.GetDislike(vid.Id)
                        };
                    }
                    catch { }

                    if (vid.IsLiveStreamOngoing())
                        ytChannel.LatestVideo.Statistics.ConcurrentLiveViewers = vid.LiveStreamingDetails.ConcurrentViewers.ToUlong() ?? 0;
                }

                youtubeChannels.Add(ytChannel);
            }

            return youtubeChannels;
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
            
            return null;
        }
    }

    public async Task<Listable<(string channelId, List<PlaylistItem> latestVideos)>> GetPlaylistItemsAsync(Listable<string> playlistIds)
    {
        Listable<(string channelId, List<PlaylistItem> latestVideos)> playlistItems = new Listable<(string channelId, List<PlaylistItem> latestVideos)>();

        foreach (var playlistId in playlistIds)
        {
            if (playlistId.IsNullOrWhitespace())
                continue;

            try
            {
                var playlistItemsRequest = _youTubeService.PlaylistItems.List("snippet,contentDetails");
                playlistItemsRequest.MaxResults = 50;
                
                // if it's a youtube channel id
                if (playlistId.StartsWith("UC"))
                {
                    // Gets normal videos (1 quota unit)
                    List<PlaylistItem> plys = new List<PlaylistItem>();
                    playlistItemsRequest.PlaylistId = "UULF" + playlistId.Substring(2);
                    var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                    plys.AddRange(playlistItemsResponse.Items.Take(5));

                    // Gets streams (+1 quota unit)
                    if (_config.Main.ChannelConfig.EnableStreams)
                    {
                        try
                        {
                            playlistItemsRequest.PlaylistId = "UULV" + playlistId.Substring(2);
                            playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                            plys.AddRange(playlistItemsResponse.Items.Take(5));
                        }
                        catch
                        {
                        }
                    }

                    // Gets shorts (+1 quota unit)
                    if (_config.Main.ChannelConfig.EnableShorts)
                    {
                        try
                        {
                            playlistItemsRequest.PlaylistId = "UUSH" + playlistId.Substring(2);
                            playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                            plys.AddRange(playlistItemsResponse.Items.Take(5));
                        }
                        catch
                        {
                        }
                    }

                    playlistItems.Add((playlistId, plys
                        .OrderByDescending(item => item.ContentDetails.VideoPublishedAtDateTimeOffset).Take(5).ToList()));
                }
                else
                {
                    playlistItemsRequest.PlaylistId = playlistId;
                    var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                    playlistItems.Add((playlistId, playlistItemsResponse.Items.ToList()));
                }

            }
            catch (Exception e)
            {
                Log.Warning("An exception occured while getting playlist items.", e);
            }
        }

        return playlistItems;
    }

    private async Task<List<Video>> GetLatestVideoForChannelsAsync(Listable<string> channelIds)
    {
        List<string> videoIds = new List<string>();
        
        // Method 1 (RSS)
        // Much less quota used, but includes shorts
        /*
        foreach (var channelId in channelIds)
        {
            try
            {
                var url = "https://www.youtube.com/feeds/videos.xml?channel_id=" + channelId;
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                videoIds.Add(feed.Items.ToList()[0].Links.ToList()[0].Uri.AbsoluteUri.GetYouTubeId());
            }
            catch
            {

            }
        }
        */
        
        // Method 2 (API)
        // Much more quota used, but excludes shorts
        var plItems = await GetPlaylistItemsAsync(channelIds);
        videoIds.AddRange(plItems.Select(x => x.latestVideos[0].ContentDetails.VideoId));

        try {
            var gv = await GetVideosAsync(videoIds);
            return gv;
        } catch (GoogleApiException e)
        {
            if (e.Error.Code is 404) {
                var gv = new List<Video>();
                gv.Add(new Video()
                {
                    Snippet = new VideoSnippet()
                    {
                        Title = "No videos!"
                    }
                });
                return gv;
            }
            return null;
        }
        
    }

    public async Task<Playlist> GetPlaylistInfoAsync(string playlistId)
    {
        var plListRequest = _youTubeService.Playlists.List("snippet,contentDetails,status");
        plListRequest.Id = playlistId;

        var playlistListResponse = await plListRequest.ExecuteAsync();
        return playlistListResponse.Items[0];
    }

    public async Task<List<Video>> GetVideosAsync(Listable<string> videoIds)
    {
        var vidListRequest =
            _youTubeService.Videos.List("snippet,statistics,contentDetails,liveStreamingDetails");
        vidListRequest.Id = videoIds;
        var videoResponse = await vidListRequest.ExecuteAsync();
        return videoResponse.Items.ToList();
    }

    public async Task<string> ConvertHandleToChannelIdAsync(string handle) {
        var r = new Models.ChannelListResponse();
        using (var httpClient = new HttpClient()) {
            try {
                var req = await httpClient.GetStringAsync("https://yt.lemnoslife.com/channels?part=snippet&handle=" + handle);
                r = JsonConvert.DeserializeObject<Models.ChannelListResponse>(req);
            } catch {}
        }
        return r.Items[0].Id;
    }

    public async Task<Discord.Embed> GetCommentsAsync(string videoId) {
        var comments = await GetCommentThreadsOnVideoAsync(videoId);
        var embed = new EmbedBuilder()
            {
                Title = $"Comments on video {videoId}",
                Description = "To see the title of the video, use /ytinfo video."
            };

        if (comments is not null) {
            var cmts = await GetCommentAsync(comments.Select(x => x.Id).ToList()); 
            foreach (var cm in cmts) {
                embed.AddField(cm.Snippet.AuthorDisplayName, $"{cm.Snippet.TextDisplay.ProcessAndConvertTimestamps(videoId).Limit(200)}\r\n\r\n[See comment](https://www.youtube.com/watch?v={videoId}&lc={cm.Id})\r\nLikes: **{cm.Snippet.LikeCount}**", true);
            }
            return embed.Build();
        }

        return null;
    }
    
    public async Task<Discord.Embed> GetChannelAsync(string channelId)
    {
        var chnl = await GetChannelsAsync(channelId);
        var channel = chnl[0];

        var embed = new EmbedBuilder()
            {
                Title = $"{channel.Name} ({channel.Handle})",
                Url = channel.ChannelUrl,
                Description = $"{channel.Description.Limit(100)}",
                ThumbnailUrl = channel.ChannelAvatarUrl
            }
            .AddField("Subscribers", channel.Statistics.FormattedSubscribers, true)
            .AddField("Views", channel.Statistics.FormattedViews, true)
            .AddField("Videos", channel.Statistics.FormattedVideos, true);

        try
        {
            double ctl = (double)channel.LatestVideo.Statistics.Likes / (double)channel.LatestVideo.Statistics.Views;
            double ctlr = ctl * 100.00;
            
            embed.AddField("Latest Video",
                    $"[**{channel.LatestVideo.Title}**]({channel.LatestVideo.Url})\r\n"+ 
                    $"{channel.LatestVideo.Description.Limit(100)}")
                .AddField("Views", channel.LatestVideo.Statistics.FormattedViews, true)
                .AddField("Likes", channel.LatestVideo.Statistics.FormattedLikes + $" ({Math.Round(ctlr, 2)}% approx view-to-like ratio)", true)
                .AddField("Comments", channel.LatestVideo.Statistics.FormattedComments, true);
        } catch {}

        return embed.Build();
    }

    public async Task<Discord.Embed> GetPlaylistAsync(string playlistId)
    {
        var plyl = await GetPlaylistItemsAsync(playlistId);
        var plinf = await GetPlaylistInfoAsync(playlistId);
        var pllim = plyl.Take(15);

        var embed = new EmbedBuilder()
        {
            Title = plinf.Snippet.Title,
            Url = "https://youtube.com/playlist?list=" + plinf.Id,
            Description = plinf.Snippet.Description,
            Author = new EmbedAuthorBuilder()
            {
                Name = plinf.Snippet.ChannelTitle,
                Url = plinf.Snippet.ChannelId.IdUrl()
            }
        };

        foreach (var plia in pllim)
        {
            var pli = plia.latestVideos[0];
            embed.AddField(pli.Snippet.Title, $"by [{pli.Snippet.VideoOwnerChannelTitle}]({pli.Snippet.VideoOwnerChannelId.IdUrl()})\r\n" + 
                                              $"**[Go to video](https://youtu.be/{pli.Snippet.ResourceId.VideoId})**\r\n" +
                                              pli.Snippet.Description.Limit(100), true);
        }

        return embed.Build();
    }

    public async Task<Discord.Embed> GetVideoAsync(string videoId)
    {
        try {
            var videos = await GetVideosAsync(videoId);
            var video = videos[0];

            var r = await ReturnYouTubeDislike.GetDislike(video.Id);
            var d = await DeArrow.GetVideo(videoId);
            
            double ctl = (double)video.Statistics.LikeCount / (double)video.Statistics.ViewCount;
            double ctlr = ctl * 100.00;

            double ctd = (double)r.Dislikes / (double)video.Statistics.ViewCount;
            double ctdr = ctd * 100.00;

            DateTimeOffset o = (DateTimeOffset)video.Snippet.PublishedAtDateTimeOffset;

            EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = video.Snippet.ChannelTitle,
                        Url = video.Snippet.ChannelId.IdUrl()
                    },
                    Title = video.Snippet.Title,
                    Url = $"https://youtu.be/{video.Id}",
                    Description = video.Snippet.Description.Limit(100),
                    ThumbnailUrl = video.Snippet.Thumbnails.Maxres.Url
                };

            if (d is not null && d.Titles is not null)
                if (d.Titles.Count > 0 && (bool)!d.FirstTitle.Original) 
                    embed.AddField("DeArrowed Title", d.FirstTitle.TitleName);

            embed.AddField("Views", video.Statistics.ViewCount.ToUlong().ToFormattedNumber(), true)
                .AddField("Likes", video.Statistics.LikeCount.ToUlong().ToFormattedNumber() + $"\n({Math.Round(ctlr, 2)}% approx view-to-like ratio)", true)
                .AddField("Dislikes", r.Dislikes.ToUlong().ToFormattedNumber() + $"\n({Math.Round(ctdr, 2)}% approx view-to-dislike ratio)", true)
                .AddField("Created at", $"<t:{o.ToUnixTimeSeconds()}>", true);

            if (video.IsLiveStreamOngoing())
                embed.AddField("Concurrent Viewers", video.LiveStreamingDetails.ConcurrentViewers, true);
            else
                embed.AddField("Comments", video.Statistics.CommentCount.ToUlong().ToFormattedNumber(), true);

            return embed.Build();
        } catch (Exception e) {
            return new EmbedBuilder() {
                Title = "There is a exception occured while getting the video!",
                Description = $"{e}".Limit(300)
            }.Build();
        }
    }
}