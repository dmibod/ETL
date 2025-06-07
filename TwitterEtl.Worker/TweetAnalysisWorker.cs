using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class TweetAnalysisWorker : BackgroundService
{
    private readonly ILogger<TweetAnalysisWorker> _logger;
    private readonly TwitterClient _twitterClient;
    private readonly ITweetFilter _tweetFilter;
    private readonly TweetStateRepository _stateRepository;
    private readonly string[] _accounts = new[] { "@example" };

    public TweetAnalysisWorker(ILogger<TweetAnalysisWorker> logger, ITweetFilter tweetFilter)
    {
        _logger = logger;
        _tweetFilter = tweetFilter;
        _stateRepository = new TweetStateRepository();
        // In real code, keys should come from configuration or secrets
        _twitterClient = new TwitterClient(
            Environment.GetEnvironmentVariable("TWITTER_API_KEY"),
            Environment.GetEnvironmentVariable("TWITTER_API_SECRET"),
            Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN"),
            Environment.GetEnvironmentVariable("TWITTER_ACCESS_SECRET"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var account in _accounts)
            {
                try
                {
                    var user = await _twitterClient.Users.GetUserAsync(account);
                    var timeline = await _twitterClient.Timelines.GetUserTimelineAsync(user);

                    var lastId = _stateRepository.GetLastTweetId(account);
                    var newTweets = timeline
                        .Where(t => t.Id > lastId)
                        .OrderBy(t => t.Id);
                    long maxId = lastId;

                    foreach (var tweet in newTweets)
                    {
                        if (await _tweetFilter.ShouldProcessAsync(tweet))
                        {
                            await WriteResultAsync(tweet);
                        }

                        if (tweet.Id > maxId)
                        {
                            maxId = tweet.Id;
                        }
                    }

                    if (maxId > lastId)
                    {
                        _stateRepository.SetLastTweetId(account, maxId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing account {Account}", account);
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }


    private static async Task WriteResultAsync(ITweet tweet)
    {
        var record = new { tweet.CreatedAt, tweet.FullText, tweet.CreatedBy.ScreenName };
        await File.AppendAllTextAsync("results.jsonl", JsonSerializer.Serialize(record) + "\n");
    }
}
