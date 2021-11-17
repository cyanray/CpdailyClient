using Cpdaily.LoginWorkers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace Cpdaily
{
    internal class Utils
    {
        public static DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        public static JsonSerializerSettings GlobalSetting = new JsonSerializerSettings
        {
            ContractResolver = contractResolver
        };

        public static Type? GetLoginWorkerByName(string name)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(ILoginWorker));
            if (assembly == null) return null;
            var loginWorkerTypes = assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(ILoginWorker)))
                .ToList();

            foreach (var type in loginWorkerTypes)
            {
                if (type.GetCustomAttributes(typeof(SchoolNameAttribute), false).FirstOrDefault() 
                        is SchoolNameAttribute schoolNameAttribute)
                {
                    if (schoolNameAttribute.Name == name)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

    }
}
