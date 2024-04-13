using System;
using System.IO;
using System.Reflection;
using console.Dto;
using console.Input;
using console.Output;
using console.Selenium;

namespace console
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Пример запуска команды: Console.exe <input.json> <C:\\Users\\lehadnk\\>");
                return;
            }

            var applicationConfig = new ApplicationConfig();
            applicationConfig.InputFilePath = args[0];
            applicationConfig.BrowserDownloadsDirectory = args[1];
            applicationConfig.DisplayScale = 2; // @todo Since I have a retina screen
            applicationConfig.ExecutablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fileOutput = new FileOutput(applicationConfig);

            var jsonInput = new JsonInput();
            var taskList = jsonInput.readJsonFile(applicationConfig.InputFilePath);
            foreach (var task in taskList)
            {
                if (fileOutput.isReportExists(task))
                {
                    Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ уже существует");
                    continue;
                }
            
                var reportReader = new ReportReader(applicationConfig);
                if (reportReader.readReport(task))
                {
                    Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ загружен");
                    fileOutput.saveReport(task);                    
                }
                else
                {
                    Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ отсутствует");
                }
            }
        }
    }
}