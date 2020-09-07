using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        [Fact]
        public async void GivenRequestIsConfiguredWithSpecificContentTypeAndRequestHasDifferentContentType_UnsupportedMediaTypeIsReturned()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(
                    HttpMethod.Put,
                    "/api/hello?foo=bar",
                    "application/json")
                .With(HttpStatusCode.NoContent);

            var response = await client.PutAsync("https://tempuri.org/api/hello?foo=bar", new StringContent("foo", Encoding.ASCII,"text/plain"));

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async void GivenRequestIsConfiguredWithRouteParameters_RequestIsHandled()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async void GivenRequestIsConfiguredWithCookie_SetCookieHeaderIsInResponse()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}")
                .AndCookie("cookie-name", "cookie-value");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Set-Cookie")
                .Which
                .Value
                .First()
                .Should()
                .Be("cookie-name=cookie-value");
        }

        [Fact]
        public async void GivenRequestIsConfiguredWithCookieThatExpires_SetCookieHeaderIsInResponse()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);
            var expiresAt = DateTime.UtcNow.AddHours(1);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}")
                .AndCookie("cookie-name", "cookie-value", expiresAt);

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Set-Cookie")
                .Which
                .Value
                .First()
                .Should()
                .Be($"cookie-name=cookie-value; Expires={expiresAt:R}"); // See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Date
        }

        [Fact]
        public async void GivenRequestIsConfiguredWithCookieForDomain_SetCookieHeaderIsInResponse()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);
            var expiresAt = DateTime.UtcNow.AddHours(1);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}")
                .AndCookie("cookie-name", "cookie-value", domain: "jedlix.com");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Set-Cookie")
                .Which
                .Value
                .First()
                .Should()
                .Be($"cookie-name=cookie-value; Domain=jedlix.com");
        }

        [Fact]
        public async void GivenRequestIsConfiguredWithCookieForDomainAndPath_SetCookieHeaderIsInResponse()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);
            var expiresAt = DateTime.UtcNow.AddHours(1);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}")
                .AndCookie("cookie-name", "cookie-value", domain: "jedlix.com", path: "/some/path");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Set-Cookie")
                .Which
                .Value
                .First()
                .Should()
                .Be($"cookie-name=cookie-value; Path=/some/path;Domain=jedlix.com");
        }

        [Fact]
        public async void GivenRequestHasQueryStringParameterForAnyValue_RequestIsMatched()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}?key=value&someFilter=another")
                .ForQueryStringParameter("key").WithAnyValue()
                .ForQueryStringParameter("someFilter").WithValue("another")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123?key=SOMETHINGELSE&someFilter=another");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async void GivenRequestHasQueryStringParameterForAnyValueButOtherParameterDoesNotMatch_ResponseIsInternalServerErrorBecauseNoResponseMatches()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}?key=value&someFilter=another")
                .ForQueryStringParameter("key").WithAnyValue()
                .ForQueryStringParameter("someFilter").WithValue("another")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123?key=SOMETHINGELSE&someFilter=SOMETHINGELSE");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.InternalServerError);
        }
    }
}