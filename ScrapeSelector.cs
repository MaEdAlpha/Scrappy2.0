using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Scrappy2._0
{
    class ScrapeSelector
    {
        public int SelectedSite()
        {
            try
            {
                Console.WriteLine("Choose from the following options: \n 1. Bet365Full \n 2. Bet365HA \n 3. SMarkets \n 4. Matchbook\n 5. Quit");
                int selection = Convert.ToInt32(Console.ReadLine());
                return selection;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0} ", e.Message);
            }
            return 0;
        }

        public static void HandleSelection(int _selection)
        {
            if (_selection == 0)
            {
                ScrapeSelector obj = new ScrapeSelector();
                obj.SelectedSite();
            }
            if (_selection == 1)
            {
                Program.scraping365Full = true;                
                RootClass.GetRootPage("https://www.bet365.com/#/AS/B1/", false);
              
                    if (Bet365.leagueDivisionDict.Count() == 0) 
                    {
                      Bet365.InitiateList(); 
                      MatchPath.CreateList(); 
                    }

                Bet365.LeagueSelection();
                Program.settingUp = false;
            }
            if (_selection == 2)
            {
                Program.scraping365HA = true;
                RootClass.StartGecko(false);

                if (Bet365.leagueDivisionDict.Count() == 0)
                {
                    Bet365.InitiateListB365HA();
                    MatchPath.CreateList();
                }

                Bet365.LeagueSelection();
                Program.settingUp = false;
            }

            if (_selection == 3)
            {
                //SmarketsCode Initialize
                Program.scrapingSmarkets = true;
                RootClass.GetRootPage("https://smarkets.com/sport/football", true);
                RootClass.SetImplicitWait(7);
                Smarkets.InitiateList();
                Smarkets.ScrapeSelection();
            }
            if (_selection == 4)
            {
                //MatchbookCode Initialize
                Program.scrapingMatchbook = true;
                RootClass.GetRootPage("https://www.matchbook.com/events/soccer", true);
                RootClass.SetImplicitWait(7);
                Matchbook.InitiateList();
                Matchbook.ScrapeSelection();
            }

            if (_selection == 5)
            {
                RootClass.Quit();
            }
            else
            {
                Debug.Write("Error in HandleSelection()");
            }
        }
    }
}
