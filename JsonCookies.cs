using OpenQA.Selenium;
using System;
using Newtonsoft.Json;
using System.IO;


namespace LinkeInAuthParserV2
{
    public static class JsonCookies
    {
        public static void SaveCookiesAsJson(this IWebDriver driver, string filepath)
        {
            var cookies = driver.Manage().Cookies.AllCookies;
            string json = JsonConvert.SerializeObject(cookies, Formatting.Indented);
            File.WriteAllText(filepath, json);

        }

        public static void GetCookiesFromJson(this IWebDriver driver, string filepath)
        {
            string json = File.ReadAllText(filepath);
            var cookies = JsonConvert.DeserializeObject<dynamic>(json);
            foreach (var c in cookies)
            {
                string name = c.name;
                string value = c.value;
                string domain = c.domain;
                string path = c.path;
                bool secure = c.secure;
                bool httpOnly = c.httpOnly;
                string sameSite = c.sameSite;

                var cookie = new Cookie(name, value, domain, path, null, secure, httpOnly, sameSite);

                driver.Manage().Cookies.AddCookie(cookie);
            }
        }
    }
}
