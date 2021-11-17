using System;
using System.Collections.Generic;
using System.Text;

namespace Cpdaily.LoginWorkers
{
    /// <summary>
    /// 一些学校需要定制 LoginWorker，请使用该属性描述，
    /// 以便通过学校名称寻找到对应的 LoginWorker。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SchoolNameAttribute : Attribute
    {
        public string Name;
        public SchoolNameAttribute(string name)
        {
            Name = name;
        }
    }
}
