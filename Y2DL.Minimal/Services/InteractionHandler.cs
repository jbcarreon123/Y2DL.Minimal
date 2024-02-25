using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Y2DL.Minimal.Models;

namespace Y2DL.Minimal.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly Config _config;
    private readonly IServiceProvider _serviceProvider;

    public InteractionHandler(DiscordSocketClient client, InteractionService commands, Config config, IServiceProvider serviceProvider)
    {
        _client = client;
        _commands = commands;
        _config = config;
        _serviceProvider = serviceProvider;
        
        // Bind InteractionCreated
        _client.InteractionCreated += HandleInteraction;

        // Handle execution results
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _commands.ContextCommandExecuted += ContextCommandExecuted;
        _commands.ComponentCommandExecuted += ComponentCommandExecuted;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Can't initialize commands");
        }
    }

    public async Task RegisterAsync()
    {
        try
        {
            await _commands.RegisterCommandsGloballyAsync(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Can't register commands");
        }
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        return Task.CompletedTask;
    }

    private async Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            try
            {
                await arg2.Interaction.DeferAsync();
            }
            finally
            {
                if (arg3.ErrorReason != "Cannot respond or defer twice to the same interaction")
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "An error occured while executing the command!",
                    };
                    if (arg3.Error.HasValue)
                    {
                        embed.AddField("Error", arg3.Error.Value.ToString(), true);
                        embed.AddField("Error Reason", arg3.ErrorReason, true);
                    }
                    await arg2.Interaction.FollowupAsync(embed: embed.Build());
                }
            }
        }
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        if (!_config.Services.Commands.Enabled)
        {
            await arg.RespondAsync(embed:
                new EmbedBuilder()
                    .WithTitle("Commands service is disabled.")
                    .WithDescription("Enable it in the config.")
                    .WithColor(Color.Red)
                    .Build(), ephemeral: true);
            return;
        }
        
        try
        {
            SocketInteractionContext ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _serviceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
    
    public InteractionService GetInteractionService()
    {
        return _commands;
    }
}