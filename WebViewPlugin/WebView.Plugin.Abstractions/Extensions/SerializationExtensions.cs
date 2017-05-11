using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Xam.Plugin.Abstractions.DTO;

namespace Xam.Plugin.Abstractions.Extensions
{
    public static class SerializationExtensions
    {

        public static bool ValidateJSON(this string s)
        {
            try
            {
                return s.StartsWith("{") && s.EndsWith("}");
                return true;
            }
            catch (JsonReaderException ex)
            {
                return false;
            }
        }

        public static ActionResponse AttemptParseActionResponse(this string json)
        {
            ActionResponse ar = JsonConvert.DeserializeObject<ActionResponse>(json);
            return ar.Action != null ? ar : null;
        }

    }
}
