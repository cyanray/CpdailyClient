using Newtonsoft.Json;

namespace Cpdaily.CpdailyModels
{
    public class LoginResult
    {
        public string? AuthId { get; set; }

        public string? DeviceStatus { get; set; }

        [JsonProperty("deviceExceptionMsg")]
        public string? DeviceExceptionMessage { get; set; }

        public string? Name { get; set; }

        public string? OpenId { get; set; }

        public string? PersonId { get; set; }

        public string? SessionToken { get; set; }

        public string? TenantId { get; set; }

        public string? Tgc { get; set; }

        public string? UserId { get; set; }
    }
}
