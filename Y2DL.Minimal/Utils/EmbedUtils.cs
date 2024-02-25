using System.Drawing;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Y2DL.Minimal.Models;
using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using Y2DL.Minimal.SmartFormatters;

namespace Y2DL.Minimal.Utils;

public static class EmbedUtils
{
    public static Discord.Embed GenerateHelpEmbed()
    {
        EmbedBuilder embedBuilder = new EmbedBuilder()
        {
            Title = "Y2DL uses slash commands!",
            Description = "If you didn't know, Y2DL uses Slash commands.\r\n" +
                        "Slash commands is a really useful thing and it makes things easier!\r\n" +
                        "Please use the slash command menu to see what Y2DL can offer.",
            ImageUrl = "https://jbcarreon123.github.io/y2dl_slash.gif"
        };

        return embedBuilder.Build();
    }
    
    public static EmbedBuilder ToDiscordEmbedBuilder(this Embeds embeds, YoutubeChannel channel)
    {
        try
        {
            Smart.Default.AddExtensions(new LimitFormatter());
            Smart.Default.AddExtensions(new ToSnowflakeFormatter());
            
            var color = ColorTranslator.FromHtml(embeds.Color);

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = embeds.Author is not null ? Smart.Format(embeds.Author, channel) : "",
                    Url = embeds.AuthorUrl is not null ? Smart.Format(embeds.AuthorUrl, channel) : "",
                    IconUrl = embeds.AuthorAvatarUrl is not null ? Smart.Format(embeds.AuthorAvatarUrl, channel) : ""
                },
                Title = embeds.Title is not null ? Smart.Format(embeds.Title, channel) : "",
                Url = embeds.TitleUrl is not null ? Smart.Format(embeds.TitleUrl, channel) : "",
                Description = embeds.Description is not null ? Smart.Format(embeds.Description, channel) : "",
                Color = new Discord.Color(color.R, color.G, color.B),
                ImageUrl = embeds.ImageUrl is not null ? Smart.Format(embeds.ImageUrl, channel) : "",
                ThumbnailUrl = embeds.ThumbnailUrl is not null ? Smart.Format(embeds.ThumbnailUrl, channel) : "",
                Footer = new EmbedFooterBuilder()
                {
                    Text = embeds.Footer is not null ? Smart.Format(embeds.Footer, channel) : "",
                    IconUrl = embeds.FooterUrl is not null ? Smart.Format(embeds.FooterUrl, channel) : ""
                }
            };

            if (embeds.Fields is not null)
            {
                foreach (EmbedFields fields in embeds.Fields)
                {
                    if (!fields.Name.IsNullOrWhitespace() && !fields.Value.IsNullOrWhitespace()) {
                        embedBuilder.AddField(Smart.Format(fields.Name, channel), Smart.Format(fields.Value, channel),
                            fields.Inline);
                    }
                }
            }

            Log.Debug($"Y2DL: Converted Embed to EmbedBuilder for YouTubeChannel {channel.Name}");

            return embedBuilder;
        }
        catch (Exception e)
        {
            Log.Warning(e, $"An error occured while converting");

            return default;
        }
    }
}