using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class Next
    {
        [Fact]
        public void One()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "https://blog.codenizer.nl/api/v1/some/entity?query=param&query=blah&foo=bar",
                null);

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            configuredRequests
                .Count
                .Should()
                .Be(1);
        }

        [Fact]
        public void GivenGetRequestForFullyQualifiedUrl_MatchingExactRequestSucceeds()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "https://blog.codenizer.nl/api/v1/some/entity?query=param&query=blah&foo=bar",
                null);

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "https://blog.codenizer.nl/api/v1/some/entity?query=param&query=blah&foo=bar"));

            match
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenGetRequestForRelativeUrl_MatchingRelativeRequestSucceeds()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param&query=blah&foo=bar",
                null);

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity?query=param&query=blah&foo=bar"));

            match
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenGetRequestForRelativeUrl_MatchingPostRequestFails()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param&query=blah&foo=bar",
                null);

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Post,
                    "/api/v1/some/entity?query=param&query=blah&foo=bar")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });

            match
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenGetRequestForRelativeUrlWithQueryParams_MatchingRequestWithoutQueryParamsFails()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param&query=blah&foo=bar",
                null);

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity"));

            match
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenGetRequestForRelativeUrlWithQueryParams_MatchingRequestWithDifferentQueryParamValueFails()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param",
                null);

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity?query=bar"));

            match
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenGetRequestWithAcceptHeader_MatchingRequestWithoutAcceptHeaderFails()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param",
                null);
            
            builder.Accepting("text/plain");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity?query=param"));

            match
                .Should()
                .BeNull();
        }

        [Fact]
        public void Dump()
        {
            var handler = new TestableMessageHandler();

            handler
                .RespondTo()
                .Get()
                .ForUrl("/api/v1/some/entity?query=param")
                .Accepting("text/plain")
                .With(HttpStatusCode.Accepted);

            handler
                .RespondTo()
                .Get()
                .ForUrl("https://tempuri.org/api/v1/some/entity")
                .Accepting("text/plain")
                .With(HttpStatusCode.Created)
                .AndContent("application/json", new { id = 1});

            handler
                .RespondTo()
                .Get()
                .ForUrl("https://tempuri.org/api/v2/foo/bar?derp=foo")
                .Accepting("application/json")
                .ForQueryStringParameter("derp").WithAnyValue()
                .With(HttpStatusCode.NotModified)
                .AndHeaders(new Dictionary<string, string>
                {
                    { "Cache-Control", "maxage=3600"},
                    { "Pragma", "no-cache"}
                });
            
            handler
                .RespondTo()
                .Head()
                .ForUrl("http://tempuri.org/api/v1/some")
                .Accepting("text/plain")
                .With(HttpStatusCode.OK)
                .AndCookie("foo", "bar");

            Debug.WriteLine(handler.DumpConfiguredResponses());

#if NCRUNCH
            throw new Exception("BANG!");
#endif
        }
    }
}