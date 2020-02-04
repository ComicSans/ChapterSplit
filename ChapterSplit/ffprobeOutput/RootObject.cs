using Newtonsoft.Json;

namespace ChapterSplit.ffprobeOutput
{
    public class RootObject
    {
        [JsonProperty(PropertyName = "chapters")]
        public Chapter[] Chapters { get; set; }

        [JsonProperty(PropertyName = "format")]
        public Format Format { get; set; }
    }
}