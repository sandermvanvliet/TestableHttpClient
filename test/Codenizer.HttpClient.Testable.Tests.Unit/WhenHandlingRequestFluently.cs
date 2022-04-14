using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenHandlingRequestFluently
    {
        [Fact]
        public void UsingGet_MethodOfRequestBuilderIsGet()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Get())
                .Method
                .Should()
                .Be(HttpMethod.Get);
        }

        [Fact]
        public void UsingPut_MethodOfRequestBuilderIsPut()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Put())
                .Method
                .Should()
                .Be(HttpMethod.Put);
        }

        [Fact]
        public void UsingPost_MethodOfRequestBuilderIsPost()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Post())
                .Method
                .Should()
                .Be(HttpMethod.Post);
        }

        [Fact]
        public void UsingDelete_MethodOfRequestBuilderIsDelete()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Delete())
                .Method
                .Should()
                .Be(HttpMethod.Delete);
        }

        [Fact]
        public void UsingHead_MethodOfRequestBuilderIsHead()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Head())
                .Method
                .Should()
                .Be(HttpMethod.Head);
        }

        [Fact]
        public void UsingOptions_MethodOfRequestBuilderIsOptions()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Options())
                .Method
                .Should()
                .Be(HttpMethod.Options);
        }

        [Fact]
        public void GivenUrlWithoutQueryParameters_PathAndQueryIsSet()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Get().ForUrl("/derp"))
                .PathAndQuery
                .Should()
                .Be("/derp");
        }

        [Fact]
        public void GivenUrlWithQueryParameters_PathAndQueryIsSet()
        {
            var handler = new TestableMessageHandler();

            var requestBuilder = ((RequestBuilder)handler.RespondTo().Get().ForUrl("/derp?foo=bar&bar=baz"));

            requestBuilder
                .PathAndQuery
                .Should()
                .Be("/derp");

            requestBuilder
                .QueryParameters
                .Select(kv => kv.Key)
                .Should()
                .Contain("foo", "bar");
        }

        [Fact]
        public void GivenContentType_ContentTypeIsSet()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Post().ForUrl("/derp").AndContentType("foo/bar"))
                .ContentType
                .Should()
                .Be("foo/bar");
        }

        [Fact]
        public void GivenContentTypeAndMethodIsGet_ArgumentExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();

            Action action = () => handler.RespondTo().Get().ForUrl("/derp").AndContentType("foo/bar");

            action
                .Should()
                .Throw<ArgumentException>("a GET cannot have a content type");
        }

        [Fact]
        public void GivenContentTypeAndMethodIsHead_ArgumentExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();

            Action action = () => handler.RespondTo().Get().ForUrl("/derp").AndContentType("foo/bar");

            action
                .Should()
                .Throw<ArgumentException>("a GET cannot have a content type");
        }

        [Fact]
        public void GivenContentTypeAndMethodIsDelete_ArgumentExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();

            Action action = () => handler.RespondTo().Get().ForUrl("/derp").AndContentType("foo/bar");

            action
                .Should()
                .Throw<ArgumentException>("a GET cannot have a content type");
        }
        

        [Fact]
        public void GivenAcceptType_AcceptTypeIsSet()
        {
            var handler = new TestableMessageHandler();

            ((RequestBuilder)handler.RespondTo().Post().ForUrl("/derp").Accepting("foo/bar"))
                .Accept
                .Should()
                .Be("foo/bar");
        }

        [Fact]
        public async void GivenConfiguredResponseCodeOk_ResponseStatusIsOk()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
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
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
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
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
                .With(HttpStatusCode.OK)
                .AndContent("application/json", "data");

            var response = await client.GetAsync("https://tempuri.org/api/hello?foo=bar");

            response
                .Content
                .Headers
                .ContentType
                .Should()
                .Be(new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" });
        }

        [Fact]
        public async void GivenResponseHeaderConfigured_ResponseContainsHeader()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
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
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
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
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
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
                .RespondTo().Get().ForUrl("/api/hello?foo=bar")
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
                .RespondTo()
                .Put()
                .ForUrl("/api/hello?foo=bar")
                .AndContentType("application/json")
                .With(HttpStatusCode.NoContent);

            var response = await client.PutAsync("https://tempuri.org/api/hello?foo=bar", new StringContent("foo", Encoding.ASCII, "text/plain"));

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
                .RespondTo().Get().ForUrl("/api/entity/{id}")
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
                .RespondTo().Get().ForUrl("/api/entity/{id}")
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
                .RespondTo().Get().ForUrl("/api/entity/{id}")
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
                .RespondTo().Get().ForUrl("/api/entity/{id}")
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

            handler
                .RespondTo().Get().ForUrl("/api/entity/{id}")
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
                .RespondTo().Get().ForUrl("/api/entity/{id}?key=value&someFilter=another")
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
                .RespondTo().Get().ForUrl("/api/entity/{id}?key=value&someFilter=another")
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
                .RespondTo().Get().ForUrl("/api/entity/{id}?key=value&key=othervalue&key=specificvalue")
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
                .RespondTo()
                .Get()
                .ForUrl(
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
                .RespondTo().Get().ForUrl("/api/entity/blah?foo&bar&baz")
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
                .RespondTo().Get().ForUrl("/api/entity/blah")
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
                .RespondTo().Get().ForUrl("/api/entity/blah")
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
                .RespondTo().Get().ForUrl("/api/entity/blah")
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
                .RespondTo().Get().ForUrl("/api/entity/blah")
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
                .ContainInOrder(new byte[] { 0x1, 0x2, 0x3 });
        }
    }
}