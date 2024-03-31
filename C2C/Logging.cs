using System;

namespace C2C
{
    internal static class Logging
    {
#if DEBUG
        public static void Log(string message, params object[] args) => Console.WriteLine(string.Format("[C2C] " + message, args));
#else
        public static void Log(string message, params object[] args)
        {
        }
#endif
    }
}
