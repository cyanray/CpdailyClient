using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;
using System.Web;

namespace Cpdaily.CpdailyModels
{
    public class SchoolDetails
    {
        public enum SchoolJoinType
        {
            NOTCLOUD, CLOUD, CAS
        }

        /// <summary>
        /// SchoolId
        /// </summary>
        public string? Id { get; set; }
        public string? AppId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SchoolJoinType JoinType { get; set; }

        public string? IdsUrl { get; set; }

        public string? CasLoginUrl { get; set; }

        public string? AmpUrl { get; set; }

        public string? GetAmpUrlPrefix()
        {
            if (AmpUrl is null) return null;
            return new Uri(AmpUrl).GetLeftPart(UriPartial.Authority);
        }

        public string? GetIdsUrlPrefix()
        {
            if (IdsUrl is null) return null;
            return new Uri(IdsUrl).GetLeftPart(UriPartial.Authority);
        }

        public string GetIdsLoginUrl()
        {
            return $"{IdsUrl}/login?service={GetAmpUrlPrefix()}/wec-counselor-sign-apps/stu/mobile/index.html";
        }

        public string GetLoginUrl()
        {
            var serviceUrl = HttpUtility.UrlEncode("https://mobile.campushoy.com/v6/auth/campus/cas/login");
            return JoinType switch
            {
                SchoolJoinType.CAS => $"{CasLoginUrl}/login?service={serviceUrl}",
                SchoolJoinType.CLOUD => $"{IdsUrl}/login?service={serviceUrl}",
                SchoolJoinType.NOTCLOUD => $"{IdsUrl}/mobile/auth?appId={AppId}",
                _ => throw new NotImplementedException("尚未支持的 SchoolJoinType.")
            };
        }
    }
}
