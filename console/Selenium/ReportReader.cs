using System;
using System.Threading;
using console.Dto;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace console.Selenium
{
    public class ReportReader
    {
        private FirefoxDriver _driver;
        private Actions _actions;
        private WebDriverWait _wait;
        
        public ReportReader()
        {
                _driver = new FirefoxDriver();
                _actions = new Actions(_driver);
                _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));            
        }

        public bool readReport(ReportDownloadTask task)
        {
            try
            {
                
                
                openWebsite();
                injectToIframe();
                selectTrainingLevel();
                selectOrganization(task);
                fillDocumentData(task);
                doCaptcha();
                if (isReportAvailable())
                {
                    saveReport();                
                }
                else
                {
                    return false;
                }
                
                return true;
            }
            finally
            {
                _driver.Close();
            }
        }

        private void openWebsite()
        {
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl("https://obrnadzor.gov.ru/gosudarstvennye-uslugi-i-funkczii/7701537808-gosfunction/formirovanie-i-vedenie-federalnogo-reestra-svedenij-o-dokumentah-ob-obrazovanii-i-ili-o-kvalifikaczii-dokumentah-ob-obuchenii/");
        }

        private void injectToIframe()
        {
            var openIframeButton = _driver.FindElement(By.CssSelector(".su-spoiler.su-spoiler-style-fancy.su-spoiler-icon-folder-1.center.su-spoiler-closed"));
            openIframeButton.Click();
            
            _actions.ScrollToElement(_driver.FindElement(By.CssSelector("iframe")));
            
            _driver.SwitchTo().Frame(0);
            
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("[name=trainingLevel]")));
        }

        private void selectTrainingLevel()
        {
            var select = _driver.FindElement(By.CssSelector("[name=trainingLevel]"));
            select.Click();
            
            var selectOption = _driver.FindElement(By.CssSelector("[value=dpo]"));
            selectOption.Click();
            _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector(".dpo .searchOrg")));
        }
        
        private void selectOrganization(ReportDownloadTask task)
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

        private void fillDocumentData(ReportDownloadTask task)
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

        private void doCaptcha()
        {
            var captchaText = _driver.FindElement(By.CssSelector("input[name=captchaText]"));
            captchaText.SendKeys("");

            var waitForCaptcha = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
            waitForCaptcha.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("#modalWin")));
        }

        private void saveReport()
        {
            var saveButton = _driver.FindElement(By.CssSelector(".modal-body button"));
            saveButton.Click();
        }
        
        private bool isReportAvailable()
        {
            try
            {
                var errorText = _driver.FindElement(By.CssSelector("#modalWin .alert"));
                return false;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        }
    }
}