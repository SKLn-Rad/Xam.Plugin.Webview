using Newtonsoft.Json;

namespace Xam.Plugin.Abstractions.DTO
{
    public class ActionResponse
    {

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
