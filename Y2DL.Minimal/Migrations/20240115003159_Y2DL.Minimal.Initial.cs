using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y2DL.Minimal.Migrations
{
    public partial class Y2DLMinimalInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelReleasesLatestVideos",
                columns: table => new
                {
                    VideoId = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelReleasesLatestVideos", x => x.VideoId);
                });

            migrationBuilder.CreateTable(
                name: "DynamicChannelInfoMessages",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    YoutubeChannelId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicChannelInfoMessages", x => x.Hash);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelReleasesLatestVideos");

            migrationBuilder.DropTable(
                name: "DynamicChannelInfoMessages");
        }
    }
}
