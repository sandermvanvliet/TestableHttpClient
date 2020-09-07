using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    internal class RouteDictionary
    {
        internal Dictionary<string, RouteSegment> RootSegments = new Dictionary<string, RouteSegment>();

        internal static RouteDictionary From(List<RequestBuilder> routes)
        {
            var routeDictionary = new RouteDictionary();

            foreach (var route in routes)
            {
                RouteSegment pointer;

                var routeParts = PathAndQueryToSegments(route.PathAndQuery);

                if (!routeParts.Any())
                {
                    pointer = new RouteSegment("/");
                }
                else if (routeDictionary.RootSegments.ContainsKey(routeParts[0]))
                {
                    pointer = routeDictionary.RootSegments[routeParts[0]];
                }
                else
                {
                    pointer = new RouteSegment(routeParts[0]);
                    routeDictionary.RootSegments.Add(pointer.Part, pointer);
                }

                for (var index = 1; index < routeParts.Length; index++)
                {
                    var part = routeParts[index];

                    if (pointer.Segments.ContainsKey(part))
                    {
                        if (index == routeParts.Length - 1)
                        {
                            pointer.Segments[part].Add(route);

                            break;
                        }

                        pointer = pointer.Segments[part];
                        continue;
                    }

                    var segment = new RouteSegment(part);
                    pointer.Segments.Add(part, segment);
                    pointer = segment;

                    if (index == routeParts.Length - 1)
                    {
                        pointer.Add(route);
                    }
                }

                if (routeParts.Length == 1)
                {
                    routeDictionary.RootSegments[pointer.Part].Add(route);
                }
            }

            return routeDictionary;
        }

        internal RequestBuilder Match(HttpMethod method, string pathAndQuery)
        {
            var segments = PathAndQueryToSegments(pathAndQuery);
            var queryParameters = new Dictionary<string, string>();

            if (segments.Last().Contains("?"))
            {
                var parts = segments.Last().Split('?');

                segments[segments.Length - 1] = parts[0];

                queryParameters = parts[1]
                    .Split('&')
                    .Select(p => p.Split('='))
                    .ToDictionary(p => p[0], p => p[1]);
            }

            if (segments.Last() == "")
            {
                segments = segments.Take(segments.Length - 1).ToArray();
            }

            RouteSegment pointer = null;

            foreach (var segment in segments)
                if (pointer == null)
                {
                    if (!RootSegments.ContainsKey(segments[0])) return null;

                    pointer = RootSegments[segment];
                }
                else
                {
                    if (!pointer.Segments.ContainsKey(segment))
                    {
                        if (pointer.Segments.Any())
                        {
                            var first = pointer.Segments.Keys.First();

                            if (IsParameter(first))
                            {
                                pointer = pointer.Segments[first];
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        pointer = pointer.Segments[segment];
                    }
                }

            if (pointer != null)
            {
                if (pointer.HasForQueryParameters(method))
                {
                    // Match on query parameters
                    return pointer.GetForQueryParameters(method, queryParameters);
                }

                return pointer.GetWithoutQueryParameters(method);
            }

            return null;
        }

        private static bool IsParameter(string segment)
        {
            return segment.StartsWith("{") && segment.EndsWith("}");
        }

        private static string[] PathAndQueryToSegments(string pathAndQuery)
        {
            var parts = pathAndQuery.Split(new[] {'?'}, StringSplitOptions.RemoveEmptyEntries);

            var pathAndQueryToSegments = parts[0].Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (parts.Length == 2)
            {
                pathAndQueryToSegments[pathAndQueryToSegments.Count - 1] += "?" + parts[1];
            }

            return pathAndQueryToSegments.ToArray();
        }
    }
}