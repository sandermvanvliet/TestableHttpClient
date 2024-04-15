using System.Net.Http;

namespace Codenizer.HttpClient.Testable;

internal class RequestContentNode : RequestNode
{
    private readonly string? _expectedContent;

    public RequestContentNode(string? expectedContent)
    {
        _expectedContent = expectedContent;
    }
    
    public override void Accept(RequestNodeVisitor visitor)
    {
        if (_expectedContent != null)
        {
            visitor.Content(_expectedContent);
        }
        
        if (RequestBuilder != null)
        {
            visitor.Response(RequestBuilder);
        }
    }

    public RequestBuilder? RequestBuilder { get; private set; }

    public void SetRequestBuilder(RequestBuilder requestBuilder)
    {
        if (RequestBuilder != null)
        {
            throw new MultipleResponsesConfiguredException(2, requestBuilder.PathAndQuery!);
        }

        RequestBuilder = requestBuilder;
    }

    public bool Match(HttpContent content)
    {
        if (_expectedContent == null)
        {
            return true;
        }
        
        if (content is StringContent stringContent)
        {
            var requestContent = stringContent.ReadAsStringAsync().GetAwaiter().GetResult();

            return string.Equals(_expectedContent, requestContent);
        }
        
        return false;
    }

    public bool Match(string? expectedContent)
    {
        return string.Equals(_expectedContent, expectedContent);
    }
}