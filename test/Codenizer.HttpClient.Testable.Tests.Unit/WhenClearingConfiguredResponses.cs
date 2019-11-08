using System.Net;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenClearingConfiguredResponses
    {
        [Fact]
        public void GivenHandlerHasTwoResponsesConfigured_ResponsesCollectionIsEmpty()
        {
            var handler = new TestableMessageHandler();

            handler.RespondTo(HttpMethod.Get, "/one").With(HttpStatusCode.OK);
            handler.RespondTo(HttpMethod.Get, "/two").With(HttpStatusCode.OK);

            handler.ClearConfiguredResponses();

            handler
                .ConfiguredResponses
                .Should()
                .BeEmpty();
        }
    }
}