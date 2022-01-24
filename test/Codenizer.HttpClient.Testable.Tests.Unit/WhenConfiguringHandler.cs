using System;
using Xunit;
using FluentAssertions;

namespace Codenizer.HttpClient.Testable.Tests.Unit
{
    public class WhenConfiguringHandler
    {
        [Fact]
        public void GivenExceptionShouldBeThrown_ExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.ShouldThrow(new Exception("BANG"));

            Action action = () => client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();

            action
                .Should()
                .Throw<Exception>()
                .WithMessage("BANG");
        }
        [Fact]
        public void GivenExceptionShouldBeThrown_RequestIsCaptured()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.ShouldThrow(new Exception("BANG"));

            try
            {
                client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();
            }
            catch
            {
                // ignored
            }

            handler
                .Requests
                .Should()
                .HaveCount(1);
        }

        [Fact]
        public void GivenTwoResponsesForSamePath_MultipleResponsesExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.RespondTo().Get().ForUrl("/api/hello");
            handler.RespondTo().Get().ForUrl("/api/hello");

            Action action = () => client.GetAsync("https://tempuri.org/api/hello").GetAwaiter().GetResult();

            action.Should().Throw<MultipleResponsesConfiguredException>();
        }

        [Fact]
        public void GivenTwoResponsesForSamePathAndQueryString_MultipleResponsesExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.RespondTo().Get().ForUrl("/api/hello?foo=bar");
            handler.RespondTo().Get().ForUrl("/api/hello?foo=bar");

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action.Should().Throw<MultipleResponsesConfiguredException>();
        }

        [Fact]
        public void GivenTwoResponsesForSamePathButDifferentQueryString_NoMultipleResponsesExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.RespondTo().Get().ForUrl("/api/hello?foo=bar");
            handler.RespondTo().Get().ForUrl("/api/hello?foo=qux");

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action.Should().NotThrow<MultipleResponsesConfiguredException>();
        }

        [Fact]
        public void GivenTwoResponsesForSamePathAndQueryStringAndMethod_MultipleResponsesExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.RespondTo().Get().ForUrl("/api/hello?foo=bar");
            handler.RespondTo().Get().ForUrl("/api/hello?foo=bar");

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action.Should().Throw<MultipleResponsesConfiguredException>();
        }

        [Fact]
        public void GivenTwoResponsesForSamePathAndQueryStringButDifferentMethod_NoMultipleResponsesExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            var client = new System.Net.Http.HttpClient(handler);

            handler.RespondTo().Get().ForUrl("/api/hello?foo=bar");
            handler.RespondTo().Post().ForUrl("/api/hello?foo=bar");

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action.Should().NotThrow<MultipleResponsesConfiguredException>();
        }

        [Fact]
        public void GivenRequestBuilderWithMethodButNoUrl_ResponseConfigurationExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            handler.RespondTo().Get();
            var client = new System.Net.Http.HttpClient(handler);

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action.Should().Throw<ResponseConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("The URL to respond to has not been set");
        }

        [Fact]
        public void GivenRequestBuilderWithout_ResponseConfigurationExceptionIsThrown()
        {
            var handler = new TestableMessageHandler();
            handler.RespondTo().ForUrl("/derp");
            var client = new System.Net.Http.HttpClient(handler);

            Action action = () => client.GetAsync("https://tempuri.org/api/hello?foo=bar").GetAwaiter().GetResult();

            action.Should().Throw<ResponseConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("The HTTP verb to respond to has not been set");
        }
    }
}
