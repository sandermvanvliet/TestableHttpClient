namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Defines an assertion on a query string parameter
    /// </summary>
    public class QueryStringAssertion
    {
        /// <summary>
        /// Creates a new QueryStringAssertion that matches the given value
        /// </summary>
        /// <param name="key">The name of the query parameter to match</param>
        /// <param name="value">The value to match</param>
        public QueryStringAssertion(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Creates a new QueryStringAssertion that matches any value
        /// </summary>
        /// <param name="key">The name of the query parameter to match</param>
        public QueryStringAssertion(string key)
        {
            Key = key;
            AnyValue = true;
        }

        /// <summary>
        /// The name of the query string parameter
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Flag indicating whether any value is considered valid.
        /// </summary>
        public bool AnyValue { get; }
        /// <summary>
        /// The value to match on
        /// </summary>
        /// <remarks>This value is ignored if <see cref="AnyValue"/> is set to true</remarks>
        public string? Value { get; }
    }
}