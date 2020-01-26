using Newtonsoft.Json;

namespace AudioChapterSplit.ffprobeOutput
{
    public class ChapterTags
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }

}
