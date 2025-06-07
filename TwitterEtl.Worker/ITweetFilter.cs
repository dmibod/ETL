using System.Threading.Tasks;
using Tweetinvi.Models;

public interface ITweetFilter
{
    Task<bool> ShouldProcessAsync(ITweet tweet);
}
