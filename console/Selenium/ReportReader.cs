using System;
using System.IO;
using System.Threading;
using console.Dto;
using console.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace console.Selenium
{
    public class ReportReader
    {
        private readonly string _pageUrl = "https://obrnadzor.gov.ru/gosudarstvennye-uslugi-i-funkczii/7701537808-gosfunction/formirovanie-i-vedenie-federalnogo-reestra-svedenij-o-dokumentah-ob-obrazovanii-i-ili-o-kvalifikaczii-dokumentah-ob-obuchenii/";
        
        private readonly ApplicationConfig _config;
        private FirefoxDriver _driver;
        private Actions _actions;
        private WebDriverWait _wait;
        private IWebElement _iframe;
        
        public ReportReaderThreadExecutor Executor;
        public String Captcha;

        public ReportReader(ApplicationConfig config)
        {
            _config = config;
        }

        public ReportAccessStatus ReadReport(ReportDownloadTask task)
        {
            try
            {
                OpenWebsite();
                InjectToIframe();
                SelectTrainingLevel();
                SelectOrganization(task);
                FillDocumentData(task);

                var reportAccessStatus = ReportAccessStatus.CAPTCHA_IS_INCORRECT;
                while (reportAccessStatus == ReportAccessStatus.CAPTCHA_IS_INCORRECT)
                {
                    reportAccessStatus = DoCaptcha();
                };

                if (reportAccessStatus == ReportAccessStatus.FOUND)
                {
                    SaveReport();                    
                }
                
                return reportAccessStatus;
            }
            finally
            {
                _driver.Quit();
            }
        }

        private void OpenWebsite()
        {
            var service = FirefoxDriverService.CreateDefaultService();
            if (!_config.Debug)
            {
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true;
            }
            
            var options = new FirefoxOptions();
            if (!_config.Debug)
            {
                options.AddArgument("--log-level=3");
            }
            options.SetPreference("browser.download.dir", _config.BrowserDownloadsDirectory);
            options.SetPreference("browser.download.folderList", 2);
            if (_config.Headless)
            {
                options.AddArgument("--headless");         
            }
            
            _driver = new FirefoxDriver(service, options);
            _actions = new Actions(_driver);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl(_pageUrl);
        }

        private void InjectToIframe()
        {
            var openIframeButton = _driver.FindElement(By.CssSelector(".su-spoiler.su-spoiler-style-fancy.su-spoiler-icon-folder-1.center.su-spoiler-closed"));
            openIframeButton.Click();

            _iframe = _driver.FindElement(By.CssSelector("iframe"));
            _actions.ScrollToElement(_iframe);
            
            _driver.SwitchTo().Frame(0);
            
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("[name=trainingLevel]")));
        }

        private void SelectTrainingLevel()
        {
            var select = _driver.FindElement(By.CssSelector("[name=trainingLevel]"));
            select.Click();
            
            var selectOption = _driver.FindElement(By.CssSelector("[value=dpo]"));
            selectOption.Click();
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector(".dpo .searchOrg")));
        }
        
        private void SelectOrganization(ReportDownloadTask task)
        {
            
            var searchOrgOpenButton = _driver.FindElement(By.CssSelector(".dpo .searchOrg"));
            searchOrgOpenButton.Click();
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("#searchText")));
            
            // Pressing these buttons makes no sense so far since the search is done automatically, but I'll leave it here just in case if they decide to switch this behaviour
            // var searchButtons = driver.FindElements(By.CssSelector("#searchOrg button"));
            // var orgSearchNameButton = searchButtons[0];
            // var orgSearchOgrnButton = searchButtons[1];
            if (task.organizationOgrn != null)
            {
                var orgSearchOgrnInput = _driver.FindElement(By.CssSelector("#searchOGRN"));
                orgSearchOgrnInput.SendKeys(task.organizationOgrn);                
            }
            else
            {
                var orgSearchNameInput = _driver.FindElement(By.CssSelector("#searchText"));
                orgSearchNameInput.SendKeys(task.organizationName);
            }
            
            // @todo Handle 0 results
            Thread.Sleep(1000);
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("#searchOrg div.panel.panel-default")));
            
            var firstEntry = _driver.FindElement(By.CssSelector("#searchOrg div.panel.panel-default"));
            _driver.ExecuteScript("arguments[0].scrollIntoView(true);", firstEntry);
            firstEntry.Click();
            
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                By.CssSelector("[style*=\"color: darkblue\"]")));
        }

        private void FillDocumentData(ReportDownloadTask task)
        {
            var surnameInput = _driver.FindElement(By.CssSelector(".dpo input[name=surname]"));
            surnameInput.SendKeys(task.lastName);
            
            var seriesInput = _driver.FindElement(By.CssSelector(".dpo input[name=series]"));
            seriesInput.SendKeys(task.documentSeries);
            
            var numberInput = _driver.FindElement(By.CssSelector(".dpo input[name=number_dpo]"));
            numberInput.SendKeys(task.documentNumber);
            
            var registerNumberInput = _driver.FindElement(By.CssSelector(".dpo input[name=register_number_dpo]"));
            registerNumberInput.SendKeys(task.registrationNumber);

            var yearInput = _driver.FindElement(By.CssSelector(".dpo input[name=year]"));
            yearInput.SendKeys(task.issuedAt);
            _driver.ExecuteScript("$(\".datepicker\").hide();");
            Thread.Sleep(100);
        }

        private ReportAccessStatus DoCaptcha()
        {
            var captchaText = _driver.FindElement(By.CssSelector("input[name=captchaText]"));
            captchaText.SendKeys("");
            captchaText.Clear();

            var ss = new ScreenshotSaver(_driver, _iframe, _config);
            var captchaImagePath = ss.SaveCaptchaImage();

            if (Executor != null)
            {
                Executor.PauseExecution(captchaImagePath);                
            }
            
            captchaText.SendKeys(Captcha);
            Thread.Sleep(500);

            var validateCaptchaButton = _driver.FindElement(By.CssSelector("button[name=checkDoc]"));
            validateCaptchaButton.Submit();
            validateCaptchaButton.Click();
            validateCaptchaButton.SendKeys(Keys.Return);
            
            var _captchaWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(120));
            try
            {
                _captchaWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("#modalWin")));
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Captcha is incorrect");
                return ReportAccessStatus.CAPTCHA_IS_INCORRECT;
            }
            
            try 
            {
                var incorrectCaptchaAlert = _driver.FindElement(By.CssSelector("#modalWin .alert.alert-success"));
                
                var closeAlertButton = _driver.FindElement(By.CssSelector("button[data-dismiss=modal].btn.btn-default"));
                closeAlertButton.Click();
                Thread.Sleep(500);
                
                return ReportAccessStatus.CAPTCHA_IS_INCORRECT;
            } catch (NoSuchElementException) { }

            try
            {
                var formDataIsIncorrectAlert = _driver.FindElement(By.CssSelector("#modalWin .alert.alert-warning"));
                if (formDataIsIncorrectAlert.Text.Contains("Проверьте правильность заполнения полей"))
                {
                    return ReportAccessStatus.FORM_IS_INCORRECT;                    
                }
                if (formDataIsIncorrectAlert.Text.Contains("Уточните поиск или воспользуйтесь формой обратной связи и сообщите Нам о выявленных ошибках или отсутствии данных."))
                {
                    return ReportAccessStatus.DOCUMENT_NOT_FOUND;                    
                }

                return ReportAccessStatus.UNKNOWN_ERROR;
            } catch (NoSuchElementException) { }
            
            ss.SaveDebugScreenshot();
            Logger.Logger.Debug("Документ найден. Сохранен отладочный снимок экрана.");

            return ReportAccessStatus.FOUND;
        }

        private void SaveReport()
        {
            var saveButton = _driver.FindElement(By.CssSelector(".modal-body button"));
            saveButton.Click();
            saveButton.SendKeys(Keys.Return);
            Logger.Logger.Debug("Нажали кнопку сохранения документа");
            Thread.Sleep(1500);
        }
    }
}