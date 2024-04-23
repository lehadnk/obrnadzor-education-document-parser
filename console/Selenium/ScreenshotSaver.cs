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
        private readonly string _captchaImagePath;
        private readonly string _debugImagePath;

        public ScreenshotSaver(FirefoxDriver _driver, IWebElement iframe, ApplicationConfig _config)
        {
            this._driver = _driver;
            this._iframe = iframe;
            this._config = _config;
            
            _captchaImagePath = Path.Combine(_config.ExecutablePath, "captcha");
            if (!Directory.Exists(_captchaImagePath))
            {
                Directory.CreateDirectory(_captchaImagePath);
            }

            if (_config.Debug)
            {
                _debugImagePath = Path.Combine(_config.ExecutablePath, "debug");
                if (!Directory.Exists(_debugImagePath))
                {
                    Directory.CreateDirectory(_debugImagePath);
                }
            }
        }
        
        public string SaveCaptchaImage()
        {
            var captchaImagePath = Path.Combine(_captchaImagePath, "captcha-" + DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss") + ".png");
            
            var captchaText = _driver.FindElement(By.CssSelector("input[name=captchaText]"));
            captchaText.SendKeys("");
         
            var screenshotPath = Path.Combine(_captchaImagePath, "screenshot-" + DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss") + ".png");
            TakeScreenshot(screenshotPath);
            СropCaptchaImage(screenshotPath, captchaImagePath);

            return captchaImagePath;
        }

        private void TakeScreenshot(string screenshotPath)
        {
            var ssdriver = _driver as ITakesScreenshot;
            var screenshot = ssdriver.GetScreenshot();
            var tempImage = screenshot;
            tempImage.SaveAsFile(screenshotPath);
        }

        private void СropCaptchaImage(string screenshotPath, string captchaImagePath)
        {
            var captchaImage = _driver.FindElement(By.CssSelector(".captchaImg"));
            var captchaDisplayPosition = new Point(AdjustCoordinateToDisplayScale(_iframe.Location.X + captchaImage.Location.X), AdjustCoordinateToDisplayScale(_iframe.Location.Y + captchaImage.Location.Y));
            
            int width = AdjustCoordinateToDisplayScale(captchaImage.Size.Width);
            int height = AdjustCoordinateToDisplayScale(captchaImage.Size.Height);

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
        
        private int AdjustCoordinateToDisplayScale(int coord)
        {
            return (int) Math.Round(coord * _config.DisplayScale);
        }

        public void SaveDebugScreenshot()
        {
            if (!_config.Debug)
            {
                return;
            }
            var debugImagePath = Path.Combine(_debugImagePath, "captcha-" + DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss") + ".png");
            TakeScreenshot(debugImagePath);
        }
    }
}