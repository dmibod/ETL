using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Twitter.Client.Models;

namespace Twitter.Client;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetUserIdByNameResponse?> GetUserIdByNameAsync(GetUserIdByNameRequest request)
    {
        var url = $"https://api.twitter.com/2/users/by/username/{Uri.EscapeDataString(request.Username)}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DataResponse<GetUserIdByNameResponse>>(json)?.Data;
    }

    public async Task<GetPostsByUserIdResponse?> GetPostsByUserIdAsync(GetPostsByUserIdRequest request)
    {
        var url = $"https://api.twitter.com/2/users/{Uri.EscapeDataString(request.UserId)}/tweets?max_results={request.MaxResults}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetPostsByUserIdResponse>(json);
    }
    
    private class DataResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
