using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public static class HttpRequestMessageExtensions
    {
        public static object GetData(this HttpRequestMessage request)
        {
            if(request.Content == null)
            {
                return null;
            }

            if(request.Content is StringContent)
            {
                var stringContent = request.Content as StringContent;

                var task = stringContent.ReadAsStringAsync();
                
                task.Wait();

                return task.Result;
            }
            if(request.Content is ByteArrayContent)
            {
                var byteContent = request.Content as ByteArrayContent;

                var task = byteContent.ReadAsByteArrayAsync();

                task.Wait();

                return task.Result;
            }

            return null;
        }
    }
}