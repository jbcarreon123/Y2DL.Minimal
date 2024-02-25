using System.Data;
using Google.Apis.YouTube.v3.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.Utils;

namespace Y2DL.Minimal.Database;

public class DatabaseManager
{
    private static Y2dlDbContext _y2dlDbContext;

    public DatabaseManager(Y2dlDbContext Y2dlDbContext)
    {
        _y2dlDbContext = Y2dlDbContext;
    }

    public void Configure()
    {
        //_y2dlDbContext.Database.Migrate();
    }

    public async Task LatestVideoAddOrReplace(string channelId, string newVideoId)
    {
        try
        {
            var lVideo =
                _y2dlDbContext.ChannelReleasesLatestVideos.FirstOrDefault(x =>
                    x.ChannelId == channelId);
            if (lVideo is null)
                _y2dlDbContext.Add(new ChannelReleasesLatestVideo()
                {
                    ChannelId = channelId,
                    VideoId = newVideoId
                });
            else
            {
                lVideo.VideoId = newVideoId;
                _y2dlDbContext.Update(lVideo);
            }

            await _y2dlDbContext.SaveChangesAsync();
        } catch {}
    }

    public async Task<ChannelReleasesLatestVideo?> LatestVideoGet(string channelId)
    {
        try
        {
            return _y2dlDbContext.ChannelReleasesLatestVideos.FirstOrDefault(x => x.ChannelId.Equals(channelId));
        } catch {
            return null;
        }
    }

    public async Task MessagesAdd(ulong channelId, ulong messageId, string youtubeChannelId)
    {
        _y2dlDbContext.Add(new DynamicChannelInfoMessages()
        {
            ChannelId = channelId,
            MessageId = messageId,
            YoutubeChannelId = youtubeChannelId,
            Hash = HashUtils.HashThingToSHA256String(channelId + messageId + youtubeChannelId)
        });
        await _y2dlDbContext.SaveChangesAsync();
    }

    public ulong MessagesGet(string ytChannelId, ulong channelId)
    {
        var f = _y2dlDbContext.DynamicChannelInfoMessages
            .First(x => x.ChannelId == channelId && x.YoutubeChannelId == ytChannelId);

        return f.MessageId;
    }

    public bool MessagesExists(string ytChannelId, ulong channelId)
    {
        try
        {
            _y2dlDbContext.DynamicChannelInfoMessages
                .First(x => x.ChannelId == channelId && x.YoutubeChannelId == ytChannelId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task MessagesRemove(ulong messageId)
    {
        _y2dlDbContext.Remove(_y2dlDbContext.DynamicChannelInfoMessages.First(x => x.MessageId == messageId));
        await _y2dlDbContext.SaveChangesAsync();
    }
}