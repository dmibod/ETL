using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<ITweetFilter, DefaultTweetFilter>();
        services.AddHostedService<TweetAnalysisWorker>();
    })
    .Build();

await host.RunAsync();
