using System.Collections.Generic;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// A <see cref="IHttpClientFactory"/> that allows you to provide <see cref="HttpClient"/> instances that are backed by a <see cref="TestableMessageHandler"/>
    /// </summary>
    public class TestableHttpClientFactory : IHttpClientFactory
    {
        private readonly Dictionary<string, TestableMessageHandler> _clientConfigurations = new Dictionary<string, TestableMessageHandler>();

        /// <inheritdoc cref="IHttpClientFactory.CreateClient"/>
        public System.Net.Http.HttpClient CreateClient(string name)
        {
            if (!_clientConfigurations.ContainsKey(name))
            {
                // No client was configured but IHttpClientFactory.CreateClient
                // requires us to return a new HttpClient instance. So we will
                // return one but it's going to be backed by a default
                // TestableMessageHandler. 
                return new System.Net.Http.HttpClient(new TestableMessageHandler());
            }

            return new System.Net.Http.HttpClient(_clientConfigurations[name]);
        }

        /// <summary>
        /// Configure a client name to return a <see cref="HttpClient"/> with the given handler
        /// </summary>
        /// <param name="name">The name of the <see cref="HttpClient"/> as requested by <see cref="CreateClient"/></param>
        /// <param name="messageHandler">The <see cref="TestableMessageHandler"/> instance that should be used for the client name</param>
        public void ConfigureClient(string name, TestableMessageHandler messageHandler)
        {
            _clientConfigurations.Add(name, messageHandler);
        }

        /// <summary>
        /// Configure a client name to return a <see cref="HttpClient"/> with a <see cref="TestableMessageHandler"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="HttpClient"/> as requested by <see cref="CreateClient"/></param>
        /// <returns>The <see cref="TestableMessageHandler"/> instance that will be used for the client name</returns>
        public TestableMessageHandler ConfigureClient(string name)
        {
            var messageHandler = new TestableMessageHandler();
            _clientConfigurations.Add(name, messageHandler);
            return messageHandler;
        }
    }
}
