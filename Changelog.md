# Codenizer.HttpClient.Testable Changelog

## 2.0.0

- Added XML Doc comments on public methods and properties to improve documentation in the IDE
- Added support for `AndJsonContent` that will serialize the provided object using either provided JSON serializer settings, the serializer settings configured on the testable handler or the default Json.Net serializer settings (in that order)
- Added support for returning a `byte[]` response using `AndContent("application/octet-stream", new byte[] { /* data */ })`
- Fix an issue where the testable handler did not support multiple query parameters with the same name (For example `/api/foo?bar=foo&bar=quux`), this is now supported and assertions for individual parameters too.

The query string parameter bug fixed in this release needed a breaking change to support that properly the version number is bumped from `1.4.0` to `2.0.0`.
