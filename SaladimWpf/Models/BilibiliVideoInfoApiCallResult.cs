namespace SaladimWpf.Models;
#nullable disable

using System.Text.Json.Serialization;

public class BilibiliVideoInfoApiCallResult
{
    public class Root
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("ttl")]
        public long Ttl { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("bvid")]
        public string Bvid { get; set; }

        [JsonPropertyName("aid")]
        public long Aid { get; set; }

        [JsonPropertyName("videos")]
        public long Videos { get; set; }

        [JsonPropertyName("tname")]
        public string Tname { get; set; }

        [JsonPropertyName("copyright")]
        public long Copyright { get; set; }

        [JsonPropertyName("pic")]
        public string Pic { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("pubdate")]
        public long Pubdate { get; set; }

        [JsonPropertyName("ctime")]
        public long Ctime { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("desc_v2")]
        public DescV2[] DescV2 { get; set; }

        [JsonPropertyName("state")]
        public long State { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("rights")]
        public Dictionary<string, long> Rights { get; set; }

        [JsonPropertyName("owner")]
        public Owner Owner { get; set; }

        [JsonPropertyName("stat")]
        public Stat Stat { get; set; }

        [JsonPropertyName("dynamic")]
        public string Dynamic { get; set; }

        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        [JsonPropertyName("pages")]
        public Page[] Pages { get; set; }
    }

    public class DescV2
    {
        [JsonPropertyName("raw_text")]
        public string RawText { get; set; }

        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("biz_id")]
        public long BizId { get; set; }
    }

    public class Dimension
    {
        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("rotate")]
        public long Rotate { get; set; }
    }

    public class Owner
    {
        [JsonPropertyName("mid")]
        public long Mid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("face")]
        public string Face { get; set; }
    }

    public class Page
    {
        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        [JsonPropertyName("page")]
        public long PagePage { get; set; }

        [JsonPropertyName("part")]
        public string Part { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("vid")]
        public string Vid { get; set; }

        [JsonPropertyName("weblink")]
        public string Weblink { get; set; }

        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }
    }

    public class Stat
    {
        [JsonPropertyName("aid")]
        public long Aid { get; set; }

        [JsonPropertyName("view")]
        public long View { get; set; }

        [JsonPropertyName("danmaku")]
        public long Danmaku { get; set; }

        [JsonPropertyName("reply")]
        public long Reply { get; set; }

        [JsonPropertyName("favorite")]
        public long Favorite { get; set; }

        [JsonPropertyName("coin")]
        public long Coin { get; set; }

        [JsonPropertyName("share")]
        public long Share { get; set; }

        [JsonPropertyName("now_rank")]
        public long NowRank { get; set; }

        [JsonPropertyName("his_rank")]
        public long HisRank { get; set; }

        [JsonPropertyName("like")]
        public long Like { get; set; }

        [JsonPropertyName("dislike")]
        public long Dislike { get; set; }

        [JsonPropertyName("evaluation")]
        public string Evaluation { get; set; }

        [JsonPropertyName("argue_msg")]
        public string ArgueMsg { get; set; }
    }
}