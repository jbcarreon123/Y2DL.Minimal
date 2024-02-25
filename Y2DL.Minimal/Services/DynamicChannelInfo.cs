using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Y2DL.Minimal.Database;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.ServiceInterfaces;
using Y2DL.Minimal.Utils;
using Embed = Discord.Embed;

namespace Y2DL.Minimal.Services;


public class DynamicChannelInfo : IY2DLService<YoutubeChannel>
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public DynamicChannelInfo(DiscordSocketClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;
    }

    public async Task RunAsync(YoutubeChannel youtubeChannel)
    {
        var msg = _config.Services.DynamicChannelInfo.Messages.First(x => x.ChannelId == youtubeChannel.Id);

        var embed = msg.Embed.ToDiscordEmbedBuilder(youtubeChannel).Build();
        
        if (msg.Output.UseWebhook)
        {
            if (_database.MessagesExists(youtubeChannel.Id, msg.Output.ChannelId))
            {
                await new DiscordWebhookClient(msg.Output.WebhookUrl)
                    .ModifyMessageAsync(_database.MessagesGet(youtubeChannel.Id, msg.Output.ChannelId), x =>
                    {
                        x.Content = msg.Content;
                        x.Embeds = new []
                        {
                            embed
                        };
                    });
            }
            else
            {
                var msgId = await new DiscordWebhookClient(msg.Output.WebhookUrl)
                    .SendMessageAsync(
                        msg.Content.Format(youtubeChannel),
                        embeds: new []
                        {
                            embed
                        }
                    );
                
                await _database.MessagesAdd(msg.Output.ChannelId, msgId, youtubeChannel.Id);
            }
        }
        else
        {
            if (_database.MessagesExists(youtubeChannel.Id, msg.Output.ChannelId))
            {
                await _client.GetGuild(msg.Output.GuildId).GetTextChannel(msg.Output.ChannelId)
                    .ModifyMessageAsync(_database.MessagesGet(youtubeChannel.Id, msg.Output.ChannelId), x =>
                    {
                        x.Content = msg.Content.Format(youtubeChannel);
                        x.Embeds = new []
                        {
                            embed
                        };
                    });
            }
            else
            {
                var m = await _client.GetGuild(msg.Output.GuildId).GetTextChannel(msg.Output.ChannelId)
                    .SendMessageAsync(
                        msg.Content.Format(youtubeChannel),
                        embed: embed
                    );

                await _database.MessagesAdd(msg.Output.ChannelId, m.Id, youtubeChannel.Id);
            }
        }
    }
}