using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Xunit;
using WebRTCme;
using OpenQA.Selenium.Support.Extensions;

namespace WebRTCme.Tests.E2EWebApiTests
{
    public class MediaStreamTests : IDisposable
    {
        private readonly IWebDriver _webDriver;
        private const string AppUrl = "https://localhost:5001";

        public MediaStreamTests()
        {

            var options = new ChromeOptions();
            options.AcceptInsecureCertificates = true;
            options.AddArgument("--headless");
            _webDriver = new ChromeDriver(options);
        }

        public void Dispose()
        {
            _webDriver.Dispose();
        }

        [Fact]
        public void MediaStreamChecks()
        {
            _webDriver.Navigate().GoToUrl(AppUrl + "/");

            ///////////////////// TODO: HOW TO USE IJSRuntime????


            //var js = _webDriver as IJavaScriptExecutor;
            //var title = js.ExecuteScript("return document.title");
            var title = _webDriver.ExecuteJavaScript<string>("return document.title");
            
            //_webDriver.
            var webRtc = CrossWebRtc.Current;
            //var window = webRtc.Window(JsRuntime);


            //var wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(100));
            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.CssSelector("h1")));


            _webDriver.Quit();
        }
    }
}
