using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xam.Plugin.WebView.Abstractions.Models
{
    [JsonObject]
    public class ActionEvent
    {
        [JsonProperty("action", Required = Required.Always)]
        public string Action { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }
    }
}
