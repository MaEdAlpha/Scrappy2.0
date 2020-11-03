using System;
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

        public static void SetImplicitWait(int WaitTime)
        {   
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(WaitTime);
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
        public static bool IsValidFormat(string str, Dictionary<string,string> list)
        {
            List<string> items = str.Split(',').ToList();
            int maxValue = list.Count();

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
        public static Dictionary<string, string> CreateCustomLeagueList(string _userInput, Dictionary<string,string> siteList)
        {
            Dictionary<string, string> tempList = new Dictionary<string, string>();
            int[] selectedLeagues = Array.ConvertAll(_userInput.Split(','), int.Parse); // convert userInput into an array of numbers.

            foreach (int item in selectedLeagues)
            {
                tempList.Add(siteList.ElementAt(item).Key, siteList.ElementAt(item).Value);
            }
            return tempList;
        }
        //Returns all leagues in proper string format to add to Dictionary list to be used for scraping
        public static string ReturnEveryLeague(Dictionary<string, string> dictList)
        {
            int count = dictList.Count;
            string chain = "";
            for (int j = 0; j < count; j++)
            {
                chain = chain + j + ",";
            }         
            return chain.Remove(chain.Length - 1);
        }

        public static string GetUniversalTeamName(string TeamName)
        {
            TeamNamesModel Result;

            Result =  Program.TeamNamesLibrary.Find(i => i._idAlias == TeamName);

            if (Result is null) 
            {
                return null;
            }
            else
            { 
                return Result.UniversalTeamName;
            };

        }

        public static string CheckifTeamNameAliasExists(string Alias)
        {
            TeamNamesModel Result;

            Result = Program.TeamNamesLibrary.Find(i => i._idAlias == Alias);

            if (Result is null)
            {
                return null;
            }
            else
            {
                return Result._idAlias;
            };

        }

    }
}
