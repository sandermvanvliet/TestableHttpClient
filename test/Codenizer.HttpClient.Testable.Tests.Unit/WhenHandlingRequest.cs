using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenHandlingRequest
    {
        [Fact]
        public async void GivenConfiguredResponseCodeOk_ResponseStatusIsOk()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.OK);

            var response = await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async void GivenConfiguredResponseBody_ResponseContainsBody()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.OK)
                .AndContent("text/plain", "data");

            var response = await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            response
                .Content
                .ReadAsStringAsync()
                .Result
                .Should()
                .Be("data");
        }

        [Fact]
        public async void GivenResponseMediaTypeIsApplicationJson_ContentTypeIsSetWithCharsetOption()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "data");

            var response = await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            response
                .Content
                .Headers
                .ContentType
                .Should()
                .Be(new MediaTypeHeaderValue("application/json") {CharSet = "utf-8"});
        }

        [Fact]
        public async void GivenResponseHeaderConfigured_ResponseContainsHeader()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.NoContent)
                .AndHeaders(new Dictionary<string, string>
                {
                    {"Test-Header", "SomeValue"}
                });

            var response = await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Test-Header");
        }

        [Fact]
        public async void GivenResponseDurationConfigured_ResponseTakesAtLeastDuration()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.NoContent)
                .AndHeaders(new Dictionary<string, string>
                {
                    {"Test-Header", "SomeValue"}
                })
                .Taking(TimeSpan.FromMilliseconds(100));

            var stopwatch = Stopwatch.StartNew();

            await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            stopwatch.Stop();

            stopwatch
                .ElapsedMilliseconds
                .Should()
                .BeGreaterOrEqualTo(100);
        }

        [Fact]
        public async void GivenWhenCalledActionConfigured_ActionIsCalledWhenRequestIsMade()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);
            var wasCalled = false;

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.NoContent)
                .WhenCalled(request => wasCalled = true);

            await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            wasCalled
                .Should()
                .BeTrue();
        }
    }
}