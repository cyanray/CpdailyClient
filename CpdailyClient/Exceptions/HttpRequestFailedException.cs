using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.Exceptions
{
    public class HttpRequestFailedException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; init; }
        public HttpRequestFailedException(HttpStatusCode code, string? message = "HTTP 请求时出现错误!") : base(message)
        {
            HttpStatusCode = code;
        }
    }
}
