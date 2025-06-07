using Azure.AI.OpenAI;
using Tweetinvi.Models;

public class DefaultTweetFilter : ITweetFilter
{
    private readonly OpenAIClient? _openAiClient;

    public DefaultTweetFilter()
    {
        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
        var key = Environment.GetEnvironmentVariable("OPENAI_KEY");
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(key))
        {
            _openAiClient = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));
        }
    }

    public async Task<bool> ShouldProcessAsync(ITweet tweet)
    {
        if (_openAiClient == null)
        {
            // Fallback simple keyword check
            return tweet.FullText.Contains("AI", StringComparison.OrdinalIgnoreCase);
        }

        var prompt = $"Does this tweet mention AI or machine learning? {tweet.FullText}";
        var response = await _openAiClient.GetCompletionsAsync(
            Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT"),
            prompt);
        var text = response.Value.Choices[0].Text;
        return text.Contains("yes", StringComparison.OrdinalIgnoreCase);
    }
}
