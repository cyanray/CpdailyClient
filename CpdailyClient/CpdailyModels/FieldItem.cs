using Newtonsoft.Json;

namespace Cpdaily.CpdailyModels
{
    public class FieldItem
    {
        [JsonProperty("itemWid")]
        public string? ItemWid { get; set; }

        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("isOtherItems")]
        public bool? IsOtherItems { get; set; }

        [JsonProperty("contendExtend")]
        public string? ContendExtend { get; set; }

        [JsonProperty("isSelected")]
        public int? IsSelected { get; set; }
    }
}
