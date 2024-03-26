using System;
using System.Net.Http;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenConfiguringHandlerThroughHttpClientFactory
    {
        private readonly TestableHttpClientFactory _factory = new();
        private const string ClientName = "test client name";

        [Fact]
        public void GivenClientNameWasNotConfigured_NewHandlerIsRegistered()
        {
            var client = _factory.CreateClient(ClientName);

            client
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void GivenClientNameWasConfigured_ClientUsesConfiguredHandler()
        {
            var handler = _factory.ConfigureClient(ClientName);

            var client = _factory.CreateClient(ClientName);

            TheHandlerOf(client)
                .Should()
                .Be(handler);
        }

        [Fact]
        public void GivenClientWasConfiguredWithSpecificHandler_ClientIsUsingSpecifiedHandler()
        {
            var handler = new TestableMessageHandler();
            _factory.ConfigureClient(ClientName, handler);

            var client = _factory.CreateClient(ClientName);

            TheHandlerOf(client)
                .Should()
                .Be(handler);
        }

        [Fact]
        public void GivenClientNameWasConfiguredWithClientConfiguration_ConfigurationIsAppliedToClient()
        {
            _factory.ConfigureClient(ClientName, client => client.BaseAddress = new Uri("https://example.com"));

            var client = _factory.CreateClient(ClientName);

            client
                .BaseAddress
                .Should()
                .Be(new Uri("https://example.com"));
        }

        private HttpMessageHandler? TheHandlerOf(System.Net.Http.HttpClient client)
        {
            var field = typeof(HttpMessageInvoker).GetField("_handler", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

            return field?.GetValue(client) as HttpMessageHandler;
        }
    }
}
