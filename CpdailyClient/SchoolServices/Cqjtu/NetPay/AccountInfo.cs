using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.SchoolServices.Cqjtu.NetPay
{
    /// <summary>
    /// 一卡通账户信息
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// 学号
        /// </summary>
        [JsonProperty("salaryNo")]
        public string? SchoolId { get; set; }

        /// <summary>
        /// 卡号（意义不明）
        /// </summary>
        [JsonProperty("cardId")]
        public string? CardId { get; set; }

        /// <summary>
        /// 一卡通余额
        /// </summary>
        [JsonProperty("lastMoney")]
        public string? RemainingAmount { get; set; }

        /// <summary>
        /// 一卡通待充值余额
        /// </summary>
        [JsonProperty("unUseMoney")]
        public string? UnaccountedAmount { get; set; }
    }
}
