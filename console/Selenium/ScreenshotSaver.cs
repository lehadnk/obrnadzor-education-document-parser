using System;
using System.Drawing;
using System.IO;
using console.Dto;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace console.Selenium
{
    public class ScreenshotSaver
    {
        private readonly FirefoxDriver _driver;
        private readonly IWebElement _iframe;
        private readonly ApplicationConfig _config;

        public ScreenshotSaver(FirefoxDriver _driver, IWebElement iframe, ApplicationConfig _config)
        {
            this._driver = _driver;
            this._iframe = iframe;
            this._config = _config;
        }
        
        public void saveCaptchaImage()
        {
            var captchaImagePath = Path.Combine(_config.ExecutablePath, "captcha.png");
            
            var captchaText = _driver.FindElement(By.CssSelector("input[name=captchaText]"));
            captchaText.SendKeys("");
         
            var screenshotPath = Path.Combine(_config.ExecutablePath, "screenshot.png");
            takeScreenshot(screenshotPath);
            cropCaptchaImage(screenshotPath, captchaImagePath);
        }

        private void takeScreenshot(string screenshotPath)
        {
            var ssdriver = _driver as ITakesScreenshot;
            var screenshot = ssdriver.GetScreenshot();
            var tempImage = screenshot;
            tempImage.SaveAsFile(screenshotPath);
        }

        private void cropCaptchaImage(string screenshotPath, string captchaImagePath)
        {
            var captchaImage = _driver.FindElement(By.CssSelector(".captchaImg"));
            var captchaDisplayPosition = new Point(adjustCoordinateToDisplayScale(_iframe.Location.X + captchaImage.Location.X), adjustCoordinateToDisplayScale(_iframe.Location.Y + captchaImage.Location.Y));
            
            int width = adjustCoordinateToDisplayScale(captchaImage.Size.Width);
            int height = adjustCoordinateToDisplayScale(captchaImage.Size.Height);

            var section = new Rectangle(captchaDisplayPosition, new Size(width, height));
            var source = new Bitmap(screenshotPath);
            var captchaImageBitmap = CropImage(source, section);

            captchaImageBitmap.Save(captchaImagePath);
        }
        
        private Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bmp = new Bitmap(section.Width, section.Height);
            var g = Graphics.FromImage(bmp);
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
            return bmp;
        }
        
        private int adjustCoordinateToDisplayScale(int coord)
        {
            return (int)Math.Round((coord) * _config.DisplayScale);
        }
    }
}