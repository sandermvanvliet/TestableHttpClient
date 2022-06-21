namespace Codenizer.HttpClient.Testable
{
    internal abstract class RequestNode
    {
        public abstract void Accept(RequestNodeVisitor visitor);
    }
}