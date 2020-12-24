using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using MongoDB.Bson.Serialization.Serializers;
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
        private static Dictionary<string, string> TabRefList = new Dictionary<string, string>();
        private static readonly List<string> countries = new List<string>();
        private static int masterCount;
        private static int tmsCount;
        private static int pathCount;
        private const int DATERANGE = 3; //days
        private static bool NewTabAdded = false;
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
         
            //If the country name is accessible...
            if (IsAccessibleCheck(divisionTitle, leagueTitle))                              //Checks to see if leagues are accessible. HeadOpen class activated or not? If not, should open it.
            {
                //Attempt to locate the league.
                IWebElement leagueIWebElement = AWebElement(leagueXPath);

                if (leagueIWebElement != null)
                {
                    //Click the league
                    ClickLeagueElement(divisionTitle, leagueTitle);     //Method will bring you to list of Matches for specified league.   
                    return true;
                } else {
                    //League not found
                    return false;
                }               
            }
            //Country not found
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

                    if (Program.bttsEnabled)
                    {
                        //Here we need to check if we are going to scrape BTTS. If Yes then just use 1 tab
                        leagueIWebElement.Click();
                    }
                    else 
                    {
                        //Else open a tab for each league and loop through

                        //Check to see if the current league already has a tab open 
                        string strTempValue;
                        
                        if (TabRefList.TryGetValue(_leagueTitle, out strTempValue))
                        {
                            //If it does set the focus to that tab
                            driver.SwitchTo().Window(strTempValue);
                        }
                        else
                        {
                            //If it doesn't then open a new tab and add the tab handler to the dictionary

                            //B365 don't have the href in the HTML so we will open the page to get the URL
                            leagueIWebElement.Click();

                            //Get the URL for the current page and duplicate it in a new tab.
                            ((IJavaScriptExecutor)driver).ExecuteScript("window.open(\'" + driver.Url + "\')");
                            driver.SwitchTo().Window(driver.WindowHandles.Last());

                            //Add the handler and league to the dictionary. 
                            TabRefList.Add(_leagueTitle, driver.CurrentWindowHandle);

                            NewTabAdded = true;

                        }

                    }

                }
            }
            catch (NullReferenceException)
            {
                Debug.Print("ClickLeagueElement couldn't find: {0}:{1} IWebElement", _divisionTitle, _leagueTitle);
                throw;
            }
        }

        public static Boolean GameOK(string GameInfo)
        {

            //if there are more than 2 /r in the match string then there is a score present so the match is inplay
            int inumLines = GameInfo.Split("\r\n").Length;

            if (inumLines > 3)
            {
                return false;
            }
            else
            {
                return true;
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
                bool OddsChanged = false;

                Console.Write("\n Building Directory {0}", leagueTitle);
                RandomSleep(1500);
                //All dates and match titles. 
                string forAllMatches = "//div[contains(@class, 'sgl-MarketFixtureDetailsLabel gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div";
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

                            if (GameOK(matchWebElement.Text))
                            {
                               
                                string item = matchWebElement.Text;
                                string[] matchDetails = matchWebElement.Text.Split("\r\n");

                                finalDate = ConverToDateTime(tempDate, matchDetails[0]).ToUniversalTime().ToString("dd'/'MM'/'yyyy HH:mm:ss");
                                matchTime = ConverToDateTime(tempDate, matchDetails[0]).ToString("HH:mm");

                                string ScrapedHomeTeam = matchDetails[1];
                                string ScrapedAwayTeam = matchDetails[2];

                               
                                if (CheckifTeamNameAliasExists(matchDetails[1]) is null)
                                {
                                    //Doesn't exist so must be a new alias for an exisiting Team name. Request user input.
                                    Console.WriteLine(" Enter the universal team name for: " + matchDetails[1]);
                                    matchDetails[1] = Convert.ToString(Console.ReadLine());

                                    TeamNamesModel TeamNamesModel = new TeamNamesModel
                                    {
                                        _idAlias = ScrapedHomeTeam,
                                        UniversalTeamName = matchDetails[1],
                                    };
                                    db.InsertRecord("TeamNamesLibrary", TeamNamesModel);

                                    //Updates the internal list of team names to include any new addtions from previous scrape
                                    Program.TeamNamesLibrary = db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");
                                }
                                else
                                {
                                    matchDetails[1] = GetUniversalTeamName(matchDetails[1]);
                                }

                                if (CheckifTeamNameAliasExists(matchDetails[2]) is null)
                                {
                                    //Doesn't exist so must be a new alias for an exisiting Team name. Request user input.
                                    Console.WriteLine(" Enter the universal team name for: " + matchDetails[2]);
                                    matchDetails[2] = Convert.ToString(Console.ReadLine());

                                    TeamNamesModel TeamNamesModel = new TeamNamesModel
                                    {
                                        _idAlias = ScrapedAwayTeam,
                                        UniversalTeamName = matchDetails[2],
                                    };
                                    db.InsertRecord("TeamNamesLibrary", TeamNamesModel);

                                    //Updates the internal list of team names to include any new addtions from previous scrape
                                    Program.TeamNamesLibrary = db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");
                                }
                                else
                                {
                                    matchDetails[2] = GetUniversalTeamName(matchDetails[2]);
                                }

                                Console.WriteLine("\n---Date: {3} Time: {0} Home: {1} Away: {2}", matchTime, matchDetails[1], matchDetails[2], finalDate);


                                //First check to see if any matches are postponed. If they are, get the index number in which they reside in.
                                //Pass an array of all existing indexes to act as identifiers where postponed match divs exist. 

                                List<string> filteredOddsResults = GetStuffedB365(); //Checks for postponed matches

                                string index = (pathCount + 1).ToString(); //pathCount is a tracker for what match we are on. Postponed matches are still part of this list. We need to determine a way to detect them.
                                                                     //Compare this index string to the filtered ShoveItB365Odds() array. If index == elm.index && elm[index] == null (postponed match) do nothing. 


                                string forOdds = GetOddsXpath(index);
                                List<IWebElement> oddsList = WebElements(forOdds);

                                //DebugPrintOdds(oddsList);

                                if(filteredOddsResults == null)
                                {
                                    tempHomeOdds = oddsList[pathCount].Text;
                                    tempDrawOdds = oddsList[((oddsList.Count / 3)) + pathCount].Text;
                                    tempAwayOdds = oddsList[((oddsList.Count / 3 * 2)) + pathCount].Text;
                                    //Console.WriteLine("In non Postponed odds write");
                                }
                                else
                                {
                                    tempHomeOdds = filteredOddsResults[pathCount];
                                    tempDrawOdds = filteredOddsResults[((filteredOddsResults.Count / 3)) + pathCount];
                                    tempAwayOdds = filteredOddsResults[((filteredOddsResults.Count / 3 * 2)) + pathCount];
                                    
                                }

                                Console.WriteLine("Date: {0}\n Country: {1} \n League: {2} \n mTime: {3}\n home:{4}\n away:{5} \nODDS H/D/A: {6}/{7}/{8}", finalDate, divisionTitle, leagueTitle, matchTime, matchDetails[1].Trim(), matchDetails[2].Trim(), tempHomeOdds, tempDrawOdds, tempAwayOdds);
                                MatchPath package = new MatchPath(divisionTitle, leagueTitle, matchTime, finalDate, ScrapedHomeTeam, ScrapedAwayTeam, null);
                                if(tempHomeOdds == "999")
                                {
                                    package.isPostponed = true;
                                }
                                else
                                {
                                    package.isPostponed = false;
                                }

                                //Get match from DB. If odds have changed update and add a record to OddsRecords table. 
                                MatchesModel match;

                                //Check if RefTag exists
                                if (db.CountRecordsByRefTag<MatchesModel>("matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim() + " " + finalDate) > 0)
                                {
                                    match = db.LoadRecordByRefTag<MatchesModel>("matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim() + " " + finalDate);

                                    //If the scraped Home odds are different to the current odds in DB add the current odds to the oddsrecord object and update the current odds.
                                    if (Convert.ToDouble(tempHomeOdds) != match.B365HomeOdds && tempHomeOdds != "")
                                    {
                                        OddsRecordModel OddsUpdated = new OddsRecordModel
                                        {
                                            RefTag = match.RefTag,
                                            TeamName = match.HomeTeamName,
                                            Odds = tempHomeOdds,
                                            DateTimeStamp = DateTime.UtcNow,
                                            OddsType = "B365Home"
                                        };

                                        db.InsertRecord("OddsRecords", OddsUpdated);

                                        //Add the previous odds to the matches object before updating
                                        match.PreviousB365HomeOdds = match.B365HomeOdds;

                                        OddsChanged = true;
                                    }

                                    //If the scraped Away odds are different to the current odds in DB add the current odds to the oddsrecord object and update the current odds.
                                    if (Convert.ToDouble(tempAwayOdds) != match.B365AwayOdds && tempAwayOdds != "")
                                    {
                                        OddsRecordModel OddsUpdated = new OddsRecordModel
                                        {
                                            RefTag = match.RefTag,
                                            TeamName = match.AwayTeamName,
                                            Odds = tempAwayOdds,
                                            DateTimeStamp = DateTime.UtcNow,
                                            OddsType = "B365Away"
                                        };

                                        db.InsertRecord("OddsRecords", OddsUpdated);

                                        //Add the previous odds to the matches object before updating
                                        match.PreviousB365AwayOdds = match.B365AwayOdds;

                                        OddsChanged = true;
                                    }

                                    if (OddsChanged == true)
                                    {
                                        //If the odds changed update the matches object
                                        match.B365HomeOdds = Convert.ToDouble(tempHomeOdds);
                                        match.B365DrawOdds = Convert.ToDouble(tempDrawOdds);
                                        match.B365AwayOdds = Convert.ToDouble(tempAwayOdds);
                                        match.League = leagueTitle;

                                        WriteToDataBase(match);

                                        //Reset odds changed variable
                                        OddsChanged = false;
                                    }
                                }
                                else
                                {

                                    MatchesModel Newmatch = new MatchesModel
                                    {
                                        RefTag = matchDetails[1].Trim() + " " + matchDetails[2].Trim() + " " + finalDate,
                                        HomeTeamName = matchDetails[1].Trim(),
                                        AwayTeamName = matchDetails[2].Trim(),
                                        B365HomeOdds = Convert.ToDouble(tempHomeOdds),
                                        B365DrawOdds = Convert.ToDouble(tempDrawOdds),
                                        B365AwayOdds = Convert.ToDouble(tempAwayOdds),
                                        League = leagueTitle,
                                        StartDateTime = finalDate,
                                        PreviousB365HomeOdds = 0,
                                        PreviousB365AwayOdds = 0
                                    };

                                    WriteToDataBase(Newmatch);
                                }

                            /////////////////////////////////////////////////////////////////


                            matchDetails[1] = ScrapedHomeTeam;
                            matchDetails[2] = ScrapedAwayTeam;

                            MatchPath.SaveXpath(package);

                            leagueMatchCount++;
                            pathCount++;
                            masterCount++;
                            tmsCount++;
                            }
                            else
                            {
                                //this match is inplay or postponed. Move on to the next
                                pathCount++;
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

                    if (Program.bttsEnabled == false)
                    {
                        //If using multiple tabs ensure we are on the main tab
                        driver.SwitchTo().Window(driver.WindowHandles.First());

                        if (NewTabAdded == true)
                        {
                            driver.Navigate().Back();
                            NewTabAdded = false;
                        }
                    }
                    else
                    {
                        driver.Navigate().Back();
                    }
                    
                }
            }
        }

        private static List<string> GetStuffedB365()
        {
            if (AWebElement("//div[contains(@class, 'postponed')]") != null) //see if this page has any postponed matches
            {
                List<IWebElement> allOddFixtures = WebElements("//div[contains(@class, 'sgl-MarketOddsExpand gl-Market_General gl-Market_General-columnheader ')]/div");
                int totalElm = allOddFixtures.Count(); // gives you total count. We will later divide by 3, to get the total rows in each column of '1' 'X' '2'.
                List<string> indexFilterList = new List<string>();

                for (int i = 0; i < totalElm; i++)
                {
                    //Xpath for condition where '1' 'X' '2' exist.
                    if (allOddFixtures[i].Text.Length == 1)
                    {
                        //div[contains(@class, 'sgl-MarketOddsExpand gl-Market_General gl-Market_General-columnheader ')]/div[contains(@class,'rcl-MarketColumnHeader ')]
                        //div[contains(@class, 'sgl-MarketOddsExpand gl-Market_General gl-Market_General-columnheader ')]/div[contains(@class,'rcl-MarketHeaderLabel ')]
                        //Do Nothing
                        //Exclude these from your list.
                    }
                    //Check to see if div has null or texts "Match Postponed". 
                    else if (allOddFixtures[i].Text.Length > 4 || allOddFixtures[i].Text.Length == 0)
                    {
                        indexFilterList.Add("999");
                        //Console.Write(i + " Postponed\n");

                    }
                    else if (allOddFixtures[i].Text.Length == 4)
                    {
                        //Xpath;
                        //div[contains(@class, 'sgl-MarketOddsExpand gl-Market_General gl-Market_General-columnheader ')]/div[contains(@class,'postponed')]
                        //Add this to your list.
                        indexFilterList.Add(allOddFixtures[i].Text);
                        //Console.Write(i + " " + allOddFixtures[i].Text + " " + allOddFixtures[i].Text.Count() + "\n");
                        //Check to see if div has actual number.
                    }
                    else
                    {
                        Debug.Print("Something Unaccounted in GetStuffedBet365()");
                    }
                }
                return indexFilterList;
            }
            return null; //if no postponed matches exist, return a null list to be used in if else statement for getting odds.
        }


        private static void WriteToDataBase(MatchesModel match)
        {
            db.UpsertRecordByRefTag("matches", match, match.RefTag);
        }

            private static void WriteToDB(string leagueTitle, string tempHomeOdds, string tempDrawOdds, string tempAwayOdds, string[] matchDetails, string finalDate)
        {
           
            MatchesModel match;

            //Check if RefTag exists
            if (db.CountRecordsByRefTag<MatchesModel>("matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim() + " " + finalDate) > 0)
            {
                match = db.LoadRecordByRefTag<MatchesModel>("matches", matchDetails[1].Trim() + " " + matchDetails[2].Trim() + " " + finalDate);

                match.B365HomeOdds = Convert.ToDouble(tempHomeOdds);
                match.B365DrawOdds = Convert.ToDouble(tempDrawOdds);
                match.B365AwayOdds = Convert.ToDouble(tempAwayOdds);
                match.League = leagueTitle;
            }
            else
            {
                match = new MatchesModel
                {
                    RefTag = matchDetails[1].Trim() + " " + matchDetails[2].Trim() + " " + finalDate,
                    HomeTeamName = matchDetails[1].Trim(),
                    AwayTeamName = matchDetails[2].Trim(),
                    B365HomeOdds = Convert.ToDouble(tempHomeOdds),
                    B365DrawOdds = Convert.ToDouble(tempDrawOdds),
                    B365AwayOdds = Convert.ToDouble(tempAwayOdds),
                    League = leagueTitle,
                    StartDateTime = finalDate
                };
            }

            db.UpsertRecordByRefTag("matches", match, match.RefTag);
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
            //Build xPath for country name
            string xPath = CheckOpenHeaderXpath(_headerTitle);
            IWebElement container;

            //If the country name is found
            if (AWebElement(xPath) != null)
            {
                return true;
            }
            else 
            {
                // if a Openheader returns nothing then use GetHeaderTitleXpath() to build an xpath locate a counrty name with a closed branch. 
                xPath = GetHeaderTitleXpath(_headerTitle);

                Thread.Sleep(2000);
                //Wait for country name element to load.
                
                //while (PageLoading)
                //{
                    container = AWebElement(xPath);
                    if (container != null)
                    {
                        container.Click(); // opens drop down
                        Thread.Sleep(2000);
                        bool answer = AWebElement(xPath) != null ? true : false;
                        return answer;
                    }
                //}

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
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

            if (month != "Jan")
            {
                date = "2020 " + month + dayInt + " " + hoursMinutes.Trim() + ":00";
            } else
            {
                date = "2021 " + month + dayInt + " " + hoursMinutes.Trim() + ":00";
            }
            try
            {
                dateconv = DateTime.SpecifyKind(DateTime.Parse(date) - offset, DateTimeKind.Utc);
                return dateconv;
            }
            catch (Exception)
            {
            }
           
            return dateconv;
        }
        public static void CollectData()
        {
            List<MatchPath> clonedDirectory = new List<MatchPath>(); //Initialize a new list
            
            clonedDirectory = MatchPath.GetXpathList();

            Console.WriteLine("\n Collecting Data for {0} matches...", clonedDirectory.Count());

            //Create an inefficient way to travel to these pages. Humanizes the URL navigation
             Shuffle(clonedDirectory);

            //Print(clonedDirectory);
            for (int i = 0; i < clonedDirectory.Count(); i++)
            {
                //Enter into first league.
                string divisionTitle = clonedDirectory[i].country;
                string leagueTitle = clonedDirectory[i].league;

                string homeTeam = clonedDirectory[i].homeT;
                string awayTeam = clonedDirectory[i].awayT;

                RandomSleep(1112);
                if(clonedDirectory[i].isPostponed != true)
                {
                    AccessLeagueElement(divisionTitle, leagueTitle); //Brings you to current list of matches for specified league.
                                                                     //Access specific match detail, then bring back to the root page. 

                    if (InPlay(homeTeam, awayTeam))
                    {
                        Console.Write("\n {0} & {1} is InPlay. Do Nothing \n", homeTeam, awayTeam);
                        RandomSleep(512);
                        driver.Navigate().Back();
                    }
                    else
                    {
                        RandomSleep(1312);
                        //Find Match

                        string xPathHome = GetMatchXpath(homeTeam); // get xPath for homeT
                        string xPathAway = GetMatchXpath(awayTeam); ; // get xPath for awayT

                        IWebElement matchDetails = AWebElement(xPathHome);
                        string webHome = matchDetails.Text.Trim();
                        string webAway = AWebElement(xPathAway).Text.Trim();
                        //string webDate = AWebElement(xPathDate).Text.Trim();

                        if (webHome == homeTeam && awayTeam == webAway)
                        {
                            Console.WriteLine("\n Entered a match: {0} vs {1} \n", clonedDirectory[i].homeT, clonedDirectory[i].awayT );
                            matchDetails.Click();
                            RandomSleep(721);
                            GrabBTTSData(webHome, webAway, clonedDirectory[i].date, clonedDirectory[i].mTime);
                            RandomSleep(500);
                            driver.Navigate().Back();
                            RandomSleep(1030);
                            driver.Navigate().Back();
                        }
                        else
                        {
                            driver.Navigate().Back();
                        }
                    }
                }else
                {
                    Console.WriteLine("\n" + clonedDirectory[i].homeT + " vs " + clonedDirectory[i].awayT + " :POSTPONED. Skipping.... \n");
                }              
            }
        }

        private static void GoBack()
        {
           // string forBackButton = "//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div";

            IWebElement button;
            do
            {
                button = AWebElement("//div[contains(@class, 'sgl-MarketFixtureDetailsLabelExpand3 gl-Market_General gl-Market_General-columnheader gl-Market_General-haslabels ')]/child::div");
                Thread.Sleep(500);
            } while (button == null);

            button.Click();
        }

        private static bool InPlay(string home, string away)
        {

            string forHomeTeam = "//div[contains(@class , 'rcl-ParticipantFixtureDetails_TeamWrapper ')][1]/div[contains(text(), \"" + home.Trim() + "\")]";
            string forAwayTeam = "//div[contains(@class , 'rcl-ParticipantFixtureDetails_TeamWrapper ')][2]/div[contains(text(), \"" + away.Trim() + "\")]";
            Thread.Sleep(1800);
            bool homeTeam = AWebElement(forHomeTeam) != null ? true : false;
            Thread.Sleep(2100);
            bool awayTeamMatch = AWebElement(forAwayTeam) != null ? true : false;

            Debug.Print("\n Boolean Results -> Home:{2} {0} Away:{3} {1}\n", homeTeam, awayTeamMatch, home, away);
            if(homeTeam && awayTeamMatch)
            {
                //This checks the div element that contains the inPlay clock data value. 
                string forInPlayClock = "//div[contains(@class , 'rcl-ParticipantFixtureDetails_TeamWrapper ')][1]/div[contains(text(), \"" + home.Trim() + "\")]/parent::div/parent::div/parent::div/parent::div/div/div[contains(@class,'Clock')]/div[contains(@class,'ClockInPlay_Extra')]";
                //If does not retrun null, retrun true, else return false
                bool isInPlay = AWebElement(forInPlayClock) != null ? true : false;
                return isInPlay;
            }
            
            return true;
        }
        private static void GrabBTTSData(string HomeTeamName, string AwayTeamName, string date, string matchTime)
        {
            string danielsDateData = ConverToDateTime(date, matchTime).ToString("dd'/'MM'/'yyyy HH:mm:ss");
            string danielsMatchTimedata = matchTime;
            
            RandomSleep(700);
            List<IWebElement> elements = null;
            do
            {
              elements = WebElements("//div[contains(@class, 'gl-MarketGroupButton_Text ') and contains(text(),'Full Time Result')]/parent::div/following-sibling::div/child::div/child::div/div/span[contains(@class, 'gl-Participant_Odds')]");

            } while (elements.Count < 2);

            Double homeOddsPath = Convert.ToDouble(elements[0].Text);
            Double drawOddsPath = Convert.ToDouble(elements[1].Text);
            Double awayOddsPath = Convert.ToDouble(elements[2].Text);

            List<IWebElement> odds = WebElements("//div[@class = 'gl-MarketGroupButton_Text ' and contains(text(),'Goals Over/Under')]/parent::div/following-sibling::div/div/div/div/span[@class='gl-ParticipantOddsOnly_Odds']");
            RandomSleep(1121);
            Double overTwoFivePath = Convert.ToDouble(odds[0].Text);
            Double btsYesPath = Convert.ToDouble(AWebElement("//div[@class = 'gl-MarketGroupButton_Text ' and contains(text(),'Both Teams to Score')]/parent::div/following-sibling::div/descendant::span[@class= 'gl-ParticipantBorderless_Odds'][1]").Text);


            // Add the match the DB
            BTTStoDB(GetUniversalTeamName(HomeTeamName), GetUniversalTeamName(AwayTeamName), homeOddsPath, drawOddsPath, awayOddsPath, overTwoFivePath, btsYesPath, date);

            Console.WriteLine("Over 2.5: {0} BTS(Y): {1} Home: {2} Draw: {3} Away: {4} \n", overTwoFivePath, btsYesPath, homeOddsPath, drawOddsPath, awayOddsPath);
        }

        private static void BTTStoDB(string HomeTeamName, string AwayTeamName, double homeOddsPath, double drawOddsPath, double awayOddsPath, double overTwoFivePath, double btsYesPath, string GameStartDate)
        {
            
            //////Retrieve record from DB if exists 

            MatchesModel match;

            match = db.LoadRecordByRefTag<MatchesModel>("matches", HomeTeamName + " " + AwayTeamName + " " + GameStartDate);

            match.B365HomeOdds = Convert.ToDouble(homeOddsPath);
            match.B365DrawOdds = Convert.ToDouble(drawOddsPath);
            match.B365AwayOdds = Convert.ToDouble(awayOddsPath);
            match.B365BTTSOdds = Convert.ToDouble(btsYesPath);
            match.B365O25GoalsOdds= Convert.ToDouble(overTwoFivePath);
            match.URLB365 = driver.Url;

            //Get Occurrence of 2up based on game odds
            MatchOccurence.GetOccurrences(match);
            
            db.UpsertRecordByRefTag("matches", match, match.RefTag);

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
            //leagueDivisionDict.Add("England FA Cup", "United Kingdom");
            //leagueDivisionDict.Add("England EFL Cup", "United Kingdom");
            //leagueDivisionDict.Add("Scottish Premiership", "United Kingdom");
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
