// IT WON'T HAPPEN AGAIN, i said.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Y2DL.Minimal.Models;

namespace Y2DL.Minimal.Database;

public class Y2dlDbContext : DbContext 
{
    public DbSet<DynamicChannelInfoMessages> DynamicChannelInfoMessages { get; set; }
    public DbSet<ChannelReleasesLatestVideo> ChannelReleasesLatestVideos { get; set; }
    
    public string DbPath { get; set; }
    
    public Y2dlDbContext()
    {
        // var currentDirectory = Directory.GetCurrentDirectory();// + "/Database";
        //if (!Directory.Exists(currentDirectory)) {
        //    Directory.CreateDirectory(currentDirectory);
        //}
        var path = System.IO.Directory.GetCurrentDirectory();
        DbPath = path + "/Y2DL.db";
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class DynamicChannelInfoMessages
{
    public ulong ChannelId { get; set; } 
    public ulong MessageId { get; set; }
    public string YoutubeChannelId { get; set; }
        
    [Key]
    public string Hash { get; set; }
}

public class ChannelReleasesLatestVideo
{
    [Key]
    public string? VideoId { get; set; }
    public string? ChannelId { get; set; } 
}