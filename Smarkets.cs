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
        private static int dateRange = 6; //days
        public static int pathCount = 0; //Checksum comparer.
        private static bool validate;
        private static string userInput;

        public static void InitiateList()
        {
            smarketsUrlDict.Add("UEFA Champions League", "https://smarkets.com/listing/sport/football/uefa-champions-league-qualification");
            smarketsUrlDict.Add("UEFA Europa League", "https://smarkets.com/listing/sport/football/uefa-europa-league-qualification");
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
                validate = userInput == "all" ? true : IsValidFormat(userInput);
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

                //ul[contains(@class,'event-list')]/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][i]/div[@class='contract-items  open ']

                // Tick-Buy home oddsXpath
                //ul[contains(@class,'event-list')]/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][i]/div[@class='contract-items  open ']/span[contains(@class, 'contract-item')][1]/div[@class='current-price']/span[@class ='offer']/span[1]
                // Tick-Buy away oddsXpath
                //ul[contains(@class,'event-list')]/li[contains(@class, 'item-tile event-tile  upcoming layout-row')][i]/div[@class='contract-items  open ']/span[contains(@class, 'contract-item')][3]/div[@class='current-price']/span[@class ='offer']/span[1]

                //Home Team
                homeTeamXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   ']/descendant::span[@class = 'team-name'][" + (i * 2 - 1) + "]";
                awayTeamXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   ']/descendant::span[@class = 'team-name'][" + (i * 2) + "]";
                oddsHomeXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   '][" + i + "]/div[@class='contract-items  open ']/span[contains(@class, 'contract-item')][1]/div[@class='current-price']/span[@class ='offer']/span[1]";
                oddsAwayXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   '][" + i + "]/div[@class='contract-items  open ']/span[contains(@class, 'contract-item')][3]/div[@class='current-price']/span[@class ='offer']/span[1]";
                dateTimeXpth = "//ul[@class='event-list list-view  football']/li[@class='item-tile event-tile  upcoming layout-row   '][" + i + "]//div[@class ='event-date']/time";
                Console.WriteLine("Hitting If....");

                if (IsWithinDays(dateTimeXpth) <= 1000) //For Testing Purposes
                {
                    Console.WriteLine("Hello Ryan!");
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
                        dateTimeResult = AWebElement(dateTimeXpth).GetAttribute("datetime");
                    }

                    Console.WriteLine("HomeTeam: {0} AwayTeam: {1} oddsHome: {2} oddsAway: {3} dateTime = {4}", homeTeam, awayTeam, oddsHome, oddsAway, dateTimeResult);

                    break;
                    //Write to Database
                    MongoCRUD db = new MongoCRUD("MBEdge");

                    //First Check if the match already exists. If it exists retrieve the object. If it doesn't, make a new one.
                    if (db.CountRecordsByRefTag<long>("Matches", homeTeam.Trim() + " " + awayTeam.Trim()) < 1)
                    {

                        MatchesModel match = new MatchesModel
                        {
                            RefTag = homeTeam.Trim() + " " + awayTeam.Trim(),
                            HomeTeamName = homeTeam.Trim(),
                            AwayTeamName = awayTeam.Trim(),

                            SmarketsHomeOdds = oddsHome,
                            SmarketsAwayOdds = oddsAway
                        };

                        db.InsertRecord("Matches", match);
                    }
                    else
                    {
                        //We need to retrieve the existing document from the DB and update the fields
                        var docUpdate = db.LoadRecordByRefTag<MatchesModel>("Matches", homeTeam.Trim() + " " + awayTeam.Trim());

                        docUpdate.SmarketsHomeOdds = oddsHome;
                        docUpdate.SmarketsAwayOdds = oddsAway;

                        db.UpsertRecordByRefTag<MatchesModel>("Matches", docUpdate, homeTeam.Trim() + " " + awayTeam.Trim());
                    }
                }
            }
        }

        private static double IsWithinDays(string xPath)
        {
            //yyyy - MM - ddTHH:mm: ssZ (how SMarkets date is formatted)
            string matchDate = AWebElement(xPath).GetAttribute("datetime");
            string format = "yyyy-MM-ddTHH:mm:ssZ";

            DateTime matchDateConv = DateTime.ParseExact(matchDate, format, System.Globalization.CultureInfo.InvariantCulture); //MatchDate by default comes as UTC format.

            double timeDiff = Math.Round((matchDateConv - DateTime.UtcNow).TotalHours, 2); //Total time difference in hours

            return timeDiff;
        }
    }
}
