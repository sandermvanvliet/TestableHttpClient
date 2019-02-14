# Codenizer.HttpClient.Testable

A package to help you test the usage of HttpClient in your applications.

Most people would call this a stub, a double or a mock and they are probably right. This was written to be an easy way to have your code
interact with a `HttpClient` that just works instead of having to create wrappers around it.

It supports setting up various behaviours to responses it receives (actual: that are sent by the `HttpClient`) and allows you to assert that
calls are made and to verify the properties of those requests.

## Usage

To include this package in your applications, add the NuGet package:

**dotnet CLI:**

```bash
dotnet add package Codenizer.HttpClient.Testable
```

**PowerShell:**

```PowerShell
Install-Package -Name Codenizer.HttpClient.Testable
```

## Example use in a test

Let's say we have a class that calls an external API using HttpClient. It's a simple one:

```csharp
public class InfoApiClient
{
    private readonly HttpClient _httpClient;
    
    public InfoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> GetLatestInfo()
    {
        var response = await _httpClient.GetAsync("/api/info/latest");
        
        return await response.Content.ReadAsStringAsync();
    }
}
```

Unfortunately it doesn't deal with errors very well, in fact, it doesn't handle any errors at all!
Let's change that and return a string `Sorry, no content` if we get a `404 Not Found`. 

```csharp
public class WhenRequestingLatestInfo
{
    [Fact]
    public void GivenApiReturns404_SorryNoContentIsReturned()
    {
        var infoClient = new InfoApiClient(new HttpClient());
        
        infoClient
            .GetLatestInfo()
            .Result
            .Should()
            .Be("Sorry, no content");
    }
}
```

Unless we're suddenly very unlucky, this test is going to fail with: 

> One or more errors occurred. (An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.)

because we didn't configure any URL to talk to. So how can we make sure a `404 Not Found` is seen by the `HttpClient`
and for us to start changing the implementation to handle that?

We will need to intercept the calls made by the `HttpClient`:

```csharp
var messageHandler = new Codenizer.HttpClient.Testable.TestableMessageHandler();
var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost:5000") };
var infoClient = new InfoApiClient(httpClient);
```

Let's run the test again:

> Expected string to be  
   "Sorry, no content" with a length of 17, but  
   "No response configured for /api/info/latest" has a length of 43.

Right. At least it tells us what's going on. We need to set up something to make this work.
So let's instruct the handler to return a `404 Not Found` when we try to hit `/api/info/latest`:

```csharp
messageHandler
    .RespondTo(HttpMethod.Get, "/api/info/latest")
    .With(HttpStatusCode.NotFound);
```

Our test now looks like:

```csharp
[Fact]
public void GivenApiReturns404_SorryNoContentIsReturned_Step3()
{
    var messageHandler = new Codenizer.HttpClient.Testable.TestableMessageHandler();
    var httpClient = new System.Net.Http.HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost:5000") };
    var infoClient = new InfoApiClient(httpClient);
    
    messageHandler
        .RespondTo(HttpMethod.Get, "/api/info/latest")
        .With(HttpStatusCode.NotFound);

    infoClient
        .GetLatestInfo()
        .Result
        .Should()
        .Be("Sorry, no content");
}
```

Should work right....? Wrong:

> Object reference not set to an instance of an object

This exception actually occurs in the `InfoApiClient` because it tries to read content that isn't there. Our final step is to
implement a status code check in the `InfoApiClient` and make it work:

```csharp
public async Task<string> GetLatestInfo()
{
    var response = await _httpClient.GetAsync("/api/info/latest");

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
        return "Sorry, no content";
    }
    
    return await response.Content.ReadAsStringAsync();
}
```

Run the test again:

> Success

Yay!

## Taking it further

The message handler has a number of additional methods to control the responses it will generate. They follow a fluent 
style so are meant to be used in a chained fashion.

### Configuring the URLs it will respond to

```csharp
RespondTo(HttpMethod method, string pathAndQuery)
```

This method allows you to specify the relative part of the URI and the applicable HTTP method. The handler will match
against the entire path and query string including the HTTP method. If multiple responses are configured against the same URL
a `MultipleResponsesConfigureException` will be thrown.

### Setting the response code

```csharp
With(HttpStatusCode httpStatusCode)
```

### Setting content to return

```csharp
AndContent(string mimeType, string data)
```

You will need to specify the media type of the content to return. Data is supplied as a string, if you need to return JSON serialized data
you can use this method as:

```csharp
var serializedJson = JsonConvert.SerializeObject(myObject);

handler
    .RespondTo(HttpMethod.Get, "/api/info/latest")
    .With(HttpStatus.OK)
    .AndContent("application/json", serializedJson);
```

The handler will not serialize the data itself because it does not know about the required serialization settings.

### Response headers

```csharp
AndHeaders(Dictionary<string, string> headers)
```

For example:

```csharp
handler
    .RespondTo(HttpMethod.Get, "/api/info/latest")
    .With(HttpStatus.OK)
    .AndContent("application/json", serializedJson)
    .AndHeaders(new Dictionary<string, string>
                {
                    {"Test-Header", "SomeValue"}
                });
```

If you need specific HTTP headers on the response you can add them using this method.
This method can be called multiple times but be aware that it will append values to existing header names.

### Delaying responses

Let's say you want to test timeouts in your code, it would be useful to have the handler simulate that a request takes some time to process.
With `Taking(TimeSpan time)` you can do that:

```csharp
handler
    .RespondTo(HttpMethod.Get, "/api/info/latest")
    .With(HttpStatus.OK)
    .AndContent("application/json", serializedJson)
    .Taking(TimeSpan.FromMilliseconds(100));
```

This will make the handler delay for 100ms before returning the response you've configured. You do not necissarily need to define content first, `Taking()` can be applied to `With()` directly.

### Handle requests only if they match a content type

You may want to configure a response only if the request has a specific `Content-Type`, for example on `PUT` or `POST` endpoints that require `application/json`.
As of version `0.4.0` you can now do that like so:

```csharp
handler
    .RespondTo(HttpMethod.Post, "/api/infos/", "application/json")
    .With(HttpStatus.Created);
```

If you make a request with a content type of `text/plain`:

```csharp
await httpClient.PostAsync("/api/infos", new StringContent("test", Encoding.ASCII, "text/plain"));
```

the response returned from the handler will be `415 Unsupported Media Type`

### Verifying requests have been made

The handler exposes a `Requests` property that contains all requests made to the handler. You can use [FluentAssertions](https://github.com/fluentassertions/fluentassertions/) to assert the properties of the requests.

For example:

```csharp
_handler
    .Requests
    .Should()
    .Contain(req => req.RequestUri.PathAndQuery == "/api/info");
```

### Callback when request is made

For some cases it may be useful to have a callback when the request is handled by the testable handler. You can use `WhenCalled` to register one:

```csharp
var wasCalled = false;

handler
    .RespondTo(HttpMethod.Get, "/api/info/latest")
    .With(HttpStatus.OK)
    .AndContent("application/json", serializedJson)
    .WhenCalled(request => wasCalled = true);
```

When you make a request to `/api/info/latest` the `wasCalled` variable will now be set to `true`.