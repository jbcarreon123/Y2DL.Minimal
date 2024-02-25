using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;

namespace Y2DL.Minimal.Models;

public class ReturnYouTubeDislike
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("dateCreated")]
    public DateTime DateCreated { get; set; }

    [JsonProperty("likes")]
    public int Likes { get; set; }

    [JsonProperty("dislikes")]
    public int Dislikes { get; set; }

    [JsonProperty("rating")]
    public double Rating { get; set; }

    [JsonProperty("viewCount")]
    public int ViewCount { get; set; }

    [JsonProperty("deleted")]
    public bool Deleted { get; set; }

    public static async Task<ReturnYouTubeDislike> GetDislike(string videoId) {
        var r = new ReturnYouTubeDislike();
        using (var httpClient = new HttpClient()) {
            try {
                var req = await httpClient.GetStringAsync("https://returnyoutubedislikeapi.com/Votes?videoId=" + videoId);
                r = JsonConvert.DeserializeObject<ReturnYouTubeDislike>(req);
            } catch {}
        }
        return r;
    }

}
