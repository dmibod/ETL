using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twitter.Client;
using Twitter.Client.Models;

namespace Twitter.Client.Tests;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    public HttpRequestMessage? Request { get; private set; }

    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;
        return Task.FromResult(_response);
    }
}

[TestClass]
public class ApiClientTests
{
    [TestMethod]
    public async Task GetUseIdByNameAsync_ReturnsResponseAndUsesCorrectRequest()
    {
        var json = "{\"Id\":\"123\",\"Name\":\"John\",\"Username\":\"john\"}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler);
        var client = new ApiClient(httpClient);

        var result = await client.GetUseIdByNameAsync(new GetUseIdByNameRequest { Username = "john" });

        Assert.IsNotNull(result);
        Assert.AreEqual("123", result!.Id);
        Assert.AreEqual("John", result.Name);
        Assert.AreEqual("john", result.Username);
        Assert.IsNotNull(handler.Request);
        Assert.AreEqual(HttpMethod.Get, handler.Request!.Method);
        Assert.AreEqual("https://api.twitter.com/2/users/by/username/john", handler.Request.RequestUri!.ToString());
        Assert.IsTrue(handler.Request.Headers.Accept.Any(h => h.MediaType == "application/json"));
    }

    [TestMethod]
    public async Task GetPostsByUserIdAsync_ReturnsItemsAndUsesCorrectRequest()
    {
        var json = "{\"Items\":[{\"Id\":\"1\",\"Text\":\"hello\"}]}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler);
        var client = new ApiClient(httpClient);

        var result = await client.GetPostsByUserIdAsync(new GetPostsByUserIdRequest { UserId = "42", MaxResults = 5 });

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result!.Items.Count);
        Assert.AreEqual("1", result.Items[0].Id);
        Assert.AreEqual("hello", result.Items[0].Text);
        Assert.IsNotNull(handler.Request);
        Assert.AreEqual(HttpMethod.Get, handler.Request!.Method);
        Assert.AreEqual("https://api.twitter.com/2/users/42/tweets?max_results=5", handler.Request.RequestUri!.ToString());
        Assert.IsTrue(handler.Request.Headers.Accept.Any(h => h.MediaType == "application/json"));
    }
}
