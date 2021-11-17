using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.Exceptions
{
    public class CpdailyApiException : Exception
    {
        public int ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public CpdailyApiException(int code, string? message)
            : base($"Cpdaily API 响应了错误或数据为Null，错误信息: {message}, 错误代码: {code}。")
        {
            ErrorCode = code;
            ErrorMessage = message;
        }
    }
}
