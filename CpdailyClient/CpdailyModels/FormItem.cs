using Newtonsoft.Json;
using System;

namespace Cpdaily.CpdailyModels
{
    public class FormItem
    {
        public string? WId { get; set; }
        public string? FormWId { get; set; }
        [JsonProperty("subject")]
        public string? Title { get; set; }
        public string? SenderUserName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
