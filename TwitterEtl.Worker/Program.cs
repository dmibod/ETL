using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<ITweetFilter, DefaultTweetFilter>();
        services.AddHostedService<TweetAnalysisWorker>();
    })
    .Build();

await host.RunAsync();
