using Newtonsoft.Json;

namespace Y2DL.Minimal.Models;

public class Title
{
    [JsonProperty("title")]
    public string? TitleName { get; set; }

    [JsonProperty("original")]
    public bool? Original { get; set; }

    [JsonProperty("votes")]
    public int? Votes { get; set; }

    [JsonProperty("locked")]
    public bool? Locked { get; set; }

    [JsonProperty("UUID")]
    public string? UUID { get; set; }
}

public class Thumbnail
{
    [JsonProperty("timestamp")]
    public double? Timestamp { get; set; }

    [JsonProperty("original")]
    public bool? Original { get; set; }

    [JsonProperty("votes")]
    public int? Votes { get; set; }

    [JsonProperty("locked")]
    public bool? Locked { get; set; }

    [JsonProperty("UUID")]
    public string? UUID { get; set; }
}

public class DeArrow
{
    [JsonProperty("titles")]
    public List<Title>? Titles { get; set; }

    public Title? FirstTitle {
        get => Titles?[0] ?? null;
    }

    [JsonProperty("thumbnails")]
    public List<Thumbnail>? Thumbnails { get; set; }

    public Thumbnail? FirstThumbnail {
        get => Thumbnails?[0] ?? null;
    }

    [JsonProperty("randomTime")]
    public double? RandomTime { get; set; }

    [JsonProperty("videoDuration")]
    public double? VideoDuration { get; set; }

    public static async Task<DeArrow> GetVideo(string videoId) {
        var r = new DeArrow();
        using (var httpClient = new HttpClient()) {
            try {
                var req = await httpClient.GetStringAsync("https://sponsor.ajay.app/api/branding?videoID=" + videoId);
                r = JsonConvert.DeserializeObject<DeArrow>(req);
            } catch {}
        }
        return r;
    }
}
