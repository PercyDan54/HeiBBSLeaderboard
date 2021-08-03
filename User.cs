using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeiBBSLeaderboard
{
    public class User
    {
        [JsonProperty("credits")]
        public int Credit;

        [JsonProperty("uid")]
        public int UID;

        [JsonProperty("username")]
        public string Name;

        protected readonly Dictionary<string, int> LevelMap = new Dictionary<string, int>
        {
            { "幽灵", 0 },
            { "物质灵", 1 },
            { "生灵", 2 },
            { "精灵", 3 },
            { "妖灵", 4 },
            { "仙灵", 5 },
            { "神灵", 6 },
            { "催更灵", 7 },
        };

        public string Level
        {
            get
            {
                if (Credit >= 50000) return "催更灵";
                if (Credit >= 10000) return "神灵";
                if (Credit >= 5000) return "仙灵";
                if (Credit >= 1000) return "妖灵";
                if (Credit >= 500) return "精灵";
                if (Credit >= 100) return "生灵";
                if (Credit >= 0) return "物质灵";
                if (Credit < 0) return "幽灵";

                throw new ArgumentException("非法参数");
            }
        }

        public int LevelID
        {
            get
            {
                LevelMap.TryGetValue(Level, out var levelID);
                return levelID;
            }
        }

        public override string ToString() => $"{Name}（UID：{UID}）\t等级：{Level}\t积分：{Credit}";
    }
}
