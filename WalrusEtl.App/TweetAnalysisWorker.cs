using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Twitter.Client;
using Twitter.Client.Models;

public class TweetAnalysisWorker : BackgroundService
{
    private readonly ILogger<TweetAnalysisWorker> _logger;
    private readonly ApiClient _twitterClient;
    private readonly ITweetFilter _tweetFilter;
    private readonly TweetStateRepository _stateRepository;
    private readonly string[] _accounts = new[] { "SuiNetwork" };

    public TweetAnalysisWorker(ILogger<TweetAnalysisWorker> logger, ITweetFilter tweetFilter)
    {
        _logger = logger;
        _tweetFilter = tweetFilter;
        _stateRepository = new TweetStateRepository();
        // In real code, keys should come from configuration or secrets
        var httpClient = new HttpClient(/*handler*/);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("TWITTER_BEARER_TOKEN")}");
        _twitterClient = new ApiClient(httpClient);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var account in _accounts)
            {
                try
                {
                    var user = await _twitterClient.GetUserIdByNameAsync(new GetUserIdByNameRequest{Username = account});
                    var timeline = await _twitterClient.GetPostsByUserIdAsync(new GetPostsByUserIdRequest{UserId = user.Id, MaxResults = 25});

                    var lastId = _stateRepository.GetLastTweetId(account);
                    var newTweets = timeline.Items
                        .Where(t => long.Parse(t.Id) > lastId)
                        .OrderBy(t => t.Id);
                    long maxId = lastId;

                    foreach (var tweet in newTweets)
                    {
                        if (await _tweetFilter.ShouldProcessAsync(tweet))
                        {
                            await WriteResultAsync(tweet);
                        }

                        var id = long.Parse(tweet.Id);
                        
                        if (id > maxId)
                        {
                            maxId = id;
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


    private static async Task WriteResultAsync(GetPostsByUserIdResponse.Item tweet)
    {
        var record = new { tweet.Id, tweet.Text };
        await File.AppendAllTextAsync("results.jsonl", JsonSerializer.Serialize(record) + "\n");
    }
}
