using Discord.WebSocket;
using Y2DL.Minimal.Database;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.ServiceInterfaces;

namespace Y2DL.Minimal.Services;

public class LinkedSubscriberRoles
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public LinkedSubscriberRoles(DiscordSocketClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;

        _client.ButtonExecuted += LSR_ButtonExecuted;
    }

    public async Task LSR_ButtonExecuted(SocketMessageComponent component)
    {
        
    }
}