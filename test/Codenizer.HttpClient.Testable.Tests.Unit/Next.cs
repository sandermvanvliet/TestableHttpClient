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
                "application/json");

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
                "application/json");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "https://blog.codenizer.nl/api/v1/some/entity?query=param&query=blah&foo=bar")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });

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
                "application/json");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity?query=param&query=blah&foo=bar")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });

            match
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenGetRequestForRelativeUrl_MatchingAbsoluteRequestFails()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param&query=blah&foo=bar",
                "application/json");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "https://blog.codenizer.nl/api/v1/some/entity?query=param&query=blah&foo=bar")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });

            match
                .Should()
                .BeNull();
        }

        [Fact]
        public void GivenGetRequestForRelativeUrl_MatchingPostRequestFails()
        {
            var builder = new RequestBuilder(
                HttpMethod.Get,
                "/api/v1/some/entity?query=param&query=blah&foo=bar",
                "application/json");

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
                "application/json");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });

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
                "application/json");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity?query=bar")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });

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
                "application/json");
            
            builder.Accepting("text/plain");

            var configuredRequests = ConfiguredRequests.FromRequestBuilders(new[] { builder });

            var match = configuredRequests
                .Match(new HttpRequestMessage(HttpMethod.Get,
                    "/api/v1/some/entity?query=param"));

            match
                .Should()
                .BeNull();
        }
    }
}