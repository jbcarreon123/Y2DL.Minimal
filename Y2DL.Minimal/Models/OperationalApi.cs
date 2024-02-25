using Newtonsoft.Json;
using System.Collections.Generic;

namespace Y2DL.Minimal.Models;

public class OperationalApiSnippet
{
    [JsonProperty("avatar")]
    public List<Image> Avatar { get; set; }

    [JsonProperty("banner")]
    public List<Image> Banner { get; set; }

    [JsonProperty("tvBanner")]
    public List<Image> TvBanner { get; set; }

    [JsonProperty("mobileBanner")]
    public List<Image> MobileBanner { get; set; }
}

public class Image
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }
}

public class Channel
{
    [JsonProperty("kind")]
    public string Kind { get; set; }

    [JsonProperty("etag")]
    public string Etag { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("snippet")]
    public OperationalApiSnippet Snippet { get; set; }
}

public class ChannelListResponse
{
    [JsonProperty("kind")]
    public string Kind { get; set; }

    [JsonProperty("etag")]
    public string Etag { get; set; }

    [JsonProperty("items")]
    public List<Channel> Items { get; set; }
}
