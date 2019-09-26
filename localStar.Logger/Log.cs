using System;
using System.Diagnostics;
using System.Text;

namespace localStar.Logger
{
    public static class Log
    {
        private static DateTime InitTime = Process.GetCurrentProcess().StartTime;
        private static void Logging(string prefix, string str)
        {
            var tmp = DateTime.Now - InitTime;
            Console.WriteLine("[{0}][{1:N3}]\t\t{2}", prefix, tmp.TotalSeconds, str);
        }
        public static void lowLevel(string log) => Logging("lowLevel", log);
        public static void lowLevel(string log, params object[] objs) => Logging("lowLevel", String.Format(log, objs));

        public static void debug(string log) => Logging("Debug", log);
        public static void debug(string log, params object[] objs) => Logging("Debug", String.Format(log, objs));

        public static void info(string log) => Logging("Info", log);
        public static void info(string log, params object[] objs) => Logging("Info", String.Format(log, objs));

        public static void warn(string log) => Logging("Warn", log);
        public static void warm(string log, params object[] objs) => Logging("Warn", String.Format(log, objs));

        public static void error(string log) => Logging("Error", log);
        public static void error(string log, params object[] objs) => Logging("Error", String.Format(log, objs));

        public static void fatal(string log) => Logging("Fatal", log);
        public static void fatal(string log, params object[] objs) => Logging("Fatal", String.Format(log, objs));


        public static void Init()
        {
            Console.WriteLine(" _                    _ ____  _");
            Console.WriteLine("| |    ___   ___ __ _| / ___|| |_ __ _ _ __");
            Console.WriteLine("| |   / _ \\ / __/ _` | \\___ \\| __/ _` | '__|");
            Console.WriteLine("| |__| (_) | (_| (_| | |___) | || (_| | |");
            Console.WriteLine("|_____\\___/ \\___\\__,_|_|____/ \\__\\__,_|_|");
        }
    }
}