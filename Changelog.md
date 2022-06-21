# Codenizer.HttpClient.Testable Changelog

## 2.3.0

This release reworks the way that the configured requests are handled internally. Originally it was a very simple approach that proved to be very difficult to extend with new features over time.
That lead to a lot of kludges in the code to make for example cookie handling work and as a side-effect it made the code hard to understand.

The new approach uses a tree like structure to build the map of configured requests/responses which makes it easier to plug in new behaviour.

Additionally you can now get an overview of the configured requests for use when troubleshooting by calling `DumpConfiguredResponses()` on a `TestableMessageHandler` instance:

```csharp
handler
	.RespondTo()
	.Get()
	.ForUrl("/foo/bar")
	.Accepting("text/xml")
	.With(HttpStatusCode.OK)
	.AndContent("text/xml", "<foo>blah</foo>");

var output = handler.DumpConfiguredResponses();

Debug.WriteLine(output);
```

will show you:

```text
GET
    *://
        *
            /foo/bar
                Accept: text/xml
                Response:
                    HTTP 200 OK with text/xml payload
```

The `*` denotes a wildcard. Here we're using a relative URI which means that it will be matched on any scheme (`http://`, `https://`, `gopher://`) and any authority (host).

When specifying an absolute URI the output includes more details:

```csharp
handler
	.RespondTo()
	.Get()
	.ForUrl("/foo/bar")
	.Accepting("text/xml")
	.With(HttpStatusCode.OK)
	.AndContent("text/xml", "<foo>blah</foo>");

handler
	.RespondTo()
	.Post()
	.ForUrl("https://tempuri.org:5200/foo/bar")
	.Accepting("text/xml")
	.With(HttpStatusCode.Created)
	.AndContent("text/xml", "<foo>blah</foo>");

var output = handler.DumpConfiguredResponses();

Debug.WriteLine(output);
```

```text
GET
    *://
        *
            /foo/bar
                Accept: text/xml
                Response:
                    HTTP 200 OK with text/xml payload
POST
    https://
        tempuri.org:5200
            /foo/bar
                Accept: text/xml
                Response:
                    HTTP 201 Created with text/xml payload
```

Here you can see that the scheme and authority are included. Matching will be exactly on those parameters.

**Special note:**

The behaviour where the handler would return a `415 Unsupported Media Type` when you would PUT/POST to a request with the wrong `Content-Type` header set will be removed in version 2.4.0.
The rationale here is that when your code depends on this particular behaviour then you should configure the requests accordingly. It was added as a convenience but it turns out that it may lead to requests matching incorrectly and that's not what you want from a library such as this.

## 2.2.1

This release fixes an issue where paths would get a match if when they shouldn't.

```http
GET /foo/bar
```

would also match
```http
GET /foo/v1/bar
```

## 2.2.0

This release changes the target frameworks for the test project to netcore 3.1, net5 and net6.

It also adds a feature to match requests based on the `Accept` header:

```csharp
handler
	.RespondTo()
	.Get()
	.ForUrl("/foo/bar")
	.Accepting("text/xml")
	.With(HttpStatusCode.OK)
	.AndContent("text/xml", "<foo>blah</foo>");
```

That will match a request like this:

```http
GET /foo/bar
Accept: text/xml
```

but won't match:

```http
GET /foo/bar
Accept: application/json
```

or

```http
GET /foo/bar
```

## 2.1.0

This release introduces a more fluent way to configure the responses. Instead of calling the handler like so:

```csharp
handler
 .RespondTo(HttpMethod.Post, "/api/some/endpoint")
 .With(HttpStatusCode.Created);
```

you can now write this as:

```csharp
handler
 .RespondTo()
 .Post()
 .ForUrl("/api/some/endpoint")
 .With(HttpStatusCode.Created);
```

The value of the `Content-Type` header can be set as well using `AndContentType`:

```csharp
handler
 .RespondTo()
 .Post()
 .ForUrl("/api/some/endpoint")
 .AndContentType("application/json")
 .With(HttpStatusCode.Created);
```

The `RespondTo(HttpMethod method, string url, string contentType)` method is marked as obsolete but as a warning. It will be removed in version 3.x in the future.

## 2.0.0

- Added XML Doc comments on public methods and properties to improve documentation in the IDE
- Added support for `AndJsonContent` that will serialize the provided object using either provided JSON serializer settings, the serializer settings configured on the testable handler or the default Json.Net serializer settings (in that order)
- Added support for returning a `byte[]` response using `AndContent("application/octet-stream", new byte[] { /* data */ })`
- Fix an issue where the testable handler did not support multiple query parameters with the same name (For example `/api/foo?bar=foo&bar=quux`), this is now supported and assertions for individual parameters too.

The query string parameter bug fixed in this release needed a breaking change to support that properly the version number is bumped from `1.4.0` to `2.0.0`.
