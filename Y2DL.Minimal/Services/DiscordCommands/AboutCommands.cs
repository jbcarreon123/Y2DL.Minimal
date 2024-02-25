using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Y2DL.Minimal.Services.DiscordCommands;

[Group("about", "About this bot or Y2DL (if configured)")]
public class AboutCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("y2dl", "About Y2DL")]
    public async Task AboutY2DL()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        var version = fileVersionInfo.ProductVersion;
        var vsl = fileVersionInfo.FileVersion;

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle($"Y2DL.Minimal by jbcarreon123")
            .WithDescription($"A temporary fix for the inst-1 burdens.\n**NOT INTENDED TO BE PUBLISHED ON GITHUB!**\nPlease do not publish this in any way.")
            .WithThumbnailUrl("https://jbcarreon123.github.io/Y2DL.png")
            .AddField("Thanks to:", "Moore, Jax, Nether, Rob, Steven, Orion Nova, Fake, Oreo, and you for using the bot even if it's majority broken!", true)
            .Build());
    }

    [SlashCommand("stats", "Check bot stats")]
    public async Task Stats() {
        var latency = Context.Client.Latency;
        var guilds = Context.Client.Guilds.Count();

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle("Bot Stats")
            .AddField("Latency to HeartbeatAck", $"{latency}ms", true)
            .AddField("Guild Count", guilds, true)
            .Build());
    }

    [SlashCommand("username", "Change bot username")]
    public async Task ChangeUsername(string username) {
        await Context.Guild.CurrentUser.ModifyAsync(f => f.Nickname = username);

        await RespondAsync("Done!");
    }
}