using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenBuildingRouteDictionary
    {
        [Fact]
        public void GivenPath_RootSegmentsContainFirstPartOfPath()
        {
            var routes = new List<RequestBuilder>
            {
                new RequestBuilder(HttpMethod.Get, "/api/foo/bar", null)
            };

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .RootSegments
                .Should()
                .ContainKey("api");
        }

        [Fact]
        public void GivenPath_TailSegmentContainsRequestBuilder()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo/bar", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .RootSegments["api"]
                .Segments["foo"]
                .Segments["bar"]
                .RequestBuilders[HttpMethod.Get]
                .Should()
                .Be(requestBuilder);
        }

        [Fact]
        public void GivenPathWithQueryParameters_TailSegmentContainsRequestBuilder()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Get, "/api/foo/bar?blah=blurb", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .RootSegments["api"]
                .Segments["foo"]
                .Segments["bar?blah=blurb"]
                .RequestBuilders[HttpMethod.Get]
                .Should()
                .Be(requestBuilder);
        }

        [Fact]
        public void GivenPathForSingleDocumentAtRoot_RootSegmentContainsRequestBuilder()
        {
            var requestBuilder = new RequestBuilder(HttpMethod.Post, "/index.php", null);

            var routes = new List<RequestBuilder>
            {
                requestBuilder
            };

            var dictionary = RouteDictionary.From(routes);

            dictionary
                .RootSegments["index.php"]
                .RequestBuilders[HttpMethod.Post]
                .Should()
                .Be(requestBuilder);
        }
    }
}