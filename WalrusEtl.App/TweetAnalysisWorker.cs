using System;
using System.Collections.Generic;
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
    private readonly HashSet<string> _accounts = new()
    {
        "altcoinbuzzio",
        "a16z",
        "aeyakovenko",
        "AriannaSimpson",
        "Atoma_Network",
        "blockbaseco",
        "bluefinapp",
        "CertiKCommunity",
        "Claynosaurz",
        "coingecko",
        "CoinMarketCap",
        "CetusProtocol",
        "decryptmedia",
        "imperator_co",
        "linera_io",
        "ma2bd",
        "martypartymusic",
        "Mysten_Labs",
        "navi_protocol",
        "PythNetwork",
        "SammyMzy",
        "Scallop_io",
        "StuFinancestech",
        "SuiNetwork",
        "suilendprotocol",
        "syke0x",
        "WalrusProtocol"
    };

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
                            await WriteResultAsync(tweet, account);
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
                
                // due to the Twitter API rate limits, we should wait before the next request
                await Task.Delay(TimeSpan.FromMinutes(16), stoppingToken);
            }
        }
    }


    private static async Task WriteResultAsync(GetPostsByUserIdResponse.Item tweet, string account)
    {
        var record = new { tweet.Id, tweet.Text, Author = account };
        await File.AppendAllTextAsync("results.jsonl", JsonSerializer.Serialize(record) + "\n");
    }
}
