namespace Twitter.Client.Models;

public class GetPostsByUserIdRequest
{
    public required string UserId { get; set; }
    public int MaxResults { get; set; } = 10;
}
