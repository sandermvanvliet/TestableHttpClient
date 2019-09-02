using System.Collections.Generic;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
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

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .Match(
                    HttpMethod.Get,
                    "/api/foo/bar?blah=blurb")
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

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .Match(
                    HttpMethod.Get, 
                    "/api/baz")
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

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .Match(
                    HttpMethod.Get, 
                    "/api/foos/1234")
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

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .Match(
                    HttpMethod.Get, 
                    "/api/foos/1234/?blah=baz")
                .Should()
                .Be(requestBuilder);
        }
    }
}