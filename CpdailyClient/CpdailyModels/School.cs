using Newtonsoft.Json;

namespace Cpdaily.CpdailyModels
{
    public class School
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        [JsonProperty("img")]
        public string? ImageUrl { get; set; }
    }
}
