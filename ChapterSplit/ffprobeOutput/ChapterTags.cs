using Newtonsoft.Json;

namespace ChapterSplit.ffprobeOutput
{
    public class ChapterTags
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }
}