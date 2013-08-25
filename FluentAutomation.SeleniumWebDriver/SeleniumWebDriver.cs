using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAutomation.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using System.IO;
using OpenQA.Selenium.PhantomJS;
using FluentAutomation.Wrappers;
using OpenQA.Selenium.IE;
using FluentAutomation.Exceptions;

namespace FluentAutomation
{
    /// <summary>
    /// Selenium WebDriver FluentAutomation Provider
    /// </summary>
    public class SeleniumWebDriver
    {
        /// <summary>
        /// Supported browsers for the FluentAutomation Selenium provider.
        /// </summary>
        public enum Browser
        {
            /// <summary>
            /// Internet Explorer. Before using, make sure to set ProtectedMode settings to be the same for all zones.
            /// </summary>
            InternetExplorer = 1,

            /// <summary>
            /// Internet Explorer (64-bit). Before using, make sure to set ProtectedMode settings to be the same for all zones.
            /// </summary>
            InternetExplorer64 = 2,

            /// <summary>
            /// Mozilla Firefox
            /// </summary>
            Firefox = 3,

            /// <summary>
            /// Google Chrome
            /// </summary>
            Chrome = 4,

            /// <summary>
            /// PhantomJS - Experimental - Headless browser - Support is Experimental
            /// </summary>
            PhantomJs = 5,

            /// <summary>
            /// Safari - Experimental - Only usable with a Remote URI
            /// </summary>
            Safari = 6,

            /// <summary>
            /// iPad - Experimental - Only usable with a Remote URI
            /// </summary>
            iPad = 7,

            /// <summary>
            /// iPhone - Experimental - Only usable with a Remote URI
            /// </summary>
            iPhone = 8,

            /// <summary>
            /// Android - Experimental - Only usable with a Remote URI
            /// </summary>
            Android = 9
        }

        /// <summary>
        /// Currently selected <see cref="Browser"/>.
        /// </summary>
        public static Browser SelectedBrowser;

        /// <summary>
        /// Bootstrap Selenium provider and utilize Firefox.
        /// </summary>
        public static void Bootstrap()
        {
            Bootstrap(Browser.Firefox);
        }

        /// <summary>
        /// Bootstrap Selenium provider and utilize the specified <paramref name="browser"/>.
        /// </summary>
        /// <param name="browser"></param>
        public static void Bootstrap(Browser browser)
        {
            SeleniumWebDriver.SelectedBrowser = browser;

            FluentAutomation.Settings.Registration = (container) =>
            {
                container.Register<ICommandProvider, CommandProvider>();
                container.Register<IExpectProvider, ExpectProvider>();
                container.Register<IFileStoreProvider, LocalFileStoreProvider>();

                string driverPath = string.Empty;

                switch (SeleniumWebDriver.SelectedBrowser)
                {
                    case Browser.InternetExplorer:
                        driverPath = EmbeddedResources.UnpackFromAssembly("IEDriverServer32.exe", "IEDriverServer.exe", Assembly.GetAssembly(typeof(SeleniumWebDriver)));
                        container.Register<IWebDriver>((c, o) => { return new IEDriverWrapper(Path.GetDirectoryName(driverPath)); });
                        break;
                    case Browser.InternetExplorer64:
                        driverPath = EmbeddedResources.UnpackFromAssembly("IEDriverServer64.exe", "IEDriverServer.exe", Assembly.GetAssembly(typeof(SeleniumWebDriver)));
                        container.Register<IWebDriver>((c, o) => { return new IEDriverWrapper(Path.GetDirectoryName(driverPath)); });
                        break;
                    case Browser.Firefox:
                        container.Register<IWebDriver, OpenQA.Selenium.Firefox.FirefoxDriver>();
                        break;
                    case Browser.Chrome:
                        driverPath = EmbeddedResources.UnpackFromAssembly("chromedriver.exe", Assembly.GetAssembly(typeof(SeleniumWebDriver)));
                        container.Register<IWebDriver>((c, o) => { return new ChromeDriver(Path.GetDirectoryName(driverPath)); });
                        break;
                    case Browser.PhantomJs:
                        driverPath = EmbeddedResources.UnpackFromAssembly("phantomjs.exe", Assembly.GetAssembly(typeof(SeleniumWebDriver)));
                        container.Register<IWebDriver>((c, o) => { return new PhantomJSDriver(Path.GetDirectoryName(driverPath)); });
                        break;
                }
            };
        }

        /// <summary>
        /// Bootstrap Selenium provider using a Remote web driver targeting the requested browser
        /// </summary>
        /// <param name="driverUri"></param>
        /// <param name="browser"></param>
        public static void Bootstrap(Uri driverUri, Browser browser)
        {
            SeleniumWebDriver.SelectedBrowser = browser;

            FluentAutomation.Settings.Registration = (container) =>
            {
                container.Register<ICommandProvider, CommandProvider>();
                container.Register<IExpectProvider, ExpectProvider>();
                container.Register<IFileStoreProvider, LocalFileStoreProvider>();

                DesiredCapabilities browserCapabilities = GetBrowserCapabilities(SeleniumWebDriver.SelectedBrowser);
                container.Register<IWebDriver, EnhancedRemoteWebDriver>(new EnhancedRemoteWebDriver(driverUri, browserCapabilities));
            };
        }

        /// <summary>
        /// Bootstrap Selenium provider using a Remote web driver targeting the requested browser with extra capabilities enabled.
        /// </summary>
        /// <param name="driverUri"></param>
        /// <param name="browser"></param>
        /// <param name="additionalCapabilities"></param>
        public static void Bootstrap(Uri driverUri, Browser browser, Dictionary<string, object> additionalCapabilities)
        {
            var browserCapabilities = GetBrowserCapabilities(browser);
            FluentAutomation.Settings.Registration = (container) =>
            {
                container.Register<ICommandProvider, CommandProvider>();
                container.Register<IExpectProvider, ExpectProvider>();
                container.Register<IFileStoreProvider, LocalFileStoreProvider>();

                foreach (var cap in additionalCapabilities)
                {
                    browserCapabilities.SetCapability(cap.Key, cap.Value);
                }

                container.Register<IWebDriver, EnhancedRemoteWebDriver>(new EnhancedRemoteWebDriver(driverUri, browserCapabilities));
            };
        }

        /// <summary>
        /// Bootstrap Selenium provider using a Remote web driver service with the requested capabilities
        /// </summary>
        /// <param name="driverUri"></param>
        /// <param name="capabilities"></param>
        public static void Bootstrap(Uri driverUri, Dictionary<string, object> capabilities)
        {
            FluentAutomation.Settings.Registration = (container) =>
            {
                container.Register<ICommandProvider, CommandProvider>();
                container.Register<IExpectProvider, ExpectProvider>();
                container.Register<IFileStoreProvider, LocalFileStoreProvider>();

                var browserCapabilities = SeleniumWebDriver.SelectedBrowser != null ? GetBrowserCapabilities(SeleniumWebDriver.SelectedBrowser) : null;
                if (browserCapabilities == null)
                {
                    browserCapabilities = new DesiredCapabilities(capabilities);
                }
                else
                {
                    foreach (var cap in capabilities)
                    {
                        browserCapabilities.SetCapability(cap.Key, cap.Value);
                    }
                }

                container.Register<IWebDriver, EnhancedRemoteWebDriver>(new EnhancedRemoteWebDriver(driverUri, browserCapabilities));
            };
        }

        private static DesiredCapabilities GetBrowserCapabilities(Browser browser)
        {
            DesiredCapabilities browserCapabilities = null;

            switch (browser)
            {
                case Browser.InternetExplorer:
                case Browser.InternetExplorer64:
                    browserCapabilities = DesiredCapabilities.InternetExplorer();
                    break;
                case Browser.Firefox:
                    browserCapabilities = DesiredCapabilities.Firefox();
                    break;
                case Browser.Chrome:
                    browserCapabilities = DesiredCapabilities.Chrome();
                    break;
                case Browser.PhantomJs:
                    browserCapabilities = DesiredCapabilities.PhantomJS();
                    break;
                case Browser.Safari:
                    browserCapabilities = DesiredCapabilities.Safari();
                    break;
                case Browser.iPad:
                    browserCapabilities = DesiredCapabilities.IPad();
                    break;
                case Browser.iPhone:
                    browserCapabilities = DesiredCapabilities.IPhone();
                    break;
                case Browser.Android:
                    browserCapabilities = DesiredCapabilities.Android();
                    break;
                default:
                    throw new FluentException("Selected browser [{0}] not supported. Unable to determine appropriate capabilities.", SeleniumWebDriver.SelectedBrowser.ToString());
            }

            browserCapabilities.IsJavaScriptEnabled = true;
            return browserCapabilities;
        }
    }
}
