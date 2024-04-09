using System.IO;
using System.Reflection;
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
            var executablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var outputPath = Path.Combine(executablePath, "output");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            _applicationConfig.ReportOutputPath = outputPath;
        }

        public void saveReport(ReportDownloadTask task)
        {
            var downloadedReportFileName = Path.Combine(this._applicationConfig.BrowserDownloadsDirectory, "Документ_об_образовании.pdf");
            File.Copy(
                downloadedReportFileName, 
                Path.Combine(_applicationConfig.ReportOutputPath, getReportFileName(task))
            );
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