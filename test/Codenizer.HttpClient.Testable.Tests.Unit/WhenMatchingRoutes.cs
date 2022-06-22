using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public static class ConfiguredRequestsExtensions
    {
        internal static RequestBuilder? Match(this ConfiguredRequests configuredRequests, HttpMethod method, string uri,
            string? accept)
        {
            var requestMessage = new HttpRequestMessage(method, uri);
            if (!string.IsNullOrEmpty(accept))
            {
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            }

            return configuredRequests.Match(requestMessage);
        }
    }

    public class WhenMatchingRoutes
    {
        [Fact]
        public void GivenPathWithQueryParameters_ReturnedRequestBuilderMatches()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo/bar?blah=blurb", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo/bar?blah=blurb",
                    null)
                .Should()
                .Be(requestBuilder);
        }

        [Fact]
        public void GivenNonConfigured_NoResultIsReturned()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo/bar?blah=blurb", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/baz", 
                    null)
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenRouteWithParameter_RequestBuilderIsReturned()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foos/{id}", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foos/1234", 
                    null)
                .Should()
                .Be(requestBuilder);
        }

        [Fact]
        public void GivenRouteWithParameterAndQueryString_RequestBuilderIsReturned()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foos/{id}/?blah=baz", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foos/1234/?blah=baz", 
                    null)
                .Should()
                .Be(requestBuilder);
        }

        [Fact]
        public void GivenRouteWithParameterAndQueryStringWithoutSeparator_RequestBuilderIsReturned()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foos/{id}?blah=baz", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foos/1234?blah=baz", 
                    null)
                .Should()
                .Be(requestBuilder);
        }

        [Fact]
        public void GivenRouteWithParameterAndQueryStringWithoutSeparatorX_RequestBuilderIsReturned()
        {
            var routes = new List<RequestBuilder>
            {
                new RequestBuilder(HttpMethod.Get, "/api/foos/{id}?blah=baz", null),
                new RequestBuilder(HttpMethod.Get, "/api/foos/{id}?blah=qux", null)
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foos/1234?blah=qux", 
                    null)
                .Should()
                .Be(routes[1]);
        }

        [Fact]
        public void GivenResponseWithAcceptHeaderAndNoAcceptHeaderInRequest_RequestDoesNotMatch()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo", null)
                .Accepting("foo/bar");

            var routes = new List<RequestBuilder>
            {
                (RequestBuilder)requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo", 
                    null)
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenResponseWithAcceptHeaderAndAcceptHeaderInRequestMatches_ResponseBuilderIsReturned()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo", null)
                .Accepting("foo/bar");

            var routes = new List<RequestBuilder>
            {
                (RequestBuilder)requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo",
                    "foo/bar")
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenResponseWithAcceptHeaderAndAcceptHeaderInRequestDoesNotMatch_RequestDoesNotMatch()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo", null)
                .Accepting("foo/bar");

            var routes = new List<RequestBuilder>
            {
                (RequestBuilder)requestBuilder
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo",
                    "derp/derp")
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenTwoResponsesWithDifferentAcceptHeaderAndAcceptHeaderInRequestMatchesSecond_ResponseBuilderIsReturned()
        {
            var requestBuilderOne = new RequestBuilder(HttpMethod.Get, "/api/foo", null)
                .Accepting("foo/bar");
            var requestBuilderTwo = new RequestBuilder(HttpMethod.Get, "/api/foo", null)
                .Accepting("baz/quux");

            var routes = new List<RequestBuilder>
            {
                (RequestBuilder)requestBuilderOne,
                (RequestBuilder)requestBuilderTwo
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo", 
                    "baz/quux")
                .Should()
                .BeOfType<RequestBuilder>()
                .Which
                .Accept
                .Should()
                .Be("baz/quux");
        }

        [Fact]
        public void GivenTwoResponsesWithDifferentAcceptHeaderAndAcceptHeaderInRequestMatchesSecondWithQueryParameters_ResponseBuilderIsReturned()
        {
            var requestBuilderOne = new RequestBuilder(HttpMethod.Get, "/api/foo?bar=baz", null)
                .Accepting("foo/bar");
            var requestBuilderTwo = new RequestBuilder(HttpMethod.Get, "/api/foo?bar=baz", null)
                .Accepting("baz/quux");

            var routes = new List<RequestBuilder>
            {
                (RequestBuilder)requestBuilderOne,
                (RequestBuilder)requestBuilderTwo
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo?bar=baz", 
                    "baz/quux")
                .Should()
                .BeOfType<RequestBuilder>()
                .Which
                .Accept
                .Should()
                .Be("baz/quux");
        }

        [Fact]
        public void GivenRouteHasExtraPartInPath_ShouldNotReturnAMatch()
        {
            var routes = new List<RequestBuilder>
            {
                new RequestBuilder(HttpMethod.Post, "/api/foos/1/bla-bla", "application/json"),
            };

            var dictionary = ConfiguredRequests.FromRequestBuilders(routes);

            dictionary.Match(HttpMethod.Post, "/api/v2/foos/1/bla-bla", "application/json")
                .Should()
                .BeNull();
        }
    }
}