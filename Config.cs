using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public static class Config
    {
        private static IConfigurationRoot _Values;
        public static IConfigurationRoot Values
        {
            get
            {
                if (_Values == null)
                {
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    var builder = new ConfigurationBuilder()
                        .AddJsonFile($"appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{env}.json", true, true)
                        .AddEnvironmentVariables();
                    _Values = builder.Build();
                }

                return _Values;
            }
        }        
    }
}
