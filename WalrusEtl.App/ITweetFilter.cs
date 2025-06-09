using System.Threading.Tasks;
using Twitter.Client.Models;

public interface ITweetFilter
{
    Task<bool> ShouldProcessAsync(GetPostsByUserIdResponse.Item tweet);
}
