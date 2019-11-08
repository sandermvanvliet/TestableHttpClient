using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public static class HttpRequestMessageExtensions
    {
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