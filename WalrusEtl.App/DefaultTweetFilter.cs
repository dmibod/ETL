using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Twitter.Client.Models;

public class DefaultTweetFilter : ITweetFilter
{
    private readonly OpenAIClient? _openAiClient;

    public DefaultTweetFilter()
    {
        var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(key))
        {
            _openAiClient = new OpenAIClient(key);
        }
    }

    public async Task<bool> ShouldProcessAsync(GetPostsByUserIdResponse.Item tweet)
    {
        if (_openAiClient == null)
        {
            // Fallback simple keyword check
            return tweet.Text.ToLower().Contains("walrus", StringComparison.OrdinalIgnoreCase);
        }

        var prompt =
            $"Answer only yes or no.\nDoes the following tweet mention Walrus Protocol?\n----------------\nTweet: \"{tweet.Text}\"";
        var response = await _openAiClient.GetCompletionsAsync("gpt-4o-mini", new CompletionsOptions
        {
            Prompts = { prompt },
            ChoicesPerPrompt = 1,
            Temperature = 0.0f
        });
        var text = response.Value.Choices[0].Text;
        return text.ToLower().Contains("yes", StringComparison.OrdinalIgnoreCase);
    }
}
