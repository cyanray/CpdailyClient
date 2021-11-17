using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Cpdaily.CpdailyModels
{
    internal class ApiResponse<T>
    {
        [JsonProperty("errCode")]
        public int ErrorCode { get; set; }

        [JsonProperty("errMsg")]
        public string? ErrorMessage { get; set; }

        [AllowNull]
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
