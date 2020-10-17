﻿using System;
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
    class Bet365 : XpathDirectory
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
            string bothToScoreSelected;

            do
            {
                Console.WriteLine("Scrape BTTS, O2.5? (Y/N)");
                string optionSelected = Console.ReadLine();
                bothToScoreSelected = optionSelected.ToLower();
                validate = bothToScoreSelected == "y" ? true : bothToScoreSelected == "n" ? true : ThrowMessage();
                Program.bttsEnabled = bothToScoreSelected == "y" ? true : false;              
            } while (!validate);

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
                userInput = "0,1,2,3,4,5,6,7,8,9,10,11,12"; //make this code adaptable
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
                bool leagueExists = AccessLeagueElement(divisionTitle, leagueTitle); // Finds an IWebElement for a specific league, then clicks on that element to expose all matches. 
                
                if(leagueExists == true)
                {
                    GetLeagueData(DATERANGE, leagueTitle, divisionTitle); 
                } else {
                    Console.WriteLine("INACTIVE: {1}, {0}", divisionTitle, leagueTitle);
                }
            }
        }

        private static bool AccessLeagueElement(string divisionTitle, string leagueTitle)
        {
            string leagueXPath = GetLeagueXpath(divisionTitle, leagueTitle);                //Acquire Xpath to get specific leagues.
            IWebElement leagueIWebElement = AWebElement(leagueXPath);

            if (IsAccessibleCheck(divisionTitle, leagueTitle))                              //Checks to see if leagues are accessible. HeadOpen class activated or not? If not, should open it.
            {
                //Access the league.
                if (leagueIWebElement != null)
                {
                    ClickLeagueElement(divisionTitle, leagueTitle);     //Method will bring you to list of Matches for specified league.   
                    return true;
                } else {
                    return false;
                }               
            }
            return false;
        }

        public static void ClickLeagueElement(string _divisionTitle, string _leagueTitle)
        {
            
            string leagueXPath = GetLeagueXpath(_divisionTitle, _leagueTitle); //BuildXpath for Scenario '1'
            RandomSleep(2500);
            IWebElement leagueIWebElement;
            try
            {
                leagueIWebElement = AWebElement(leagueXPath);
                    if (leagueIWebElement == null)
                    {
                        Console.WriteLine("League xPath not found. Skipping {0} {1}", _divisionTitle, _leagueTitle);
                    } else {
                        leagueIWebElement.Click();
                    }
            }
            catch (NullReferenceException)
            {
                Debug.Print("ClickLeagueElement couldn't find: {0}:{1} IWebElement", _divisionTitle, _leagueTitle);
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
                            string forOdds = GetOddsXpath(index);
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
                    Debug.Print("------> Stale Element {0}",serex.Message);
                    //GetWebElement again.
                    matchesList = WebElements("//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div");
                }
                catch (Exception e)
                {
                    Debug.Print("------> Error: {0}", e.Message);
                    throw;
                }
                finally
                {
                    Console.WriteLine("\n Total Matches found: {0} \n", leagueMatchCount);
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
                    B365HomeOdds = Convert.ToDouble(tempHomeOdds),
                    B365DrawOdds = Convert.ToDouble(tempDrawOdds),
                    B365AwayOdds = Convert.ToDouble(tempAwayOdds),
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

                docUpdate.B365HomeOdds = Convert.ToDouble(tempHomeOdds);
                docUpdate.B365AwayOdds = Convert.ToDouble(tempAwayOdds);
                docUpdate.B365DrawOdds = Convert.ToDouble(tempDrawOdds);

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
        public static void CollectData()
        {
            List<MatchPath> directoryList = new List<MatchPath>();
            List<MatchPath> clonedDirectory = new List<MatchPath>(); //Initialize a new list
            
            directoryList = MatchPath.GetXpathList();
            clonedDirectory = directoryList; //create a copy.

            Console.WriteLine("\n Collecting Data for {0} matches...", directoryList.Count());

            //Create an inefficient way to travel to these pages. Humanizes the URL navigation
            Shuffle(clonedDirectory);

            //Print(clonedDirectory);
            for (int i = 0; i < clonedDirectory.Count(); i++)
            {
                RandomSleep(4312);
                //Enter into first league.
                string divisionTitle = clonedDirectory[i].country;
                string leagueTitle = clonedDirectory[i].league;
                string leagueXPath = GetLeagueXpath(divisionTitle, leagueTitle); //BuildXpath for Scenario '1'

                AccessLeagueElement(divisionTitle, leagueTitle); //Brings you to current list of matches for specified league.
                //Access specific match detail, then bring back to the root page. 

                RandomSleep(2312);
                //Find Match

                string xPathHome = GetMatchXpath(clonedDirectory[i].homeT); // get xPath for homeT
                string xPathAway = GetMatchXpath(clonedDirectory[i].awayT); ; // get xPath for awayT
                //string xPathDate = BuildPath(clonedDirectory[i].homeT, clonedDirectory[i].date, 4); //get xPath to date

                IWebElement matchDetails = AWebElement(xPathHome);
                string webHome = matchDetails.Text.Trim();
                string webAway = AWebElement(xPathAway).Text.Trim();
                //string webDate = AWebElement(xPathDate).Text.Trim();


                if (webHome == clonedDirectory[i].homeT && clonedDirectory[i].awayT == webAway)
                {
                    Console.WriteLine("\nEntered a match!");
                    matchDetails.Click();
                    RandomSleep(5121);
                    GrabData(webHome, webAway);
                }


            }
        }

        private static void GrabData(string HomeTeamName, string AwayTeamName)
        {
            List<IWebElement> elements = WebElements("//div[@class = 'gl-MarketGroupButton_Text ' and contains(text(),'Full Time Result')]/parent::div/following-sibling::div/child::div/child::div/div/span[@class= 'gl-Participant_Odds']");

            Double homeOddsPath = Convert.ToDouble(elements[0].Text);
            Double drawOddsPath = Convert.ToDouble(elements[1].Text);
            Double awayOddsPath = Convert.ToDouble(elements[2].Text);

            List<IWebElement> odds = WebElements("//div[@class = 'gl-MarketGroupButton_Text ' and contains(text(),'Goals Over/Under')]/parent::div/following-sibling::div/div/div/div/span[@class='gl-ParticipantOddsOnly_Odds']");
            RandomSleep(2121);
            Double overTwoFivePath = Convert.ToDouble(odds[0].Text);
            Double btsYesPath = Convert.ToDouble(AWebElement("//div[@class = 'gl-MarketGroupButton_Text ' and contains(text(),'Both Teams to Score')]/parent::div/following-sibling::div/descendant::span[@class= 'gl-ParticipantBorderless_Odds'][1]").Text);


            // Add the match the DB
            MongoCRUD db = new MongoCRUD("MBEdge");
            //First Check if the match already exists. If it exists retrieve the object. If it doesn't, make a new one.

            if (db.CountRecordsByRefTag<long>("Matches", HomeTeamName + " " + AwayTeamName) < 1)
            {

                MatchesModel match = new MatchesModel
                {
                    RefTag = HomeTeamName + " " + AwayTeamName,
                    HomeTeamName = HomeTeamName,
                    AwayTeamName = AwayTeamName,
                    // B365HomeOdds = tempHomeOdds,
                    B365HomeOdds = homeOddsPath,
                    B365DrawOdds = drawOddsPath,
                    B365AwayOdds = awayOddsPath,
                    B365BTTSOdds = btsYesPath,
                    B365O25GoalsOdds = overTwoFivePath,
                    //SmarketsHomeOdds = "6.6",
                    //SmarketsAwayOdds = "2.1",
                    //League = league,
                    //StartDateTime = new DateTime(2020, 09, 28, 19, 0, 0, DateTimeKind.Utc)

                };

                //Get Occurrence of 2up based on game odds
                MatchOccurence.GetOccurrences(match);

                db.InsertRecord("Matches", match);

            }
            else
            {
                //We need to retrieve the existing document from the DB and update the fields
                var docUpdate = db.LoadRecordByRefTag<MatchesModel>("Matches", HomeTeamName + " " + AwayTeamName);
                //

                docUpdate.B365HomeOdds = homeOddsPath;
                docUpdate.B365AwayOdds = awayOddsPath;
                docUpdate.B365DrawOdds = drawOddsPath;
                docUpdate.B365BTTSOdds = btsYesPath;
                docUpdate.B365O25GoalsOdds = overTwoFivePath;

                db.UpsertRecordByRefTag<MatchesModel>("Matches", docUpdate, HomeTeamName + " " + AwayTeamName);

                //
            }


            Console.WriteLine("Over2.5: {0} BTS(yes): {1} Home: {2} Draw: {3} Away: {4} ", overTwoFivePath, btsYesPath, homeOddsPath, drawOddsPath, awayOddsPath);
            RandomSleep(3130);
            IWebElement button = AWebElement("//div[@class= 'sph-BreadcrumbTrail_Breadcrumb ']");
            button.Click();
            //GO BACK TO ROOT URL
            RandomSleep(2130);
            IWebElement button2 = AWebElement("//div[@class= 'sph-BreadcrumbTrail_Breadcrumb ']");
            button2.Click();

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