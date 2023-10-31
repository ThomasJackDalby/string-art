using System;

namespace StringArt
{
    public interface ILogger
    {
        void Log(string message, params object[] args);
    }

    public class Logger : ILogger
    {
        public void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }

    public static class LoggingManager
    {
        public static ILogger GetLogger(string name) => new Logger();
        public static ILogger GetLogger(Type type) => GetLogger(type.Name);
        public static ILogger GetLogger<T>() => GetLogger(typeof(T));
    }
}