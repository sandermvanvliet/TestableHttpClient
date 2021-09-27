namespace Codenizer.HttpClient.Testable
{
    /// <summary>
    /// Defines an assertion on a query string parameter
    /// </summary>
    public class QueryStringAssertion
    {
        /// <summary>
        /// The name of the query string parameter
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Flag indicating whether any value is considered valid.
        /// </summary>
        public bool AnyValue { get; set; }
        /// <summary>
        /// The value to match on
        /// </summary>
        /// <remarks>This value is ignored if <see cref="AnyValue"/> is set to true</remarks>
        public string Value { get; set; }
    }
}