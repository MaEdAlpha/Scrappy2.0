using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrappy2._0
{
    class Program
    {
        public static bool settingUp = true;
        public static bool scraping365Full = false;
        public static bool scraping365HA = false;
        public static bool scrapingSmarkets = false;
        public static bool scrapingMatchbook = false;
        public static bool bttsEnabled = false;
        public static bool pathListPopulated = false;
        private static int loop;
        public static List <TeamNamesModel> TeamNamesLibrary;

        //Lookup array of team names keyvalue and secondary value. Used throughout.

        public static void Main()
        {
            ScrapeSelector valueOf = new ScrapeSelector(); //First selection user makes. Passed into HandleSelection.
            //clientDb = new MongoClient("mongodb + srv://Randy:M7bkD0xFr91G0DfA@clusterme.lfzcj.mongodb.net/matchEdge?retryWrites=true&w=majority");

           RootClass.CreateDatabaseConnection();
            TeamNamesLibrary = RootClass.db.LoadRecords<TeamNamesModel>("TeamNamesLibrary");

            while (settingUp) { 
                    ScrapeSelector.HandleSelection(valueOf.SelectedSite());
                settingUp = false;
            }

            while (scraping365Full) {
                    loop = 0;
                    RootClass.SetImplicitWait(2);
                    Bet365.Scrape();
                    pathListPopulated = true;
                    Bet365.DisplaySummary();
                while(bttsEnabled && pathListPopulated) {
                    Bet365.CollectData();
                    loop++;
                    if(loop > 5)
                    {
                        MatchPath.ResetList(); // Otherwise we build off of this. 
                        pathListPopulated = false;
                    }
                }
            }

            while (scraping365HA)
            {
                loop = 0;
                Bet365.ScrapeHA();
                pathListPopulated = true;
                Bet365.DisplaySummary();
                while (bttsEnabled && pathListPopulated)
                {
                    Bet365.CollectData();
                    loop++;
                    if (loop > 5)
                    {
                        MatchPath.ResetList(); // Otherwise we build off of this. 
                        pathListPopulated = false;
                    }
                }
            }

            while (scrapingSmarkets)
            {
                Smarkets.ScrapeLeague();
            }
            while (scrapingMatchbook)
            {
                Matchbook.ScrapeLeague();
            }
        }
    }
}
