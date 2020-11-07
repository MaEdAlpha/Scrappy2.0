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
    class Smarkets : RootClass
    {
        private static Dictionary<string, string> smarketsUrlDict = new Dictionary<string, string>();
        private static Dictionary<string, string> customList = new Dictionary<string, string>();
        private static int dateRange = 3; //days
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
                driver.Navigate().GoToUrl(league.Value);
                Thread.Sleep(2500);
                GetData();
            }
        }
        private static void GetData()
        {
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
                //string LeagueXpth;

                //ul[contains(@class,'event-list')]/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][i]/div[@class='contract-items  open ']

                // Tick-Buy home oddsXpath
                //ul[contains(@class,'event-list')]/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][i]/div[@class='contract-items  open ']/span[contains(@class, 'contract-item')][1]/div[@class='current-price']/span[@class ='offer']/span[1]
                // Tick-Buy away oddsXpath
                //ul[contains(@class,'event-list')]/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][i]/div[@class='contract-items  open ']/span[contains(@class, 'contract-item')][3]/div[@class='current-price']/span[@class ='offer']/span[1]

                //Home Team
                homeTeamXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'event-info-container')]/a[contains(@class, 'title  with-score')]/div[contains(@class, 'teams')]/div[contains(@class, 'team')][1]";
                awayTeamXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'event-info-container')]/a[contains(@class, 'title  with-score')]/div[contains(@class, 'teams')]/div[contains(@class, 'team')][2]";
                oddsHomeXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items  open ')]/span[contains(@class, 'contract-item')][1]/div[contains(@class, 'current-price')]/span[contains(@class,'bid')]/span[1]";
                oddsAwayXpth = "//ul[@class='event-list list-view  football']/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][" + i + "]/div[contains(@class, 'contract-items  open ')]/span[contains(@class, 'contract-item')][3]/div[contains(@class, 'current-price')]/span[contains(@class, 'bid')]/span[1]";
                dateTimeXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   '][" + i + "]//div[@class ='event-date']/time";
                //LeagueXpth = league



                //if (IsWithinDays(dateTimeXpth) <= 14) //For Testing Purposes
                //{
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
                        oddsHome = AWebElement(oddsHomeXpth).Text;
                    }
                    if (AWebElement(oddsAwayXpth) != null) // Odds -Away
                    {
                        oddsAway = AWebElement(oddsAwayXpth).Text;
                    }
                    if (AWebElement(dateTimeXpth).GetAttribute("datetime") != null)
                    {
                        dateTimeResult = AWebElement(dateTimeXpth).GetAttribute("datetime").Trim('Z').Replace("T", " ");                
                        DateTime date = DateTime.Parse(dateTimeResult);
                        dateTimeResult = date.ToString("dd/MM/yyyy HH:mm:ss");
                        //10 - 25 - 2020 15:00:00 Smarkets
                        //10 - 24 - 2020 21:00:00

                    }

                    Console.WriteLine("HomeTeam: {0} AwayTeam: {1} oddsHome: {2} oddsAway: {3} dateTime = {4}", homeTeam, awayTeam, oddsHome, oddsAway, dateTimeResult);


                    ////////////////////////// Add the match the DB
                    
                    MongoCRUD db = new MongoCRUD("MBEdge");
                    MatchesModel match;

                    //Check if RefTag exists
                    if (db.CountRecordsByRefTag<MatchesModel>("matches", homeTeam.Trim() + " " + awayTeam.Trim() + " " + dateTimeResult) > 0)
                    {
                        match = db.LoadRecordByRefTag<MatchesModel>("matches", homeTeam.Trim() + " " + awayTeam.Trim() + " " + dateTimeResult);
                        match.SmarketsHomeOdds = oddsHome;
                        match.SmarketsAwayOdds = oddsAway;
                    }
                    else
                    {
                        match = new MatchesModel
                        {

                            RefTag = homeTeam.Trim() + " " + awayTeam.Trim() + " " + dateTimeResult,
                            HomeTeamName = homeTeam.Trim(),
                            AwayTeamName = awayTeam.Trim(),
                            StartDateTime = dateTimeResult,

                            SmarketsHomeOdds = oddsHome,
                            SmarketsAwayOdds = oddsAway

                           // League = leagueTitle,

                        };
                    }

                    db.UpsertRecordByRefTag("matches", match, match.RefTag);
                
                

                    //Check to see if Team names are already in list. if not add them in
                    
                    if (GetUniversalTeamName(homeTeam) is null)
                    {
                        TeamNamesModel TeamNamesModel = new TeamNamesModel
                        {
                            _idAlias = match.HomeTeamName,
                            UniversalTeamName = match.HomeTeamName,
                        };
                        db.InsertRecord("TeamNamesLibrary", TeamNamesModel);
                        
                        //Updates the internal list of team names to include any new addtions from previous scrape
                        Program.TeamNamesLibrary = db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");
                    }
                    

                    if (GetUniversalTeamName(awayTeam) is null)
                    {
                        TeamNamesModel TeamNamesModel = new TeamNamesModel
                        {
                            _idAlias = match.AwayTeamName,
                            UniversalTeamName = match.AwayTeamName,
                        };
                        db.InsertRecord("TeamNamesLibrary", TeamNamesModel);

                        //Updates the internal list of team names to include any new addtions from previous scrape
                        Program.TeamNamesLibrary = db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");
                    }
                }

            //} DATE Time Range 
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
