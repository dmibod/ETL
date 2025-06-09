using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Twitter.Client.Models;

public class GetPostsByUserIdResponse
{
    [JsonPropertyName("data")]
    public List<Item> Items { get; set; } = new();

    public class Item
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
