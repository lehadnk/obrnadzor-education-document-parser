namespace console.Dto
{
    public class ApplicationConfig
    {
        public string InputFilePath { get; set; }
        public string BrowserDownloadsDirectory { get; set; }
        public string ReportOutputPath { get; set; }
        public float DisplayScale { get; set; }
        public string ExecutablePath { get; set; }
        public bool Headless { get; set; } = false;
    }
}