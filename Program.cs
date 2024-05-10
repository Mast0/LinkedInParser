using Aspose.Zip;
using Aspose.Zip.Saving;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace LinkeInAuthParserV2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            DotNetEnv.Env.Load(projectDirectory + "\\.env");
            ChromeOptions options = new ChromeOptions();
            options.AddArgument(Environment.GetEnvironmentVariable("USER_AGENT"));
            IWebDriver driver = new ChromeDriver(options);
            try
            {
                driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("LINKEDIN_LINK"));
                driver.Manage().Window.Maximize();
                System.Threading.Thread.Sleep(2000);
                if (!Directory.Exists(projectDirectory + "\\cookies"))
                    Directory.CreateDirectory(projectDirectory + "\\cookies");
                if (!File.Exists(projectDirectory + "\\cookies\\json-cookies.json"))
                {
                    System.Threading.Thread.Sleep(30000);
                    JsonCookies.SaveCookiesAsJson(driver, projectDirectory + "\\cookies\\json-cookies.json");
                }
                else
                {
                    JsonCookies.GetCookiesFromJson(driver, projectDirectory + "\\cookies\\json-cookies.json");
                    driver.Navigate().Refresh();
                }
                System.Threading.Thread.Sleep(2000);
                IWebElement goToProfileBtn = driver.FindElement(By.XPath("//*[@class=\"ember-view block\"]"));
                goToProfileBtn.Click();
                System.Threading.Thread.Sleep(2000);
                IWebElement profilePhoto = driver.FindElement(By.XPath("//*[@class=\"presence-entity__image EntityPhoto-circle-1  evi-image lazy-image ember-view\"]"));
                string imageSrc = profilePhoto.GetAttribute("src");
                WebClient client = new WebClient();
                if (!Directory.Exists(projectDirectory + "\\res"))
                    Directory.CreateDirectory(projectDirectory + "\\res");
                client.DownloadFile(imageSrc, projectDirectory + "\\res\\profilePhoto.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        static void AddProxyExtention(ChromeOptions options, string projectDirectory)
        {
            DotNetEnv.Env.Load(projectDirectory + "\\.env");
            string manifest_json = "{" +
                "\"version\": \"1.0.0\"," +
                "\"manifest_version\": 2," +
                "\"name\": \"Chrome Proxy\"," +
                "\"permissions\": [" +
                "\"proxy\"," +
                "\"tabs\"," +
                "\"unlimitedStorage\"," +
                "\"storage\"," +
                "\"<all_urls>\"," +
                "\"webRequest\"," +
                "\"webRequestBlocking\"" +
                "]," +
                "\"background\": {" +
                "\"scripts\": [\"background.js\"]" +
                "}," +
                "\"minimum_chrome_version\":\"22.0.0\"" +
                "}";
            string background_js = "" +
                "var config = {\n" +
                       "\t\tmode: \"fixed_servers\",\n" +
                       "\t\trules: {\n" +
                        "\t\tsingleProxy: {\n" +
                            "\t\t\tscheme: \"http\",\n" +
                            $"\t\t\thost: {Environment.GetEnvironmentVariable("HTTP_HOST")},\n" +
                            $"\t\t\tport: parseInt({Environment.GetEnvironmentVariable("HTTP_PORT")})\n" +
                        "\t\t}," +
                        "\t\tbypassList: [\"localhost\"]\n" +
                        "\t\t}\n" +
                    "\t};\n\n" +

                "chrome.proxy.settings.set({value: config, scope: \"regular\"}, function() {});\n\n" +

                "function callbackFn(details) {\n" +
                    "\treturn {\n" +
                    "\t\tauthCredentials: {\n" +
                           $"\t\t\tusername: {Environment.GetEnvironmentVariable("HTTP_USER")},\n" +
                           $"\t\t\tpassword: {Environment.GetEnvironmentVariable("HTTP_PASS")}\n" +
                        "\t\t}\n" +
                    "\t};\n" +
                "}\n\n" +

                "chrome.webRequest.onAuthRequired.addListener(\n" +
                            "\t\t\tcallbackFn,\n" +
                            "\t\t\t{urls: [\"<all_urls>\"]},\n" +
                            "\t\t\t['blocking']\n" +
                ");";

            using (FileStream zipFile = File.Open("proxy.zip", FileMode.Create))
            {
                using (var archive = new Archive())
                {
                    // Add files to the archive
                    archive.CreateEntry("manifest.json", manifest_json);
                    archive.CreateEntry("background.js", background_js);
                    // ZIP the files
                    archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.ASCII });
                }
            }

            options.AddExtension(projectDirectory + "proxy.zip");
        }
    }
}
