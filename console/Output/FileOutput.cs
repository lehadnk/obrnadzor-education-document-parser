using System;
using System.IO;
using console.Dto;

namespace console.Output
{
    public class FileOutput
    {
        private readonly ApplicationConfig _applicationConfig;

        public FileOutput(ApplicationConfig applicationConfig)
        {
            _applicationConfig = applicationConfig;
            PrepareReportOutputPath();
        }

        private void PrepareReportOutputPath()
        {
            var outputPath = Path.Combine(_applicationConfig.ExecutablePath, "output");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (!Directory.Exists(_applicationConfig.BrowserDownloadsDirectory))
            {
                Directory.CreateDirectory(_applicationConfig.BrowserDownloadsDirectory);
            }

            _applicationConfig.ReportOutputPath = outputPath;
        }

        public bool SaveReport(ReportDownloadTask task)
        {
            var downloadedReportFileName = Path.Combine(_applicationConfig.BrowserDownloadsDirectory, "Документ_об_образовании.pdf");
            if (!File.Exists(downloadedReportFileName))
            {
                Console.WriteLine("Не удалось найти загруженный файл " + downloadedReportFileName);
                Console.WriteLine("Зайдите на страницу about:config Mozilla Firefox, и проверьте установку переменных browser.downloads.dir и browser.downloads.folderList. Попробуйте изменить значение на другой путь при помощи аргумента программы --seleniumDownloadDir, например: obrnadzor.exe input.json --seleniumDownloadDir=C:\\Downloads");
                return false;
            }
            
            File.Copy(
                downloadedReportFileName, 
                Path.Combine(_applicationConfig.ReportOutputPath, GetReportFileName(task))
            );
            return true;
        }

        public bool IsReportExists(ReportDownloadTask task)
        {
            return File.Exists(Path.Combine(_applicationConfig.ReportOutputPath, GetReportFileName(task)));
        }

        private string GetReportFileName(ReportDownloadTask task)
        {
            return task.registrationNumber + "_" + task.organizationOgrn + ".pdf";
        }
    }
}