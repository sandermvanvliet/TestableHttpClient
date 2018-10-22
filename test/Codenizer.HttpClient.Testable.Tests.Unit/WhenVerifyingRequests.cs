using System.Linq;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenVerifyingRequests
    {
        private MessageHandler _handler;
        private System.Net.Http.HttpClient _client;

        public WhenVerifyingRequests()
        {
            _handler = new MessageHandler();
            _client = new System.Net.Http.HttpClient(_handler);
            _client.BaseAddress = new System.Uri("https://tempuri.org/");

            _handler
                .RespondTo(HttpMethod.Get, "/api/info")
                .With(HttpStatusCode.OK);
        }

        [Fact]
        public async void GivenRequest_RequestIsCaptured()
        {
            await _client.GetAsync("/api/info");

            _handler
                .Requests
                .Should()
                .Contain(req => req.RequestUri.PathAndQuery == "/api/info");
        }

        [Fact]
        public async void GivenRequestWithHeaders_RequestHeadersCaptured()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/info");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Test-Header", "Value");
            
            await _client.SendAsync(request);

            _handler
                .Requests
                .Single()
                .Headers
                .Should()
                .Contain(h => h.Key == "Test-Header")
                .And
                .Contain(h => h.Key == "Accept");
        }

        [Fact]
        public async void GivenRequestWithStringContent_CapturedRequestContainsContent()
        {
            await _client.PostAsync("/api/info", new StringContent("test data"));

            _handler
                .Requests
                .Single()
                .GetData()
                .Should()
                .Be("test data");
        }

        [Fact]
        public async void GivenRequestWithByteArrayContent_CapturedRequestContainsContent()
        {
            var content = new byte[] { 0x1, 0x2, 0x3, 0x4 };

            await _client.PostAsync("/api/info", new ByteArrayContent(content));

            _handler
                .Requests
                .Single()
                .GetData()
                .Should()
                .Be(content);
        }
    }
}