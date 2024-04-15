using System;
using System.Threading;
using console.Dto;
using console.Selenium;

namespace console.Threading
{
    public class ReportReaderThreadExecutor
    {
        private readonly ReportReader _reportReader;
        private bool isConsoleMode = true;

        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        
        public ReportReaderThreadExecutor(ReportReader reportReader)
        {
            _reportReader = reportReader;
            _reportReader.Executor = this;
        }
        
        public ReportAccessStatus ExecuteReadReportTask(ReportDownloadTask reportDownloadTask)
        {
            return _reportReader.readReport(reportDownloadTask);
        }

        public void PauseExecution()
        {
            Console.WriteLine("Execution paused, waiting for captcha...");
            if (this.isConsoleMode)
            {
                var captcha = Console.ReadLine();
                _reportReader.Captcha = captcha;
            }
            else
            {
                _manualResetEvent.WaitOne();                
            }
        }

        public void ResumeExecution()
        {
            _manualResetEvent.Set();
            Console.WriteLine("Execution continued...");
        }
    }
}