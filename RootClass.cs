﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Scrappy2._0
{
    class RootClass : Humanize
    {
        public static IWebDriver driver;
        public static DateTime timeZero;
        public static void GetRootPage(string url)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--test-type");
            options.AddArgument("--disable-extensions");
            options.AddArguments("disable-infobars", "--disable-extensions");
            options.AddArguments("--ignore-certificate-errors");
            options.AddArgument("--ignore-ssl-errors");
            options.AddArguments("--disable-notifications");
            options.AddArguments("--disable-popup-blocking");
            options.AddArguments("--disable-plugins-discovery");
            options.AddArguments("--disable-blink-features=AutomationControlled");
            options.AddArguments("--incognito");
            options.AddArguments("--start-maximized");
            options.AddExcludedArguments("enable-automation");

            driver = new ChromeDriver(options);
            timeZero = DateTime.UtcNow;
            RandomSleep(2099);
            driver.Navigate().GoToUrl(url);
            RandomSleep(3000);
        }
        public static IWebElement AWebElement(string xPath) //Return IWebElement using xPath.
        {
            try
            {
                IWebElement element = driver.FindElement(By.XPath(xPath));
                return element;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public static List<IWebElement> WebElements(string xPath) //Return a list of IWebElements.
        {
            List<IWebElement> elements = driver.FindElements(By.XPath(xPath)).ToList();
            return elements;
        }

        public static void Quit()
        {
            driver.Quit();
        }

        public static bool ThrowMessage()
        {
            Console.WriteLine("Enter a single character 'y' or 'n' only");
            return false;
        }
        public static bool IsValidFormat(string str)
        {
            List<string> items = str.Split(',').ToList();
            int maxValue = Bet365.leagueCountry.Count();

            foreach (string item in items)
            {
                int selectedValue;
                bool isNumeric = int.TryParse(item, out selectedValue);

                if (isNumeric == false)
                {
                    Console.WriteLine("SMACK! Try again");
                    return false;
                }
                if (selectedValue > maxValue)
                {
                    Console.WriteLine("{0} value is out of range.", selectedValue);
                    return false;
                }
            }
            return true;
        }
    }
}