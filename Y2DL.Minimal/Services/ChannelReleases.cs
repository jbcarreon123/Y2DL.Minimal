using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Y2DL.Minimal.Database;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.ServiceInterfaces;
using Y2DL.Minimal.Utils;

namespace Y2DL.Minimal.Services;

public class ChannelReleases : IY2DLService<YoutubeChannel>
{
    private static List<(string, string)> _latestVideo { get; set; } = new();
    
    private readonly DiscordSocketClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public ChannelReleases(DiscordSocketClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;
    }

    public async Task RunAsync(YoutubeChannel channel)
    {
        try
        {
            var e = await _database.LatestVideoGet(channel.Id);
            if (channel.LatestVideo.Video.IsPremiereLiveOrNormalVideo())
            {

                if (channel.LatestVideo.Id != "" && e is not null && channel.LatestVideo.Id != e.VideoId)
                {
                    foreach (var msg in _config.Services.ChannelReleases.Messages.FindAll(x => x.ChannelId == channel.Id))
                    {
                        var embed = msg.Embed is not null ? msg.Embed.ToDiscordEmbedBuilder(channel).Build() : null;

                        if (msg.Output.UseWebhook)
                        {
                            await new DiscordWebhookClient(msg.Output.WebhookUrl)
                                .SendMessageAsync(
                                    msg.Content,
                                    embeds: new[]
                                    {
                                        embed
                                    }
                                );

                            await _database.LatestVideoAddOrReplace(channel.Id, channel.LatestVideo.Id);
                        }
                        else
                        {
                            await _client.GetGuild(msg.Output.GuildId).GetTextChannel(msg.Output.ChannelId)
                                .SendMessageAsync(
                                    msg.Content.Format(channel),
                                    embed: embed
                                );

                            await _database.LatestVideoAddOrReplace(channel.Id, channel.LatestVideo.Id);
                        }
                    }
                }
                else
                {
                    await _database.LatestVideoAddOrReplace(channel.Id, channel.LatestVideo.Id);
                }
            }
        }
        catch
        {
            await _database.LatestVideoAddOrReplace(channel.Id, channel.LatestVideo.Id);
        }
    }
}