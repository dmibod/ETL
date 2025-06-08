namespace Twitter.Client.Models;

public class GetUseIdByNameResponse
{
    public Data? Data { get; set; }

    public class Data
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
    }
}
