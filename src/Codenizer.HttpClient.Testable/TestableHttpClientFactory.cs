using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// A <see cref="IHttpClientFactory"/> that allows you to provide <see cref="HttpClient"/> instances that are backed by a <see cref="TestableMessageHandler"/>
    /// </summary>
    public class TestableHttpClientFactory : IHttpClientFactory
    {
        private readonly Dictionary<string, TestableMessageHandler> _namedHandlers = new();
        private readonly Dictionary<string, Action<System.Net.Http.HttpClient>> _clientConfigurations = new();

        /// <inheritdoc cref="IHttpClientFactory.CreateClient"/>
        public System.Net.Http.HttpClient CreateClient(string name)
        {
            System.Net.Http.HttpClient httpClient;

            if (!_namedHandlers.ContainsKey(name))
            {
                // No client was configured but IHttpClientFactory.CreateClient
                // requires us to return a new HttpClient instance. So we will
                // return one but it's going to be backed by a default
                // TestableMessageHandler. 
                httpClient = new System.Net.Http.HttpClient(new TestableMessageHandler());
            }
            else
            {
                httpClient = new System.Net.Http.HttpClient(_namedHandlers[name]);
            }

            if (_clientConfigurations.ContainsKey(name))
            {
                _clientConfigurations[name](httpClient);
            }

            return httpClient;
        }

        /// <summary>
        /// Configure a client name to return a <see cref="HttpClient"/> with the given handler
        /// </summary>
        /// <param name="name">The name of the <see cref="HttpClient"/> as requested by <see cref="CreateClient"/></param>
        /// <param name="messageHandler">The <see cref="TestableMessageHandler"/> instance that should be used for the client name</param>
        public void ConfigureClient(string name, TestableMessageHandler messageHandler)
        {
            _namedHandlers.Add(name, messageHandler);
        }

        /// <summary>
        /// Configure a client name to return a <see cref="HttpClient"/> with a <see cref="TestableMessageHandler"/> and configure the <see cref="HttpClient"/> instance with defaults
        /// </summary>
        /// <param name="name">The name of the <see cref="HttpClient"/> as requested by <see cref="CreateClient"/></param>
        /// <param name="configure">A lambda to configure the created <see cref="HttpClient"/></param>
        /// <returns>The <see cref="TestableMessageHandler"/> instance that will be used for the client name</returns>
        public TestableMessageHandler ConfigureClient(string name, Action<System.Net.Http.HttpClient> configure)
        {
            _clientConfigurations.Add(name, configure);
            var handler = ConfigureClient(name);

            return handler;
        }

        /// <summary>
        /// Configure a client name to return a <see cref="HttpClient"/> with a <see cref="TestableMessageHandler"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="HttpClient"/> as requested by <see cref="CreateClient"/></param>
        /// <returns>The <see cref="TestableMessageHandler"/> instance that will be used for the client name</returns>
        public TestableMessageHandler ConfigureClient(string name)
        {
            var messageHandler = new TestableMessageHandler();
            _namedHandlers.Add(name, messageHandler);
            return messageHandler;
        }
    }
}
