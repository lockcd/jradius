using JRadius.Core.Config;
using System;
using System.IO;

namespace JRadius.Server
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ShowUsage();
                Environment.Exit(1);
            }

            var configFilePath = args[0];

            try
            {
                var file = new FileInfo(configFilePath);
                using (var stream = file.OpenRead())
                {
                    // TODO: The Configuration class needs a logger factory
                    Configuration.Initialize(stream, null);
                }
                var server = new JRadiusServer();
                server.Start();
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($"Error: The configuration file '{configFilePath}' does not exist.");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
                ShowStackTrace(e);
            }
        }

        private static void ShowStackTrace(Exception e)
        {
            Console.Error.WriteLine("--- stack trace: ------------------------------");
            Console.Error.WriteLine(e.StackTrace);
            Console.Error.WriteLine("--- end of stack trace. -----------------------");
        }

        private static void ShowUsage()
        {
            Console.Error.WriteLine("Usage: jradius <configfile>");
            Console.Error.WriteLine("    where <configfile> is the filename of the configuration file.");
        }
    }
}
