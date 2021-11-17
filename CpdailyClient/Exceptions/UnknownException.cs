using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.Exceptions
{
    public class UnknownException : Exception
    {
        public UnknownException() : base("未知错误!")
        {
        }
    }
}
