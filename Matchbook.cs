using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using OpenQA.Selenium.Internal;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using System.Data;

namespace Scrappy2._0
{
    class Matchbook : RootClass
    {
        private static Dictionary<string, string> matchbookUrlDict = new Dictionary<string, string>();
        private static Dictionary<string, string> customList = new Dictionary<string, string>();
        private static Dictionary<string, string> TabRefList = new Dictionary<string, string>();
        public static int pathCount = 0; //Checksum comparer.
        private static bool validate;
        private static string userInput;

        public static void InitiateList()
        {
            matchbookUrlDict.Add("UEFA Champions League", "https://www.matchbook.com/events/soccer/regional/uefa-champions-league");
            matchbookUrlDict.Add("UEFA Europa League", "");
            matchbookUrlDict.Add("Scotland Premiership", "https://www.matchbook.com/events/soccer/scotland/premiership");
            matchbookUrlDict.Add("England Premier League", "https://www.matchbook.com/events/soccer/premier-league");
            matchbookUrlDict.Add("England Championship", "https://www.matchbook.com/events/soccer/england/championship");
            matchbookUrlDict.Add("England League 1", "https://www.matchbook.com/events/soccer/england/league-1");
            matchbookUrlDict.Add("England League 2", "https://www.matchbook.com/events/soccer/england/league-2");
            
            matchbookUrlDict.Add("England EFL Cup", "");
            
            matchbookUrlDict.Add("Spanish Primera Liga", "https://www.matchbook.com/events/soccer/spain/la-liga");
            
            matchbookUrlDict.Add("Germany Bundesliga I", "https://www.matchbook.com/events/soccer/germany/bundesliga");
            matchbookUrlDict.Add("Italy Serie A", "https://www.matchbook.com/events/soccer/italy/serie-a-tim");
            matchbookUrlDict.Add("France Ligue 1", "https://www.matchbook.com/events/soccer/france/ligue-1");
            
        }

        public static void ScrapeSelection()
        {
            Console.WriteLine("Select Leagues you want to scrape");
            DisplayLeagueURL();

            do
            {
                Console.WriteLine("Enter selection using , between numbers. To select all type 'all'.");
                userInput = Console.ReadLine();
                validate = userInput == "all" ? true : IsValidFormat(userInput, matchbookUrlDict);
            } while (!validate);

            if (userInput != "all")
            {
                customList = CreateCustomLeagueList(userInput, matchbookUrlDict);
            }
            else if (userInput == "all")
            {
                customList = CreateCustomLeagueList(ReturnEveryLeague(matchbookUrlDict), matchbookUrlDict);
            }
        }


        private static void DisplayLeagueURL()
        {
            int i = 0;

            foreach (KeyValuePair<string, string> league in matchbookUrlDict)
            {
                Console.WriteLine("[{0}] : {1} ", i, league.Key);
                i++;
            }
        }

        public static void ScrapeLeague()
        {
            foreach (var league in customList)
            {

                string strTempValue;

                if (TabRefList.TryGetValue(league.Key, out strTempValue))
                {
                    driver.SwitchTo().Window(strTempValue);
                }
                else
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.open(\'" + league.Value + "\')");
                    driver.SwitchTo().Window(driver.WindowHandles.Last());

                    //Build a list of league names and their window handle IDs
                    TabRefList.Add(league.Key, driver.CurrentWindowHandle);
                }

                Thread.Sleep(2500);
                SetImplicitWait(7);
                GetData();
            }
        }


        private static DateTime GetMatchbookDateTime(string DateTimeXPath)
        {
            string strDateTime;
            string strTime; 
            TimeSpan tGameTime;
            string strDayName;

            DateTime dtEventDateTime;

            strDateTime = AWebElement(DateTimeXPath).Text;
            strTime = strDateTime;
            
            //GetTime //Format: "Wed\r\n01:00"
            strTime = strTime.Substring(strTime.IndexOf(":")-2);
            tGameTime = new TimeSpan(Convert.ToInt32(strTime.Substring(0, 2)), Convert.ToInt32(strTime.Substring(3, 2)), 0);
                        
            if (strDateTime.Contains("Today"))
            //Today
            {
            return Convert.ToDateTime(DateTime.Today + tGameTime).ToUniversalTime();
            }

            else if (strDateTime.Contains("Tomorrow"))
            ////Tomorrow
            {
                return Convert.ToDateTime(DateTime.Today.AddDays(1) + tGameTime).ToUniversalTime();
            }
            else
            ////Get future date from day of week
            {
                //Get the short day name
                strDayName = strDateTime.Substring(0, 3);

                //Get date and add the time on
                dtEventDateTime = GetDateFromDayOfWeek(strDayName) + tGameTime;
                return Convert.ToDateTime(dtEventDateTime).ToUniversalTime();
            }


        }


        public static DateTime GetDateFromDayOfWeek(string strDay)
        {

            int num1 =0;

            switch (strDay)
            {
                case "Sun":
                    num1 = 0;
                    break;
                case "Mon":
                    num1 = 1;
                    break;
                case "Tue":
                    num1 = 2;
                    break;
                case "Wed":
                    num1 = 3;
                    break;
                case "Thu":
                    num1 = 4;
                    break;
                case "Fri":
                    num1 = 5;
                    break;
                case "Sat":
                    num1 = 6;
                    break;
            }
            
            int num2 = (int)DateTime.Today.DayOfWeek;
            return DateTime.Today.AddDays(num1 - num2);
        }
        
        private static void GetData()
        {
          
            List<IWebElement> matches = WebElements("//*[@class='Event-module__participant___1epBK']");

            for (int i = 1; i <= matches.Count; i++)
            {
                string homeTeam = "";
                string homeTeamXpth;
                string awayTeam = "";
                string awayTeamXpth;
                string oddsHome = "";
                string oddsHomeXpth;
                string oddsAway = "";
                string oddsAwayXpth;
                string dateTimeResult = "";
                string dateTimeXpth;
                bool OddsChanged = false;
                string urlXpth;
                string sURL = "";
                

                //Home Team
                homeTeamXpth = "(//*[@class='Event-module__eventNameLink___1hmLY'])[" + i + "]/span/div";
                awayTeamXpth = "(//*[@class='Event-module__eventNameLink___1hmLY'])[" + i + "]/span/div[2]";
                

                oddsHomeXpth = "(//*[contains(@class, 'Event-module__participant___1epBK')])[" + i + "]/div/div/div/div/div[2]/div";
                oddsAwayXpth = "(//*[contains(@class, 'Event-module__participant___1epBK')])[" + i + "]/div/div[3]/div/div/div[2]/div";

                urlXpth = "(//*[@class='Event-module__eventNameLink___1hmLY'])[" + i + "]";

                dateTimeXpth = "(//*[@class='EventTime-module__time___OMllB'])[" + i + "]";

                dateTimeResult = Convert.ToString(GetMatchbookDateTime(dateTimeXpth).ToString("dd'/'MM'/'yyyy HH:mm:ss"));

                if (Convert.ToDateTime(dateTimeResult) <= DateTime.Today.AddDays(3))
             
                {
                    if (AWebElement(homeTeamXpth).Text != null) //Home Team
                    {
                        homeTeam = AWebElement(homeTeamXpth).Text;
                    }
                    if (AWebElement(awayTeamXpth).Text != null) //Away Team
                    {
                        awayTeam = AWebElement(awayTeamXpth).Text;
                        awayTeam = awayTeam.Remove(0, 3);
                    }
                    if (AWebElement(oddsHomeXpth) != null) // Odds -Home
                    {
                        oddsHome = AWebElement(oddsHomeXpth).Text.ToUpper();
                        if (oddsHome == "Make\r\nOffer")
                        {
                            oddsHome = "0";
                        }
                        else
                        {
                            //get the odds from the returned string - "5.7\r\n£78" 
                            oddsHome = oddsHome.Substring(0, oddsHome.IndexOf("\r"));
                        }

                    }
                    if (AWebElement(oddsAwayXpth) != null) // Odds -Away
                    {
                        oddsAway = AWebElement(oddsAwayXpth).Text.ToUpper();
                        if (oddsAway == "Make\r\nOffer")
                        {
                            oddsAway = "0";
                        }
                        else
                        {                            
                            //get the odds from the returned string
                            oddsAway = oddsAway.Substring(0, oddsAway.IndexOf("\r"));
                        }
                    }

                    if (AWebElement(urlXpth).Text != null) //URL
                    {
                        sURL = AWebElement(urlXpth).GetAttribute("href");

                    }


                    //Get univeral team names
                    if (CheckifTeamNameAliasExists(homeTeam) is null)
                    {
                        //Doesn't exist so must be a new alias for an exisiting Team name. Request user input.
                        string strScrapedName = homeTeam;
                        Console.WriteLine(" Enter the universal team name for: " + homeTeam);
                        homeTeam = Convert.ToString(Console.ReadLine());

                        TeamNamesModel TeamNamesModel = new TeamNamesModel
                        {
                            _idAlias = strScrapedName,
                            UniversalTeamName = homeTeam,
                        };
                        db.InsertRecord("TeamNamesLibrary", TeamNamesModel);

                        //Updates the internal list of team names to include any new addtions from previous scrape
                        Program.TeamNamesLibrary = db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");
                    }
                    else
                    {
                        homeTeam = GetUniversalTeamName(homeTeam);
                    }


                    if (CheckifTeamNameAliasExists(awayTeam) is null)
                    {
                        //Doesn't exist so must be a new alias for an exisiting Team name. Request user input.
                        string strScrapedName = awayTeam;
                        Console.WriteLine(" Enter the universal team name for: " + awayTeam);
                        awayTeam = Convert.ToString(Console.ReadLine());

                        TeamNamesModel TeamNamesModel = new TeamNamesModel
                        {
                            _idAlias = strScrapedName,
                            UniversalTeamName = awayTeam,
                        };
                        db.InsertRecord("TeamNamesLibrary", TeamNamesModel);

                        //Updates the internal list of team names to include any new addtions from previous scrape
                        Program.TeamNamesLibrary = db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");
                    }
                    else
                    {
                        awayTeam = GetUniversalTeamName(awayTeam);
                    }
                    
                    Console.WriteLine("HomeTeam: {0} AwayTeam: {1} oddsHome: {2} oddsAway: {3} dateTime = {4}", homeTeam, awayTeam, oddsHome, oddsAway, dateTimeResult);

                    MatchesModel match;

                    //Check if RefTag exists
                    if (db.CountRecordsByRefTag<MatchesModel>("matches", homeTeam.Trim() + " " + awayTeam.Trim() + " " + dateTimeResult) > 0)
                    {
                        match = db.LoadRecordByRefTag<MatchesModel>("matches", homeTeam.Trim() + " " + awayTeam.Trim() + " " + dateTimeResult);

                        //If the scraped Home odds are different to the current odds in DB add the current odds to the oddsrecord object and update the current odds.
                        if (oddsHome != match.MatchbookHomeOdds && oddsHome != "")
                        {
                            OddsRecordModel OddsUpdated = new OddsRecordModel
                            {
                                RefTag = match.RefTag,
                                TeamName = match.HomeTeamName,
                                Odds = oddsHome,
                                DateTimeStamp = DateTime.UtcNow,
                                OddsType = "MatchbookHome"
                            };

                            db.InsertRecord("OddsRecords", OddsUpdated);

                            match.MatchbookHomeOdds = oddsHome;
                            OddsChanged = true;
                        }


                        //If the scraped Away odds are different to the current odds in DB add the current odds to the oddsrecord object and update the current odds.
                        if (oddsAway != match.MatchbookAwayOdds && oddsAway != "")
                        {
                            OddsRecordModel OddsUpdated = new OddsRecordModel
                            {
                                RefTag = match.RefTag,
                                TeamName = match.AwayTeamName,
                                Odds = oddsAway,
                                DateTimeStamp = DateTime.UtcNow,
                                OddsType = "MatchbookAway"
                            };

                            db.InsertRecord("OddsRecords", OddsUpdated);

                            match.MatchbookAwayOdds = oddsAway;
                            OddsChanged = true;

                        }
                        if (OddsChanged == true)
                        {
                            db.UpsertRecordByRefTag("matches", match, match.RefTag);
                        }
                    }
                    else
                    {
                        //New game so Insert a new record 
                        match = new MatchesModel
                        {

                            RefTag = homeTeam.Trim() + " " + awayTeam.Trim() + " " + dateTimeResult,
                            HomeTeamName = homeTeam.Trim(),
                            AwayTeamName = awayTeam.Trim(),
                            StartDateTime = dateTimeResult,

                            MatchbookHomeOdds = oddsHome,
                            MatchbookAwayOdds = oddsAway,

                            URLMatchbook = sURL

                        };
                        db.UpsertRecordByRefTag("matches", match, match.RefTag);
                    }


                    
                }
            }
        }


    }

    

}
