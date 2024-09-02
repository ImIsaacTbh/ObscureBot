
namespace Obscura.External
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class DefineData
    {
        [JsonProperty("list")]
        public List[] List { get; set; }
    }

    public partial class List
    {
        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("permalink")]
        public Uri Permalink { get; set; }

        [JsonProperty("thumbs_up")]
        public long ThumbsUp { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("defid")]
        public long Defid { get; set; }

        [JsonProperty("current_vote")]
        public string CurrentVote { get; set; }

        [JsonProperty("written_on")]
        public DateTimeOffset WrittenOn { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }

        [JsonProperty("thumbs_down")]
        public long ThumbsDown { get; set; }
    }
}
