using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Operations;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Scrappy2._0
{
    class Bet365 : RootClass
    {
        public static Dictionary<string, string> leagueCountry = new Dictionary<string, string>();
        public static Dictionary<string, string> customList = new Dictionary<string, string>();
        private static List<string> countries = new List<string>();
        private static int masterCount;
        private static int tmsCount;
        private static int pathCount;
        private static int cycleCount;
        private const int DATERANGE = 5; //days
        public static void LeagueSelection()
        {
            bool validate;
            string userInput;

            Console.WriteLine("Select Leagues you want to scrape");
            DisplayLeagues();

            do
            {
                Console.WriteLine("Enter selection using , between numbers. To select all type 'all'.");
                userInput = Console.ReadLine();
                validate = userInput == "all" ? true : IsValidFormat(userInput);
            } while (!validate);

            if (userInput != "all")
            {
                CreateCustomLeagueList(userInput);
            }
            else if (userInput == "all")
            {
                userInput = "0,1,2,3,4,5,6,7,8,9,10,11,12";
                CreateCustomLeagueList(userInput);
            }

        }

        public static void CreateCustomLeagueList(string input)
        {
            int[] selectedLeagues = Array.ConvertAll(input.Split(','), int.Parse); // convert userInput into an array of numbers.

            foreach (int item in selectedLeagues)
            {
                customList.Add(leagueCountry.ElementAt(item).Key, leagueCountry.ElementAt(item).Value);
            }
        }

        public static void DisplayLeagues()
        {
            int i = 0;

            foreach (KeyValuePair<string, string> league in leagueCountry)
            {
                Console.WriteLine("[{0}] : {1} ", i, league.Key);
                i++;
            }
        }

        public static void Scrape()
        {
            cycleCount = 0;
            //Take first item in list and see if country's class element is open. 
            Dictionary<string, string> cycleList = customList.Count == 0 ? leagueCountry : customList;

            cycleList = cycleList.ShuffleDictionary();

            for (int i = 0; i < cycleList.Count; i++)// KeyValuePair<string , string> league in cycleList) // Change to a for counter, so you can loop back and try again if page is refreshed
            {
                //Check to see if dropdown is present. 
                var item = cycleList.ElementAt(i);

                string divisionTitle = item.Value;
                string leagueTitle = item.Key;
                string leagueXPath = ForLeagueXpath(divisionTitle, leagueTitle); //BuildXpath for Scenario '1'
                IWebElement leagueIWebElement = AWebElement(leagueXPath);

                if (IsAccessibleCheck(divisionTitle, leagueTitle)) //Checks to see if leagues are accessible. 
                {
                    //Access the league.
                    if (leagueIWebElement != null)
                    {
                        ClickLeagueElement(divisionTitle, leagueTitle);   //Method will bring you to list of Matches for specified league.               
                        GetLeagueData(DATERANGE, leagueTitle, divisionTitle);
                    }
                    else
                    {
                        Console.WriteLine("INACTIVE: {1}, {0}", divisionTitle, leagueTitle);
                    }
                }
            }
        }
        public static void ClickLeagueElement(string _divisionTitle, string _leagueTitle)
        {
            // get webElement click, then scrape.
            string leagueXPath = ForLeagueXpath(_divisionTitle, _leagueTitle); //BuildXpath for Scenario '1'
            RandomSleep(3000);
            IWebElement leagueIWebElement;
            try
            {

                leagueIWebElement = AWebElement(leagueXPath);
                if (leagueIWebElement == null)
                {
                    Console.WriteLine("Did not find this, skipping {0} {1}", _divisionTitle, _leagueTitle);
                }
                else
                {
                    leagueIWebElement.Click();
                }

            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Can't find: {0}:{1} IWebElement", _divisionTitle, _leagueTitle);
                throw;
            }
        }

        public static void GetLeagueData(int DATERANGE, string leagueTitle, string divisionTitle)
        {
            {
                string tempDate = "default"; //store date here to write to list later
                int dayRange = DATERANGE; // what maximum range of data you want to grab
                int leagueMatchCount = 0;
                string tempHomeOdds;
                string tempDrawOdds;
                string tempAwayOdds;
                pathCount = 0;

                Console.Write("\nBuilding Directory {0}", leagueTitle);
                RandomSleep(2100);

                //All dates and match titles. 
                List<IWebElement> matchesList = WebElements("//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div");

                try
                {
                    foreach (IWebElement matchWebElement in matchesList)
                    {
                        //Gets IWebElement within specified day range.
                        if (MatchDateRange(matchWebElement) <= dayRange && (matchWebElement.Text.StartsWith("Mon") || matchWebElement.Text.StartsWith("Tue") || matchWebElement.Text.StartsWith("Wed") || matchWebElement.Text.StartsWith("Thu") || matchWebElement.Text.StartsWith("Fri") || matchWebElement.Text.StartsWith("Sat") || matchWebElement.Text.StartsWith("Sun")))
                        {
                            //Console.WriteLine("\n Adding matches for date: {0}", matchWebElement.Text);
                            tempDate = matchWebElement.Text;
                        }
                        //Breaks from looop when it finds date range out of spec
                        else if (MatchDateRange(matchWebElement) > dayRange && (matchWebElement.Text.StartsWith("Mon") || matchWebElement.Text.StartsWith("Tue") || matchWebElement.Text.StartsWith("Wed") || matchWebElement.Text.StartsWith("Thu") || matchWebElement.Text.StartsWith("Fri") || matchWebElement.Text.StartsWith("Sat") || matchWebElement.Text.StartsWith("Sun")))
                        {
                            break;
                        }
                        //Match Time
                        else if (matchWebElement.Text.Contains(":"))
                        {
                            pathCount++;
                            string item = matchWebElement.Text;
                            string[] matchDetails = matchWebElement.Text.Split("\r\n");

                            Console.WriteLine("\n---Date: {3} Time: {0} Home: {1} Away: {2}", matchDetails[0], matchDetails[1], matchDetails[2], tempDate);

                            MatchPath package = new MatchPath(divisionTitle, leagueTitle, matchDetails[0].Trim(), tempDate, matchDetails[1].Trim(), matchDetails[2].Trim());

                            string index = pathCount.ToString();
                            string forOdds = GetOdds(index);
                            List<IWebElement> oddsList = WebElements(forOdds);

                            //DebugPrintOdds(oddsList);

                            tmsCount++;

                            tempHomeOdds = oddsList[pathCount - 1].Text;
                            tempDrawOdds = oddsList[((oddsList.Count / 3) - 1) + pathCount].Text;
                            tempAwayOdds = oddsList[((oddsList.Count / 3 * 2) - 1) + pathCount].Text;

                            Console.WriteLine("Date: {0}\n Country: {1} \n League: {2} \n mTime: {3}\n home:{4}\n away:{5} \nODDS H/D/A: {6}/{7}/{8}", tempDate, divisionTitle, leagueTitle, matchDetails[0].Trim(), matchDetails[1].Trim(), matchDetails[2].Trim(), tempHomeOdds, tempDrawOdds, tempAwayOdds);

                            //WriteToDB(leagueTitle, tempHomeOdds, tempDrawOdds, tempAwayOdds, matchDetails);

                            MatchPath matchItem = package;
                            MatchPath.SaveXpath(matchItem);

                            leagueMatchCount++;
                            masterCount++;
                        }
                    }
                }
                catch (StaleElementReferenceException serex)
                {
                    Console.WriteLine(serex.Message);
                    //GetWebElement again.
                    matchesList = WebElements("//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div");
                }
                catch (Exception e)
                {
                    Console.WriteLine("------> ERROR: {0}", e.Message);
                    throw;
                }
                finally
                {
                    Console.WriteLine("\nTotal Matches found: {0} \n", leagueMatchCount);
                    RandomSleep(2130);
                    driver.Navigate().Back();
                }
            }
        }

        private static void WriteToDB(string leagueTitle, string tempHomeOdds, string tempDrawOdds, string tempAwayOdds, string[] matchDetails)
        {
            // Add the match the DB
            MongoCRUD db = new MongoCRUD("MBEdge");
            //First Check if the match already exists. If it exists retrieve the object. If it doesn't, make a new one.
            if (db.CountRecordsByRefTag<long>("Matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim()) < 1)
            {

                MatchesModel match = new MatchesModel
                {
                    RefTag = matchDetails[1].Trim() + " " + matchDetails[2].Trim(),
                    HomeTeamName = matchDetails[1].Trim(),
                    AwayTeamName = matchDetails[2].Trim(),
                    // B365HomeOdds = tempHomeOdds,
                    B365HomeOdds = tempHomeOdds,
                    B365DrawOdds = tempDrawOdds,
                    B365AwayOdds = tempAwayOdds,
                    //B365BTTSOdds = "1.6",
                    //B365O25GoalsOdds = "2.0",
                    //SmarketsHomeOdds = "6.6",
                    //SmarketsAwayOdds = "2.1",
                    League = leagueTitle,
                    //StartDateTime = new DateTime(2020, 09, 28, 19, 0, 0, DateTimeKind.Utc)
                };
                db.InsertRecord("Matches", match);
            }
            else
            {
                //We need to retrieve the existing document from the DB and update the fields
                var docUpdate = db.LoadRecordByRefTag<MatchesModel>("Matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim());

                docUpdate.B365HomeOdds = tempHomeOdds;
                docUpdate.B365AwayOdds = tempAwayOdds;
                docUpdate.B365DrawOdds = tempDrawOdds;

                db.UpsertRecordByRefTag<MatchesModel>("Matches", docUpdate, matchDetails[1].Trim() + " " + matchDetails[2].Trim());
            }
        }

        private static void DebugPrintOdds(List<IWebElement> oddsList)
        {
            foreach (IWebElement odds in oddsList)
            {
                Console.WriteLine("-----> " + odds.Text);
            }
        }

        public static string GetOdds(string index)
        {
            string start = "//div[@class= 'sgl-MarketOddsExpand gl-Market_General gl-Market_General-columnheader ']/div['";
            string middle = index; //awayT or homeT.
            string end = "']/span";

            string path = start + middle + end;
            return path;
        }
        //Checks to see if headerTitle(United Kingdom...UEFA Champions League etc.. is showing it's leagues. If not expanded, it returns false. If expanded true.
        public static bool IsAccessibleCheck(string _headerTitle, string _leagueTitle)
        {

            string xPath = CheckOpenHeaderXpath(_headerTitle);

            if (AWebElement(xPath) != null)
            {
                return true;
            }
            else // if a headerOpen class is not detected, use the GetHeaderTitleXpath() to expose leagues and return true. 
            {
                xPath = GetHeaderTitleXpath(_headerTitle);
                IWebElement container = AWebElement(xPath);
                bool answer = AWebElement(xPath) != null ? true : false;
                if (container != null)
                {
                    container.Click(); // opens drop down
                    return answer;
                }

                return false;
            }
        }

        public static string ForLeagueXpath(string _divisionTitle, string _leagueTitle)
        {
            string start = "//div[contains(@class, 'gl-MarketGroup_Wrapper sm-SplashMarketGroup_Container ')]//div[contains(@class, 'sm-SplashMarket_Title') and contains(text(), '";
            string mi = _divisionTitle;
            string dd = "')]/parent::div[contains(@class, 'sm-SplashMarket_Header sm-SplashMarket_HeaderOpen ')]/following-sibling::div/child::div/child::span[text() ='";
            string le = _leagueTitle;
            string end = "']";

            string path = start + mi + dd + le + end;

            return path;
        }

        //Returns an xPath for League Division Header Title THAT IS opened
        public static string CheckOpenHeaderXpath(string _headerTitle)
        {
            string start = "//div[contains(@class, 'gl-MarketGroup_Wrapper sm-SplashMarketGroup_Container ')]//div[contains(@class, 'sm-SplashMarket_Title') and contains(text(), '";
            string middle = _headerTitle;
            string end = "')]/parent::div[contains(@class, 'sm-SplashMarket_HeaderOpen ')]";

            string path = start + middle + end;
            return path;
        }

        //Returns an xPath for League Division Header Title THAT IS NOT opened
        public static string GetHeaderTitleXpath(string _headerTitle)
        {
            string start = "//div[contains(@class, 'gl-MarketGroup_Wrapper sm-SplashMarketGroup_Container ')]//div[contains(@class, 'sm-SplashMarket_Title') and contains(text(), '";
            string middle = _headerTitle;
            string end = "')]";

            string path = start + middle + end;
            return path;
        }
        private static double MatchDateRange(IWebElement matchWebElement) //Compare match dates with current date UTC.
        {
            DateTime currentTime = DateTime.UtcNow;  //TODO verify correct time conversion is carried out for different parts of the world. What time is Bet365 showing me in Thailand vs. UK?
            string dayInt = matchWebElement.Text.Substring(4, 2);
            string month = matchWebElement.Text.Substring(7, 3);
            string date = month + "" + dayInt + ", 2020";

            try
            {
                DateTime dateconv = DateTime.Parse(date);
                double days = (dateconv - currentTime).TotalDays;
                return days;
            }
            catch (FormatException)
            {
                return 99; //lots of formatExceptions...maybe re-code to avoid this?
            }
        }
        public static void InitiateList()
        {
            leagueCountry.Add("UEFA Champions League", "UEFA Competitions");
            leagueCountry.Add("UEFA Europa League", "UEFA Competitions");
            leagueCountry.Add("Scotland Premiership", "United Kingdom");
            leagueCountry.Add("England Premier League", "United Kingdom");
            leagueCountry.Add("England Championship", "United Kingdom");
            leagueCountry.Add("England League 1", "United Kingdom");
            leagueCountry.Add("England League 2", "United Kingdom");
            leagueCountry.Add("England FA Cup", "United Kingdom");
            //leagueCountry.Add("England EFL Cup", "United Kingdom");
            leagueCountry.Add("Scottish Premiership", "United Kingdom");
            //leagueCountry.Add("Spanish Primera Liga", "Spain");
            leagueCountry.Add("Spain Primera Liga", "Spain");
            leagueCountry.Add("Germany Bundesliga I", "Germany");
            leagueCountry.Add("Italy Serie A", "Italy");
            leagueCountry.Add("France Ligue 1", "France");
            //leagueCountry.Add("AFC Champions League", "Relevant?");
            //leagueCountry.Add("Copa Sudamericana", "Relevant?");

            //Countries for initial startup
            countries.Add("Full Time Result – Enhanced Prices");
            countries.Add("United Kingdom");
            countries.Add("Spain");
            countries.Add("UEFA Competitions");
            countries.Add("France");
            countries.Add("Italy");
            countries.Add("Germany");
        }

        public static void DisplaySummary()
        {
            Console.WriteLine("////////////////////////////////////////////////////////////////////////////////////////////////////\n\nLeague List Cycle: {0} \n Total Matches Scraped = {1}\nTotal Time Duration: {2} minutes \n\n////////////////////////////////////////////////////////////////////////////////////////////////////", masterCount, tmsCount, Math.Round((DateTime.UtcNow - timeZero).TotalMinutes));

        }
    }
}
