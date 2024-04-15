﻿using System;
using System.IO;
using System.Reflection;
using console.Dto;
using console.Input;
using console.Output;
using console.Selenium;
using console.Threading;

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
            foreach (var argument in args)
            {
                if (argument == "--headless")
                {
                    applicationConfig.Headless = true;
                }
            }
            
            var fileOutput = new FileOutput(applicationConfig);

            var jsonInput = new JsonInput();
            var taskList = jsonInput.readJsonFile(applicationConfig.InputFilePath);
            
            var reportReader = new ReportReader(applicationConfig);
            var reportReaderThreadExecutor = new ReportReaderThreadExecutor(reportReader);
            
            foreach (var task in taskList)
            {
                if (fileOutput.isReportExists(task))
                {
                    Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ уже существует");
                    continue;
                }

                var reportAccessStatus = reportReaderThreadExecutor.ExecuteReadReportTask(task);
                switch (reportAccessStatus)
                {
                    case ReportAccessStatus.FOUND:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ загружен");
                        fileOutput.saveReport(task);
                        break;
                    case ReportAccessStatus.FORM_IS_INCORRECT:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - некорректные данные для формы поиска");
                        break;
                    case ReportAccessStatus.DOCUMENT_NOT_FOUND:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - документ отсутствует");
                        break;
                    case ReportAccessStatus.CAPTCHA_IS_INCORRECT:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - не введена captcha");
                        break;
                    default:
                        Console.WriteLine(task.lastName + " (" + task.documentNumber + ") - неизвестная ошибка");
                        break;
                }
            }
        }
    }
}