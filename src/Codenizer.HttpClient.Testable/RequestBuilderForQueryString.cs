namespace Codenizer.HttpClient.Testable
{
    internal class RequestBuilderForQueryString : IRequestBuilderForQueryString
    {
        private readonly RequestBuilder _requestBuilder;
        private readonly string _key;

        public RequestBuilderForQueryString(RequestBuilder requestBuilder, string key)
        {
            _requestBuilder = requestBuilder;
            _key = key;
        }

        public IRequestBuilder WithAnyValue()
        {
            _requestBuilder.QueryStringAssertions.Add(new QueryStringAssertion(key: _key));

            return _requestBuilder;
        }

        public IRequestBuilder WithValue(string value)
        {
            _requestBuilder.QueryStringAssertions.Add(new QueryStringAssertion(key: _key, value: value));

            return _requestBuilder;
        }
    }
}