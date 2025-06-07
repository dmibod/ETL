using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TwitterClient _twitterClient;
    private readonly ITweetFilter _tweetFilter;
    private readonly string[] _accounts = new[] { "@example" };

    public Worker(ILogger<Worker> logger, ITweetFilter tweetFilter)
    {
        _logger = logger;
        _tweetFilter = tweetFilter;
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
                    foreach (var tweet in timeline)
                    {
                        if (await _tweetFilter.ShouldProcessAsync(tweet))
                        {
                            await WriteResultAsync(tweet);
                        }
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
        var record = new { tweet.CreatedAt, tweet.FullText, tweet.Author.ScreenName };
        await File.AppendAllTextAsync("results.jsonl", JsonSerializer.Serialize(record) + "\n");
    }
}
