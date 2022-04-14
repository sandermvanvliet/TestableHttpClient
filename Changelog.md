# Codenizer.HttpClient.Testable Changelog

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
