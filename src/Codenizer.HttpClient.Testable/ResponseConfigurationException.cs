using System;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Thrown when the <see cref="RequestBuilder"/> has not been fully configured
    /// </summary>
    public class ResponseConfigurationException : Exception
    {
        internal ResponseConfigurationException(string message) : base(message)
        {
            
        }
    }
}