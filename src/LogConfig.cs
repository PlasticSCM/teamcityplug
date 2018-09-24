using System.IO;
using System.Reflection;

namespace TeamCityPlug.Configuration
{
    internal static class LogConfig
    {
        internal static string GetLogConfigFile()
        {
            return Path.Combine(GetExecutingAssemblyDirectory(), LOG_CONFIG_FILE);
        }

        static string GetExecutingAssemblyDirectory()
        {
            return Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
        }

        const string LOG_CONFIG_FILE = "teamcityplug.log.conf";
    }
}
