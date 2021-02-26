using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GetScore_GUI
{
    internal class LeaderBoardResponse
    {
        [JsonExtensionData]
        public Dictionary<string, JToken> Users;

        [JsonProperty("ok")]
        public int OK;
    }
}
