using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
#pragma warning disable CS0618 // Because this should remain working until v3.x

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
        public void GivenResponseDurationConfiguredAndHttpClientHasTimeout_OperationCanceledExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler)
            {
                Timeout = TimeSpan.FromMilliseconds(50)
            };

            handler
                .RespondTo("/api/hello?foo=bar")
                .With(HttpStatusCode.NoContent)
                .Taking(TimeSpan.FromMilliseconds(100));

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action
                .Should()
                .Throw<OperationCanceledException>();
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

        [Obsolete("Returning HTTP 415 UnsupportedMediaType should not be handled automatically, you should configure it yourself if your code depends on it")]
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

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}")
                .AndCookie("cookie-name", "cookie-value", domain: "codenizer.nl");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Set-Cookie")
                .Which
                .Value
                .First()
                .Should()
                .Be("cookie-name=cookie-value; Domain=codenizer.nl");
        }

        [Fact]
        public async void GivenRequestIsConfiguredWithCookieForDomainAndPath_SetCookieHeaderIsInResponse()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}")
                .AndCookie("cookie-name", "cookie-value", domain: "codenizer.nl", path: "/some/path");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123");

            response
                .Headers
                .Should()
                .Contain(header => header.Key == "Set-Cookie")
                .Which
                .Value
                .First()
                .Should()
                .Be("cookie-name=cookie-value; Path=/some/path;Domain=codenizer.nl");
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
        public async void GivenRequestHasQueryStringParameterForOverriddenValueFromUri_RequestIsMatched()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}?key=value&someFilter=another")
                .ForQueryStringParameter("key").WithAnyValue()
                .ForQueryStringParameter("someFilter").WithValue("overridden")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123?key=SOMETHINGELSE&someFilter=overridden");

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

        [Fact]
        public async void GivenRequestHasMultipleOccurrencesOfSameQueryParameterAndOneMatchingValue_RequestMatches()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/{id}?key=value&key=othervalue&key=specificvalue")
                .ForQueryStringParameter("key").WithValue("specificvalue")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "{\"foo\":\"bar\"}");

            var response = await client.GetAsync("https://tempuri.org/api/entity/123??key=value&key=othervalue&key=specificvalue");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async void GivenUriHasQueryParameterWithSlashes_OnlyRoutePartIsMatched()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get,
                    "/signin-service/v1/consent/users/840f9c5a-12f6-434f-83c4-6ba605f41092/09b6cbec-cd19-4589-82fd-363dfa8c24da@apps_vw-dilab_com?scopes=address%20profile%20badge%20birthdate%20birthplace%20nationalIdentifier%20nationality%20profession%20email%20vin%20phone%20nickname%20name%20picture%20mbb%20gallery%20openid&relayState=b512822e924ef060fe820c8bbcaabe85859d2035&callback=https://identity.vwgroup.io/oidc/v1/oauth/client/callback&hmac=a63faea3311a0b4296df53ed94d617b241a2e078f080f9997e0d4d9cee2f07f3")
                .With(HttpStatusCode.Found);

            var response = await client.GetAsync("https://identity.vwgroup.io/signin-service/v1/consent/users/840f9c5a-12f6-434f-83c4-6ba605f41092/09b6cbec-cd19-4589-82fd-363dfa8c24da@apps_vw-dilab_com?scopes=address%20profile%20badge%20birthdate%20birthplace%20nationalIdentifier%20nationality%20profession%20email%20vin%20phone%20nickname%20name%20picture%20mbb%20gallery%20openid&relayState=b512822e924ef060fe820c8bbcaabe85859d2035&callback=https://identity.vwgroup.io/oidc/v1/oauth/client/callback&hmac=a63faea3311a0b4296df53ed94d617b241a2e078f080f9997e0d4d9cee2f07f3");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.Found);
        }

        [Fact]
        public async void GivenUriHasQueryParameterWithoutValues_RouteIsMatchedSuccessfully()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah?foo&bar&baz")
                .With(HttpStatusCode.Found);

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah?foo&bar&baz");

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.Found);
        }

        [Fact]
        public async void GivenRequestIsConfiguredToReturnJson_ResponseContentTypeHeaderIsApplicationJson()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndJsonContent(new
                {
                    Foo = "bar"
                });

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah");

            response
                .Content
                .Headers
                .ContentType
                .MediaType
                .Should()
                .Be("application/json");
        }

        [Fact]
        public async void GivenRequestIsConfiguredToReturnJsonWithSpecificSettings_ResponseContentHasExpectedSerializedContent()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndJsonContent(new
                    {
                        FooBar = "bar"
                    },
                    new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        }
                    });

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah");

            response
                .Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult()
                .Should()
                .Be("{\"foo_bar\":\"bar\"}");
        }

        [Fact]
        public async void GivenHandlerIsConfiguredToReturnJsonWithSpecificSettings_ResponseContentHasExpectedSerializedContent()
        {
            var handler = new TestableMessageHandler(new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndJsonContent(new
                    {
                        FooBar = "bar"
                    });

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah");

            response
                .Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult()
                .Should()
                .Be("{\"foo_bar\":\"bar\"}");
        }

        [Fact]
        public async void GivenHandlerIsConfiguredToReturnByteArray_ResponseContentIsByteArrayContent()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndContent("application/octet-stream", new byte[] { 0x1, 0x2, 0x3 });

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah");

            response
                .Content
                .As<ByteArrayContent>()
                .ReadAsByteArrayAsync()
                .GetAwaiter()
                .GetResult()
                .Should()
                .ContainInOrder(new byte[] {0x1, 0x2, 0x3 });
        }

        [Fact]
        public async Task GivenHandlerOnRequest_HandlerIsInvoked()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndContent(
                    "application/json",
                    req => $@"{{""path"":""{req.RequestUri!.PathAndQuery}""}}");

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah");

            var serializedContent = await response.Content.ReadAsStringAsync();

            serializedContent.Should().Be(@"{""path"":""/api/entity/blah""}");
        }

        [Fact]
        public async Task GivenAsyncHandlerOnRequest_HandlerIsInvoked()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Get, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndContent(
                    "application/json",
                    async req =>
                    {
                        await Task.Delay(10);
                        
                        return $@"{{""path"":""{req.RequestUri!.PathAndQuery}""}}";
                    });

            var response = await client.GetAsync("https://tempuri.org/api/entity/blah");

            var serializedContent = await response.Content.ReadAsStringAsync();

            serializedContent.Should().Be(@"{""path"":""/api/entity/blah""}");
        }

        [Fact]
        public async Task GivenAsyncHandlerOnRequestThatReadsRequestContent_ContentCanStillBeInspectedInAssertion()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo(HttpMethod.Post, "/api/entity/blah")
                .With(HttpStatusCode.OK)
                .AndContent(
                    "text/plain",
                    async req => 
                    {
                        var postedContent = await req.Content!.ReadAsStringAsync();

                        return postedContent;
                    });

            await client.PostAsync("https://tempuri.org/api/entity/blah", new StringContent("HELLO WORLD!"));

            var content = await handler
                .Requests
                .Single()
                .Content!
                .ReadAsStringAsync();

            content.Should().Be("HELLO WORLD!");
        }

        [Fact]
        public async Task GivenExpectationForContentAndContentMatches_ConfiguredResponseIsReturned()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo()
                .Post()
                .ForUrl("/search")
                .ForContent(@"{""params"":{""query_string"":""confetti""}}")
                .With(HttpStatusCode.OK);

            var response = await client.PostAsync(
                "https://tempuri.org/search",
                new StringContent(@"{""params"":{""query_string"":""confetti""}}"));

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GivenExpectationWithContentAndContentDoesNotMatch_InternalServerErrorIsReturned()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo()
                .Post()
                .ForUrl("/search")
                .ForContent(@"{""params"":{""query_string"":""confetti""}}")
                .With(HttpStatusCode.OK);

            var response = await client.PostAsync(
                "https://tempuri.org/search",
                new StringContent(@"{""boo"":""baz""}"));

            response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.InternalServerError);
        }
    }
}