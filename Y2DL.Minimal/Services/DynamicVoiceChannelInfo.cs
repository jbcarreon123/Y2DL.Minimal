using Discord.WebSocket;
using SmartFormat;
using Y2DL.Minimal.Database;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.ServiceInterfaces;
using Y2DL.Minimal.SmartFormatters;
using Y2DL.Minimal.Utils;

namespace Y2DL.Minimal.Services;

public class DynamicVoiceChannelInfo : IY2DLService<YoutubeChannel>
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public DynamicVoiceChannelInfo(DiscordSocketClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;
    }
    
    public async Task RunAsync(YoutubeChannel youtubeChannel)
    {
        var vcs = _config.Services.DynamicChannelInfoForVoiceChannels.Channels.First(x => x.ChannelId == youtubeChannel.Id);

        foreach (var vc in vcs.VoiceChannels)
        {
            try {
                Smart.Default.AddExtensions(new LimitFormatter());
                Smart.Default.AddExtensions(new ToSnowflakeFormatter());
                var s = Smart.Format(vc.Name, youtubeChannel);
                await _client.GetGuild(vc.GuildId).GetVoiceChannel(vc.ChannelId)
                    .ModifyAsync(x => x.Name = s.IsNullOrWhitespace()? $"Error occured: Please poke {youtubeChannel.Name} to fix this" : s);
            } catch {
                await _client.GetGuild(vc.GuildId).GetVoiceChannel(vc.ChannelId)
                    .ModifyAsync(x => x.Name = $"Error occured: Please poke {youtubeChannel.Name} to fix this");
            }
        }
    }
}