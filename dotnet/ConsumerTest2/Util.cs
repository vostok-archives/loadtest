using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;

namespace ConsumerTest
{
    public class Util
    {
        public static void ConfigureLog4Net()
        {
            var assemblyDirectory = AssemblyDirectory;
            GlobalContext.Properties["LogRoot"] = Path.Combine(assemblyDirectory, @"..\Logs");
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.ConfigureAndWatch(logRepository, new FileInfo(Path.Combine(assemblyDirectory, "log4net.config")));
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}