using System.Net;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenHandlingRequestWithSequence
    {
        [Fact]
        public void GivenSequenceWithThreeResponses_AllThreeResponsesAreReturned()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello")
                .WithSequence(builder => builder
                    .With(HttpStatusCode.OK)
                    .AndContent("text/plain", "response 1"))
                .WithSequence(builder => builder
                    .With(HttpStatusCode.OK)
                    .AndContent("text/plain", "response 2"))
                .WithSequence(builder => builder
                    .With(HttpStatusCode.OK)
                    .AndContent("text/plain", "response 3"));
                

            var response1 = client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();
            var response2 = client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();
            var response3 = client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();

            ContentOf(response1).Should().Be("response 1");
            ContentOf(response2).Should().Be("response 2");
            ContentOf(response3).Should().Be("response 3");
        }

        [Fact]
        public void GivenSequenceWithThreeResponsesAndFourRequests_LastRequestResultsInInternalServerError()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler
                .RespondTo("/api/hello")
                .WithSequence(builder => builder
                    .With(HttpStatusCode.OK)
                    .AndContent("text/plain", "response 1"))
                .WithSequence(builder => builder
                    .With(HttpStatusCode.OK)
                    .AndContent("text/plain", "response 2"))
                .WithSequence(builder => builder
                    .With(HttpStatusCode.OK)
                    .AndContent("text/plain", "response 3"));
                
            client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();
            client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();
            client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();
            var lastResponse = client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();

            lastResponse
                .StatusCode
                .Should()
                .Be(HttpStatusCode.InternalServerError);

            ContentOf(lastResponse)
                .Should()
                .Be("Received request number 4 for /api/hello but only 3 responses were configured");
        }

        private string ContentOf(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            return null;
        }
    }
}