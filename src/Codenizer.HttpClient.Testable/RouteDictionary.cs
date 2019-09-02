using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Codenizer.HttpClient.Testable
{
    public class RouteDictionary
    {
        public Dictionary<string, RouteSegment> RootSegments = new Dictionary<string, RouteSegment>();

        public static RouteDictionary From(List<RequestBuilder> routes)
        {
            var routeDictionary = new RouteDictionary();

            foreach (var route in routes)
            {
                RouteSegment pointer;

                var routeParts = PathAndQueryToSegments(route.PathAndQuery);

                if (routeDictionary.RootSegments.ContainsKey(routeParts[0]))
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
                            if (pointer.Segments[part].RequestBuilders.ContainsKey(route.Method))
                                throw new MultipleResponsesConfiguredException(2, route.PathAndQuery);

                            pointer.Segments[part].RequestBuilders.Add(route.Method, route);
                            break;
                        }

                        pointer = pointer.Segments[part];
                        continue;
                    }

                    var segment = new RouteSegment(part);
                    pointer.Segments.Add(part, segment);
                    pointer = segment;

                    if (index == routeParts.Length - 1) pointer.RequestBuilders.Add(route.Method, route);
                }
            }

            return routeDictionary;
        }

        public RequestBuilder Match(HttpMethod method, string pathAndQuery)
        {
            var segments = PathAndQueryToSegments(pathAndQuery);

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

                            if (IsParameter(first)) pointer = pointer.Segments[first];
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

            if (pointer != null && pointer.RequestBuilders.ContainsKey(method)) return pointer.RequestBuilders[method];

            return null;
        }

        private static bool IsParameter(string segment)
        {
            return segment.StartsWith("{") && segment.EndsWith("}");
        }

        private static string[] PathAndQueryToSegments(string pathAndQuery)
        {
            return pathAndQuery.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}