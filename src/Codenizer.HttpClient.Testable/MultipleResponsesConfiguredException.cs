using System;
using System.Runtime.Serialization;

namespace Codenizer.HttpClient.Testable
{
    [Serializable]
    public class MultipleResponsesConfiguredException : Exception
    {
        private readonly int _numberOfResponses;
        private readonly string _pathAndQuery;

        public MultipleResponsesConfiguredException(int numberOfResponses, string pathAndQuery) 
            : base("Multiple responses configured for the same path and query string")
        {
            _numberOfResponses = numberOfResponses;
            _pathAndQuery = pathAndQuery;

            Data.Add("numberOfResponses", numberOfResponses);
            Data.Add("pathAndQuery", pathAndQuery);
        }

        protected MultipleResponsesConfiguredException(SerializationInfo info, StreamingContext context)
             : base(info, context)
        {
            _numberOfResponses = info.GetInt32("numberOfResponses");
            _pathAndQuery = info.GetString("pathAndQuery");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("numberOfResponses", _numberOfResponses);
            info.AddValue("pathAndQuery", _pathAndQuery);

            base.GetObjectData(info, context);
        }
    }
}