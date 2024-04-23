using System;
using System.Runtime.CompilerServices;
using console.Dto;

namespace console.Logger
{
    public static class Logger
    {
        private static ApplicationConfig __applicationConfig;

        public static void SetApplicationConfig(ApplicationConfig applicationConfig)
        {
            __applicationConfig = applicationConfig;
        }

        public static void Debug(String message)
        {
            if (__applicationConfig.Debug)
            {
                Console.WriteLine(message);
            }
        }
    }
}