using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Extension methods to improve usage patterns of the testable message handler
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Get the data if any from the captured request
        /// </summary>
        /// <param name="request">The captured request</param>
        /// <returns>Either the <c>string</c>, <c>byte</c> array or <c>null</c> that is the content of the request</returns>
        public static object GetData(this HttpRequestMessage request)
        {
            switch (request.Content)
            {
                case null:
                    return null;
                case StringContent stringContent:
                    return stringContent
                        .ReadAsStringAsync()
                        .GetAwaiter()
                        .GetResult();
                case ByteArrayContent byteContent:
                    return byteContent
                        .ReadAsByteArrayAsync()
                        .GetAwaiter()
                        .GetResult();
                default:
                    return null;
            }
        }
    }
}