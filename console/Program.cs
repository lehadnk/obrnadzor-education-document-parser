using System;
using System.IO;
using System.Reflection;
using System.Text;
using console.Dto;
using console.Input;
using console.Output;
using console.Selenium;
using console.Threading;

namespace console
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            ;
            Console.WriteLine("Obrnadzor Report Parser v" + Assembly.GetExecutingAssembly().GetName().Version);
            if (args.Length < 1)
            {
                Console.WriteLine("Использование: Console.exe <input.json>");
                return 1;
            }

            var applicationConfig = new ApplicationConfig();
            applicationConfig.InputFilePath = args[0];
            applicationConfig.DisplayScale = 1;
            applicationConfig.ExecutablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            applicationConfig.BrowserDownloadsDirectory = Path.Combine(applicationConfig.ExecutablePath, "downloads");
            
            foreach (var argument in args)
            {
                if (argument == "--headless=0")
                {
                    applicationConfig.Headless = false;
                }
                if (argument.StartsWith("--displayScale="))
                {
                    try
                    {
                        applicationConfig.DisplayScale = int.Parse(argument.Substring(15));
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Неверное значение displayScale: " + argument.Substring(15) + ". Если вы вводите дробное число, попробуйте иной разделитель.");
                    }
                }
                if (argument == "--console")
                {
                    applicationConfig.Console = true;
                }

                if (argument == "--seleniumDownloadDir=")
                {
                    applicationConfig.BrowserDownloadsDirectory = argument.Substring(23);
                }
            }
            
            var fileOutput = new FileOutput(applicationConfig);

            var jsonInput = new JsonInput();
            var taskList = jsonInput.readJsonFile(applicationConfig.InputFilePath);
            
            var reportReader = new ReportReader(applicationConfig);
            var reportReaderThreadExecutor = new ReportReaderThreadExecutor(applicationConfig, reportReader);
            
            foreach (var task in taskList)
            {
                if (fileOutput.IsReportExists(task))
                {
                    Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ уже существует");
                    continue;
                }
                
                Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - начинаем загрузку отчета...");
                var reportAccessStatus = reportReaderThreadExecutor.ExecuteReadReportTask(task);
                switch (reportAccessStatus)
                {
                    case ReportAccessStatus.FOUND:
                        var fileOutputPath = fileOutput.SaveReport(task);
                        if (fileOutputPath == null)
                        {
                            return 1;
                        }
                        
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - загрузка завершена. Файл сохранен по пути: " + fileOutputPath);
                        break;
                    case ReportAccessStatus.FORM_IS_INCORRECT:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - неправильный формат запроса");
                        break;
                    case ReportAccessStatus.DOCUMENT_NOT_FOUND:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ не существует");
                        break;
                    case ReportAccessStatus.CAPTCHA_IS_INCORRECT:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - неверная captcha");
                        break;
                    default:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - неизвестная ошибка");
                        break;
                }
            }

            return 0;
        }
    }
}