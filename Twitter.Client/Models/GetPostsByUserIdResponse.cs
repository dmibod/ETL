using System.Collections.Generic;

namespace Twitter.Client.Models;

public class GetPostsByUserIdResponse
{
    public List<Item> Items { get; set; } = new();

    public class Item
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
    }
}
