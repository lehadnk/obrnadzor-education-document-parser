using System;
using System.Threading;
using console.Dto;
using console.Input;
using console.Selenium;

namespace console.Threading
{
    public class ReportReaderThreadExecutor
    {
        private readonly ApplicationConfig _applicationConfig;
        private readonly ReportReader _reportReader;
        private Thread _uiThread;
        private string _captchaImagePath;

        private ManualResetEvent _manualResetEvent;
        
        public ReportReaderThreadExecutor(ApplicationConfig applicationConfig, ReportReader reportReader)
        {
            _applicationConfig = applicationConfig;
            _reportReader = reportReader;
            _reportReader.Executor = this;
        }
        
        public ReportAccessStatus ExecuteReadReportTask(ReportDownloadTask reportDownloadTask)
        {
            _manualResetEvent = new ManualResetEvent(false);
            return _reportReader.readReport(reportDownloadTask);
        }

        public void PauseExecution(string captchaImagePath)
        {
            Console.WriteLine("Execution paused, waiting for captcha...");
            if (_applicationConfig.Console)
            {
                var captcha = Console.ReadLine();
                ResumeExecution(captcha);
            }
            else
            {
                _captchaImagePath = captchaImagePath;
                _uiThread = new Thread(DisplayCaptchaInputForm);
                _uiThread.Start();
                
                _manualResetEvent.WaitOne();
                Console.WriteLine("");
            }
        }

        public void ResumeExecution(string inputCaptchaText)
        {
            if (!_applicationConfig.Console)
            {
                _manualResetEvent.Set();
            }
            _reportReader.Captcha = inputCaptchaText;
            Console.WriteLine("Execution continued...");
        }

        private void DisplayCaptchaInputForm()
        {
            var windowsFormsCaptchaInput = new WindowsFormsCaptchaInput(this);
            windowsFormsCaptchaInput.DisplayCaptchaInputForm(_captchaImagePath);
        }
    }
}