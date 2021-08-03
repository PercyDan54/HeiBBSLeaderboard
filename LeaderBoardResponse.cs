using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeiBBSLeaderboard
{
    internal class LeaderBoardResponse
    {
        [JsonExtensionData]
        public Dictionary<string, JToken> Users;

        [JsonProperty("ok")]
        public int Ok;
    }
}
