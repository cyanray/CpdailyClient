﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Cpdaily.LoginWorkers
{
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
