using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Operations;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace Scrappy2._0
{
    class Bet365 : XpathDirectory
    {
        public static Dictionary<string, string> leagueDivisionDict = new Dictionary<string, string>();
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
                validate = userInput == "all" ? true : IsValidFormat(userInput, leagueDivisionDict);
            } while (!validate);

            if (userInput != "all")
            {
               customList = CreateCustomLeagueList(userInput, leagueDivisionDict);
            }
            else if (userInput == "all")
            {
                customList = CreateCustomLeagueList( ReturnEveryLeague(leagueDivisionDict), leagueDivisionDict);
            }
        }

        public static void DisplayLeagues()
        {
            int i = 0;

            foreach (KeyValuePair<string, string> league in leagueDivisionDict)
            {
                Console.WriteLine("[{0}] : {1} ", i, league.Key);
                i++;
            }
        }

        public static void Scrape()
        {
            cycleCount = 0;
            //Take first item in list and see if country's class element is open. 
            Dictionary<string, string> cycleList = customList.Count == 0 ? leagueDivisionDict : customList;

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
                string tempDate = ""; //store date here to write to list later
                string finalDate = "";
                int dayRange = DATERANGE; // what maximum range of data you want to grab
                int leagueMatchCount = 0;
                string tempHomeOdds;
                string tempDrawOdds;
                string tempAwayOdds;
                string matchTime;
                pathCount = 0;

                Console.Write("\n Building Directory {0}", leagueTitle);
                RandomSleep(1500);
                //All dates and match titles. 
                string forAllMatches = "//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div";
                List<IWebElement> matchesList = WebElements(forAllMatches);

                try
                {
                    foreach (IWebElement matchWebElement in matchesList)
                    {

                        //Gets IWebElement within specified day range.
                        if (MatchDateRange(matchWebElement) <= dayRange && (matchWebElement.Text.StartsWith("Mon") || matchWebElement.Text.StartsWith("Tue") || matchWebElement.Text.StartsWith("Wed") || matchWebElement.Text.StartsWith("Thu") || matchWebElement.Text.StartsWith("Fri") || matchWebElement.Text.StartsWith("Sat") || matchWebElement.Text.StartsWith("Sun")))
                        {

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

                            if (ScrapeThisMatch(pathCount))
                            {
                                string item = matchWebElement.Text;
                                string[] matchDetails = matchWebElement.Text.Split("\r\n");

                                finalDate = ConverToDateTime(tempDate, matchDetails[0]).ToString("MM/dd/yyyy HH:mm:ss");
                                matchTime = ConverToDateTime(tempDate, matchDetails[0]).ToString("HH:mm");


                                Console.WriteLine("\n---Date: {3} Time: {0} Home: {1} Away: {2}", matchTime, matchDetails[1], matchDetails[2], finalDate);

                                MatchPath package = new MatchPath(divisionTitle, leagueTitle, matchTime, finalDate, matchDetails[1].Trim(), matchDetails[2].Trim());

                                string index = pathCount.ToString();
                                string forOdds = GetOddsXpath(index);
                                List<IWebElement> oddsList = WebElements(forOdds);

                                //DebugPrintOdds(oddsList);

                                tmsCount++;

                                tempHomeOdds = oddsList[pathCount - 1].Text;
                                tempDrawOdds = oddsList[((oddsList.Count / 3) - 1) + pathCount].Text;
                                tempAwayOdds = oddsList[((oddsList.Count / 3 * 2) - 1) + pathCount].Text;

                                Console.WriteLine("Date: {0}\n Country: {1} \n League: {2} \n mTime: {3}\n home:{4}\n away:{5} \nODDS H/D/A: {6}/{7}/{8}", finalDate, divisionTitle, leagueTitle, matchTime, matchDetails[1].Trim(), matchDetails[2].Trim(), tempHomeOdds, tempDrawOdds, tempAwayOdds);

                                //WriteToDB(leagueTitle, tempHomeOdds, tempDrawOdds, tempAwayOdds, matchDetails, finalDate);

                                MatchPath matchItem = package;
                                MatchPath.SaveXpath(matchItem);

                                leagueMatchCount++;
                                masterCount++;
                            }
                        }
                    }
                }
                catch (StaleElementReferenceException serex)
                {
                    Debug.Print("------> Stale Element {0}",serex.Message);
                    //GetWebElement again.
                    matchesList = WebElements(forAllMatches);
                }
                catch (Exception e)
                {
                    Debug.Print("------> Error: {0}", e.Message);
                    throw;
                }
                finally
                {
                    Console.WriteLine("\n Total Matches found: {0} \n", leagueMatchCount);
                    RandomSleep(1930);
                    driver.Navigate().Back();
                }
            }
        }

        private static void WriteToDB(string leagueTitle, string tempHomeOdds, string tempDrawOdds, string tempAwayOdds, string[] matchDetails, string finalDate)
        {
            // Add the match the DB
            MongoCRUD db = new MongoCRUD("MBEdge");
            //First Check if the match already exists. If it exists retrieve the object. If it doesn't, make a new one.
            if (db.CountRecordsByRefTag<long>("matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim()) < 1)
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
                    StartDateTime = finalDate
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
        private static DateTime ConverToDateTime(string dateString, string hoursMinutes)
        {
            string date;
            string dayInt = dateString.Substring(4, 2);
            string month = dateString.Substring(7, 3);
            DateTime dateconv = DateTime.Now;

            if (month != "Jan")
            {
                date = "2020 " + month + dayInt + " " + hoursMinutes.Trim() + ":00";
            } else
            {
                date = "2021 " + month + dayInt + " " + hoursMinutes.Trim() + ":00";
            }
            try
            {
                dateconv = DateTime.Parse(date).ToUniversalTime();
                return dateconv;
            }
            catch (Exception)
            {
                Debug.Write(date);
            }
            
            Console.WriteLine("WRONG DATETIME. Debug");
            return dateconv;
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

                string homeTeam = clonedDirectory[i].homeT;
                string awayTeam = clonedDirectory[i].awayT;

                AccessLeagueElement(divisionTitle, leagueTitle); //Brings you to current list of matches for specified league.
                //Access specific match detail, then bring back to the root page. 

                if(InPlay(homeTeam, awayTeam))
                {
                    Console.Write("\n {0} & {1} is InPlay. Do Nothing \n", homeTeam, awayTeam);
                    RandomSleep(2312);
                    GoBack();
                } else {
                    RandomSleep(2312);
                    //Find Match

                    string xPathHome = GetMatchXpath(homeTeam); // get xPath for homeT
                    string xPathAway = GetMatchXpath(awayTeam); ; // get xPath for awayT

                    IWebElement matchDetails = AWebElement(xPathHome);
                    string webHome = matchDetails.Text.Trim();
                    string webAway = AWebElement(xPathAway).Text.Trim();
                    //string webDate = AWebElement(xPathDate).Text.Trim();

                    if (webHome == homeTeam && awayTeam == webAway)
                    {
                        Console.WriteLine("\n Entered a match!");
                        matchDetails.Click();
                        RandomSleep(3121);
                        GrabBTTSData(webHome, webAway);
                        RandomSleep(1130);
                        GoBack();
                        RandomSleep(1130);
                        GoBack();
                    }
                    else
                    {
                        GoBack();
                    }
                }
            }
        }

        private static void GoBack()
        {
            string forBackButton = "//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div";

            IWebElement button;
            do
            {
                button = AWebElement(forBackButton);

            } while (button == null);

            button.Click();
        }

        private static bool InPlay(string home, string away)
        {

            string forHomeTeam = "//div[contains(@class , 'rcl-ParticipantFixtureDetails_TeamWrapper ')][1]/div[contains(text(), '"+ home.Trim() +"')]";
            string forAwayTeam = "//div[contains(@class , 'rcl-ParticipantFixtureDetails_TeamWrapper ')][2]/div[contains(text(), '" + away.Trim() + "')]";
            Thread.Sleep(21000);
            bool homeTeam = AWebElement(forHomeTeam) != null ? true : false;
            Thread.Sleep(2100);
            bool awayTeamMatch = AWebElement(forAwayTeam) != null ? true : false;

            Debug.Print("\n Boolean Results -> Home:{2} {0} Away:{3} {1}\n", homeTeam, awayTeamMatch, home, away);
            if(homeTeam && awayTeamMatch)
            {
                //This checks the div element that contains the inPlay clock data value. 
                string forInPlayClock = "//div[contains(@class , 'rcl-ParticipantFixtureDetails_TeamWrapper ')][1]/div[contains(text(), " + home.Trim() + ")]/parent::div/parent::div/parent::div/parent::div/div/div[contains(@class,'Clock')]/div[contains(@class,'ClockInPlay_Extra')]";
                bool isInPlay = AWebElement(forInPlayClock) != null ? true : false;
                return isInPlay;
            }
            
            return true;
        }
        private static void GrabBTTSData(string HomeTeamName, string AwayTeamName)
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
            //BTTStoDB(HomeTeamName, AwayTeamName, homeOddsPath, drawOddsPath, awayOddsPath, overTwoFivePath, btsYesPath);

            Console.WriteLine("Over2.5: {0} BTS(yes): {1} Home: {2} Draw: {3} Away: {4} ", overTwoFivePath, btsYesPath, homeOddsPath, drawOddsPath, awayOddsPath);
        }

        private static void BTTStoDB(string HomeTeamName, string AwayTeamName, double homeOddsPath, double drawOddsPath, double awayOddsPath, double overTwoFivePath, double btsYesPath)
        {
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

                //Get Occurrence of 2up based on game odds
                MatchOccurence.GetOccurrences(docUpdate);

                db.UpsertRecordByRefTag<MatchesModel>("Matches", docUpdate, HomeTeamName + " " + AwayTeamName);

                //
            }
        }

        public static void InitiateList()
        {
            leagueDivisionDict.Add("UEFA Champions League", "UEFA Competitions");
            leagueDivisionDict.Add("UEFA Europa League", "UEFA Competitions");
            leagueDivisionDict.Add("Scotland Premiership", "United Kingdom");
            leagueDivisionDict.Add("England Premier League", "United Kingdom");
            leagueDivisionDict.Add("England Championship", "United Kingdom");
            leagueDivisionDict.Add("England League 1", "United Kingdom");
            leagueDivisionDict.Add("England League 2", "United Kingdom");
            leagueDivisionDict.Add("England FA Cup", "United Kingdom");
            //leagueDivisionDict.Add("England EFL Cup", "United Kingdom");
            leagueDivisionDict.Add("Scottish Premiership", "United Kingdom");
            //leagueDivisionDict.Add("Spanish Primera Liga", "Spain");
            leagueDivisionDict.Add("Spain Primera Liga", "Spain");
            leagueDivisionDict.Add("Germany Bundesliga I", "Germany");
            leagueDivisionDict.Add("Italy Serie A", "Italy");
            leagueDivisionDict.Add("France Ligue 1", "France");
            //leagueDivisionDict.Add("AFC Champions League", "Relevant?");
            //leagueDivisionDict.Add("Copa Sudamericana", "Relevant?");

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
