using System.Text.Json.Serialization;

namespace Twitter.Client.Models;

public class GetUserIdByNameResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("username")]
    public string? Username { get; set; }
}
