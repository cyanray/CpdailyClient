using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.Exceptions
{
    public class DeserializeResponseException : Exception
    {
        public string? Content { get; init; }

        public DeserializeResponseException(string? content) : base("解析响应内容时出错!")
        {
            Content = content;
        }
    }
}
