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
            prepareReportOutputPath();
        }

        private void prepareReportOutputPath()
        {
            var outputPath = Path.Combine(_applicationConfig.ExecutablePath, "output");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            _applicationConfig.ReportOutputPath = outputPath;
        }

        public bool saveReport(ReportDownloadTask task)
        {
            var downloadedReportFileName = Path.Combine(this._applicationConfig.BrowserDownloadsDirectory, "Документ_об_образовании.pdf");
            if (!File.Exists(downloadedReportFileName))
            {
                Console.WriteLine("Не удалось найти загруженный файл " + downloadedReportFileName);
                Console.WriteLine("Проверьте второй аргумент запускной команды, отвечающий за путь до директории для файлов, загружаемых Mozilla Firefox (по умолчанию: C:\\Users\\<Имя пользователя>\\Downloads>)");
                return false;
            }

            
            
            File.Copy(
                downloadedReportFileName, 
                Path.Combine(_applicationConfig.ReportOutputPath, getReportFileName(task))
            );
            return true;
        }

        public bool isReportExists(ReportDownloadTask task)
        {
            return File.Exists(Path.Combine(_applicationConfig.ReportOutputPath, getReportFileName(task)));
        }

        private string getReportFileName(ReportDownloadTask task)
        {
            return task.lastName + "." + task.documentNumber + ".pdf";
        }
    }
}