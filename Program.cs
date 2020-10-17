using MongoDB.Driver;
using System;
using System.Linq;

namespace Scrappy2._0
{
    class Program
    {
        public static bool settingUp = true;
        public static bool scraping = true;
        public static bool bttsEnabled = false;
        //public static MongoClient clientDb;

        public static void Main()
        {
            ScrapeSelector valueOf = new ScrapeSelector(); //First selection user makes. Passed into HandleSelection.
            //clientDb = new MongoClient("mongodb + srv://Randy:M7bkD0xFr91G0DfA@clusterme.lfzcj.mongodb.net/matchEdge?retryWrites=true&w=majority");

            if (Bet365.leagueCountry.Count() == 0) { Bet365.InitiateList(); MatchPath.CreateList(); }

            while (settingUp) { ScrapeSelector.HandleSelection(valueOf.SelectedSite()); }

            while (scraping)
            {
                Bet365.Scrape();
                Bet365.DisplaySummary();

            }
        }
    }
}
