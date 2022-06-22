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
            return HavingAnyValue();
        }

        public IRequestBuilder WithValue(string value)
        {
            return HavingValue(value);
        }

        public IRequestBuilder HavingAnyValue()
        {
            _requestBuilder.QueryStringAssertions.Add(new QueryStringAssertion(key: _key));

            return _requestBuilder;
            
        }

        public IRequestBuilder HavingValue(string value)
        {
            _requestBuilder.QueryStringAssertions.Add(new QueryStringAssertion(key: _key, value: value));

            return _requestBuilder;
        }
    }
}