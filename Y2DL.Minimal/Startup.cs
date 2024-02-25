/*
    Y2DL.Minimal by jbcarreon123

    Y2DL but without the things that inst-1 does not need.
    That means no plugin support, no ChannelReleases, and much more.

    NOT INTENDED TO BE PUBLISHED ON GITHUB!
*/

using System.Diagnostics;
using System.Reflection;
using Discord;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using Y2DL.Minimal.Database;
using Y2DL.Minimal.Models;
using Y2DL.Minimal.Services;
using Y2DL.Minimal.Utils;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;

namespace Y2DL;

public class Startup
{
    private readonly IServiceProvider _serviceProvider = CreateProvider();
    
    /// <summary>
    /// The entry point of Y2DL.
    /// Note that this isn't a single-threaded program,
    /// so don't put anything single-threaded below this (like [STAThread])
    /// </summary>
    private static void Main(string[] args)
    {
        Thread.Sleep(1000);
        new Startup().RunAsync(args).GetAwaiter().GetResult();
    }

    private static readonly int _numOfShards = 1;

    private static IServiceProvider CreateProvider()
    {
        var asm = Assembly.GetExecutingAssembly();
        var path = System.IO.Directory.GetCurrentDirectory();
        var configFile = File.ReadAllText(path + "/Y2DLConfig.yml");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        var appConfig = deserializer.Deserialize<Config>(configFile);

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(asm.Location);
        var version = fileVersionInfo.ProductVersion;
        logger.Information("Y2DL.Minimal v{0} by jbcarreon123", version);
        logger.Information("NOT INTENDED TO BE PUBLISHED ON GITHUB!");
        logger.Information("Please do not publish this in any way.");

        var ytService = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = appConfig.Main.ApiKeys[0].YoutubeApiKey,
            ApplicationName = appConfig.Main.ApiKeys[0].YoutubeApiName
        });

        var discordSocketConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All,
            LogLevel = LogSeverity.Debug
        };

        var collection = new ServiceCollection()
            .AddDbContext<Y2dlDbContext>()
            .AddScoped<Minimal.Services.YoutubeService>()
            .AddSingleton<DynamicChannelInfo>()
            .AddSingleton<DynamicVoiceChannelInfo>()
            .AddSingleton(appConfig)
            .AddSingleton(ytService)
            .AddSingleton(discordSocketConfig)
            .AddSingleton(logger)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<InteractionService>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<LoopService>()
            .AddSingleton<DatabaseManager>();

        return collection.BuildServiceProvider();
    }

    private async Task RunAsync(string[] args)
    {
        var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        var config = _serviceProvider.GetRequiredService<Config>();
        var commands = _serviceProvider.GetRequiredService<InteractionHandler>();
        var database = _serviceProvider.GetRequiredService<DatabaseManager>();
        Log.Logger = _serviceProvider.GetRequiredService<Logger>();

        database.Configure();

        client.Log += LogAsync;
        client.MessageReceived += MessageReceived;
        client.Ready += ReadyAsync;
        
        await commands.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, config.Main.BotConfig.BotToken);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private async Task ReadyAsync()
    { 
        var loopService = _serviceProvider.GetRequiredService<LoopService>();
        var commands = _serviceProvider.GetRequiredService<InteractionHandler>();
        var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        await loopService.StartAsync(CancellationToken.None);
        await commands.RegisterAsync();
        await client.SetActivityAsync(new Game("Spaghetti Code Simulator", ActivityType.Competing));
        Log.Information("Bot ready!");
    }

    private async Task MessageReceived(SocketMessage message)
    {
        var youtubeService = _serviceProvider.GetRequiredService<Minimal.Services.YoutubeService>();
        var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

        try
        {
            if (message is IUserMessage userMessage)
            {
                if (message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id) && message.Type != MessageType.Reply)
                {
                    var idType = message.Content.GetYouTubeIdAndType();

                    switch (idType.Type)
                    {
                        case "Video":
                            await userMessage.ReplyAsync(embed: await youtubeService.GetVideoAsync(idType.Id));
                            break;
                        case "Playlist":
                            await userMessage.ReplyAsync(embed: await youtubeService.GetPlaylistAsync(idType.Id));
                            break;
                        case "Channel":
                            await userMessage.ReplyAsync(embed: await youtubeService.GetChannelAsync(idType.Id));
                            break;
                        case "Handle":
                            var id = await youtubeService.ConvertHandleToChannelIdAsync(idType.Id);
                            await userMessage.ReplyAsync(embed: await youtubeService.GetChannelAsync(id));
                            break;
                        default:
                            await userMessage.ReplyAsync(embed: EmbedUtils.GenerateHelpEmbed());
                            //var path = System.IO.Directory.GetCurrentDirectory();
                            //await message.Channel.SendFileAsync(new FileAttachment(path: $"{path}/y2dl_slash.mp4", fileName: "y2dl_slash.mp4"), embed: EmbedUtils.GenerateHelpEmbed(), messageReference: new MessageReference(message.Id, message.Channel.Id, (message.Channel as SocketGuildChannel).Guild.Id));
                            break;
                    }
                }
            }
        }
        catch { }
    }
    
    private async Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, $"{message.Source}: {message.Message}");
        await Task.CompletedTask;
    }
}