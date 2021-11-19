using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.CpdailyModels
{
    /// <summary>
    /// 今日校园用户信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 学校ID
        /// </summary>
        [JsonProperty("tenantId")]
        public string? TenantId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 性别：MALE / FEMALE
        /// </summary>
        [JsonProperty("gender")]
        public string? Gender { get; set; }

        /// <summary>
        /// 用户角色：STUDENT
        /// </summary>
        [JsonProperty("userRole")]
        public string? UserRole { get; set; }

        /// <summary>
        /// 学号
        /// </summary>
        [JsonProperty("studentNo")]
        public string? SchoolId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [JsonProperty("mobilePhone")]
        public string? MobilePhone { get; set; }

        /// <summary>
        /// 状态：ENABLE
        /// </summary>
        [JsonProperty("status")]
        public string? Status { get; set; }

        /// <summary>
        /// 今日校园ID
        /// </summary>
        [JsonProperty("campusAccount")]
        public string? CampusAccount { get; set; }

        /// <summary>
        /// 学校名称
        /// </summary>
        [JsonProperty("tenant")]
        public string? Tenant { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        [JsonProperty("depart")]
        public string? Depart { get; set; }

        /// <summary>
        /// 学院名称
        /// </summary>
        [JsonProperty("academy")]
        public string? Academy { get; set; }

        /// <summary>
        /// 专业名称
        /// </summary>
        [JsonProperty("major")]
        public string? Major { get; set; }

        /// <summary>
        /// 班级名称
        /// </summary>
        [JsonProperty("className")]
        public string? ClassName { get; set; }

        /// <summary>
        /// 年级
        /// </summary>
        [JsonProperty("grade")]
        public string? Grade { get; set; }

        /// <summary>
        /// 组织全路径
        /// </summary>
        [JsonProperty("orgFullName")]
        public string? OrgFullName { get; set; }
    }


}
