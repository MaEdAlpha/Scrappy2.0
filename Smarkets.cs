﻿using System;
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
    class Smarkets : RootClass
    {
        private static Dictionary<string, string> smarketsUrlDict = new Dictionary<string, string>();
        private static Dictionary<string, string> customList = new Dictionary<string, string>();
        private static Dictionary<string, string> TabRefList = new Dictionary<string, string>();
        public static int pathCount = 0; //Checksum comparer.
        private static bool validate;
        private static string userInput;
   

        public static void InitiateList()
        {
            smarketsUrlDict.Add("UEFA Champions League", "https://smarkets.com/listing/sport/football/uefa-champions-league");
            smarketsUrlDict.Add("UEFA Europa League", "https://smarkets.com/listing/sport/football/uefa-europa-league");
            smarketsUrlDict.Add("Scotland Premiership", "https://smarkets.com/listing/sport/football/scotland-premiership");
            smarketsUrlDict.Add("England Premier League", "https://smarkets.com/listing/sport/football/england-premier-league");
            smarketsUrlDict.Add("England Championship", "https://smarkets.com/listing/sport/football/england-championship");
            smarketsUrlDict.Add("England League 1", "https://smarkets.com/listing/sport/football/england-league-1");
            smarketsUrlDict.Add("England League 2", "https://smarkets.com/listing/sport/football/england-league-2");
            //smarketsUrlDict.Add("England FA Cup", "United Kingdom");
            smarketsUrlDict.Add("England EFL Cup", "https://smarkets.com/listing/sport/football/england-league-cup");
            //smarketsUrlDict.Add("Scottish Premiership", "United Kingdom");
            smarketsUrlDict.Add("Spanish Primera Liga", "https://smarkets.com/listing/sport/football/spain-la-liga");
            //smarketsUrlDict.Add("Spain Primera Liga", "Spain");
            smarketsUrlDict.Add("Germany Bundesliga I", "https://smarkets.com/listing/sport/football/germany-bundesliga");
            smarketsUrlDict.Add("Italy Serie A", "https://smarkets.com/listing/sport/football/italy-serie-a");
            smarketsUrlDict.Add("France Ligue 1", "https://smarkets.com/listing/sport/football/france-ligue-1");
            //smarketsUrlDict.Add("AFC Champions League", "Relevant?");
            //smarketsUrlDict.Add("Copa Sudamericana", "Relevant?");
        }

        public static void ScrapeSelection()
        {
            Console.WriteLine("Select Leagues you want to scrape");
            DisplayLeagueURL();

            do
            {
                Console.WriteLine("Enter selection using , between numbers. To select all type 'all'.");
                userInput = Console.ReadLine();
                validate = userInput == "all" ? true : IsValidFormat(userInput, smarketsUrlDict);
            } while (!validate);

            if (userInput != "all")
            {
                customList = CreateCustomLeagueList(userInput, smarketsUrlDict);
            }
            else if (userInput == "all")
            {
                customList = CreateCustomLeagueList( ReturnEveryLeague(smarketsUrlDict), smarketsUrlDict);
            }
        }



        private static void DisplayLeagueURL()
        {
            int i = 0;

            foreach (KeyValuePair<string, string> league in smarketsUrlDict)
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
        private static void GetData()
        {
            bool OddsLoading = true;

            List<IWebElement> matches = WebElements("//ul[contains(@class,'event-list list-view  football')]//li[contains(@class,'item-tile event-tile  upcoming layout-row ')]");

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
                do
                {
                    try
                    {
                        //Loop until the 1st element is not null and isn't an empty string
                        if (AWebElement("//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items')]/span[contains(@class, 'contract-item')][1]/div[contains(@class, 'current-price')]/span[contains(@class,'bid')]/span[1]").Text != null)
                        {
                            if (AWebElement("//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items')]/span[contains(@class, 'contract-item')][1]/div[contains(@class, 'current-price')]/span[contains(@class,'bid')]/span[1]").Text != " ")
                            {
                                OddsLoading = false;
                            }
                            else
                            {
                                //Check to see if the reason it is empty is because no liquidity. In this case the HTML has "empty" in a div tag
                                if (AWebElement("//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items')]/span[contains(@class, 'contract-item')][1]/div[contains(@class, 'current-price')]/span[contains(@class,'bid')]/div[contains(@class, 'empty')]") != null)
                                {
                                    Console.Write("Odds are empty for ");
                                    OddsLoading = false;
                                }
                            }
                        }
                    }
                    catch(StaleElementReferenceException)
                    {
                        Console.Write("--------------STALE ELEMENTO");
                    }
                } while (OddsLoading);

                //Home Team
                homeTeamXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'event-info-container')]/a[contains(@class, 'title  with-score')]/div[contains(@class, 'teams')]/div[contains(@class, 'team')][1]";
                awayTeamXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'event-info-container')]/a[contains(@class, 'title  with-score')]/div[contains(@class, 'teams')]/div[contains(@class, 'team')][2]";
                oddsHomeXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items')]/span[contains(@class, 'contract-item')][1]/div[contains(@class, 'current-price')]/span[contains(@class,'bid')]/span[1]";
                oddsAwayXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items')]/span[contains(@class, 'contract-item')][3]/div[contains(@class, 'current-price')]/span[contains(@class, 'bid')]/span[1]";
                dateTimeXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   '][" + i + "]//div[@class ='event-date']/time";
                urlXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'event-info-container')]/a[contains(@class, 'title  with-score')]";
                //LeagueXpth = league

                if (IsWithinDays(dateTimeXpth) <= 72) // if Xpath DateTime is less than specified time (hours)
                {
                        if (AWebElement(homeTeamXpth).Text != null) //Home Team
                        {
                            homeTeam = AWebElement(homeTeamXpth).Text;
                        }
                        if (AWebElement(awayTeamXpth).Text != null) //Away Team
                        {
                            awayTeam = AWebElement(awayTeamXpth).Text;
                        }
                        if (AWebElement(oddsHomeXpth) != null) // Odds -Home
                        {
                            oddsHome = AWebElement(oddsHomeXpth).Text.ToUpper();
                            if (oddsHome == "ASK" || oddsHome == " ")
                            {
                                oddsHome = "0";
                            }
                        }                    
                        if (AWebElement(oddsAwayXpth) != null) // Odds -Away
                        {
                            oddsAway = AWebElement(oddsAwayXpth).Text.ToUpper();
                            if (oddsAway == "ASK" || oddsAway == " ")
                            {
                                oddsAway = "0";
                            }
                        }
                        if (AWebElement(dateTimeXpth).GetAttribute("datetime") != null)
                        {
                            dateTimeResult = AWebElement(dateTimeXpth).GetAttribute("datetime").Trim('Z').Replace("T", " ");                
                            DateTime date = DateTime.Parse(dateTimeResult);
                            dateTimeResult = date.ToString("dd'/'MM'/'yyyy HH:mm:ss");
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

                        //Make sure the URL is set
                        match.URLSmarkets = sURL;

                            //If the scraped Home odds are different to the current odds in DB add the current odds to the oddsrecord object and update the current odds.
                            if (oddsHome != match.SmarketsHomeOdds && oddsHome != "")
                            {
                                OddsRecordModel OddsUpdated = new OddsRecordModel
                                {
                                    RefTag = match.RefTag,
                                    TeamName = match.HomeTeamName,
                                    Odds = oddsHome,
                                    DateTimeStamp = DateTime.UtcNow,
                                    OddsType = "SmarketsHome"
                                };

                                db.InsertRecord("OddsRecords", OddsUpdated);
                         
                                match.SmarketsHomeOdds = oddsHome;
                                OddsChanged = true;
                            }
                       

                            //If the scraped Away odds are different to the current odds in DB add the current odds to the oddsrecord object and update the current odds.
                            if (oddsAway != match.SmarketsAwayOdds && oddsAway != "")
                            {
                                OddsRecordModel OddsUpdated = new OddsRecordModel
                                {
                                    RefTag = match.RefTag,
                                    TeamName = match.AwayTeamName,
                                    Odds = oddsAway,
                                    DateTimeStamp = DateTime.UtcNow,
                                    OddsType = "SmarketsAway"
                                };

                                db.InsertRecord("OddsRecords", OddsUpdated);

                                match.SmarketsAwayOdds = oddsAway;
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

                                SmarketsHomeOdds = oddsHome,
                                SmarketsAwayOdds = oddsAway,

                                URLSmarkets = sURL
                                
                            };
                            db.UpsertRecordByRefTag("matches", match, match.RefTag);
                        }

                }
            }
        }


        private static double IsWithinDays(string xPath)
        {
          
            string matchDate = AWebElement(xPath).GetAttribute("datetime");
            string format = "yyyy-MM-ddTHH:mm:ssZ";

            DateTime matchDateConv = DateTime.ParseExact(matchDate, format, System.Globalization.CultureInfo.InvariantCulture); //MatchDate by default comes as UTC format.

            double timeDiff = Math.Round((matchDateConv - DateTime.UtcNow).TotalHours, 2); //Total time difference in hours

            return timeDiff;
        }
    }
}
