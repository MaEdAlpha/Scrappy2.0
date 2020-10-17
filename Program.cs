﻿using MongoDB.Driver;
using System;
using System.Linq;

namespace Scrappy2._0
{
    class Program
    {
        public static bool settingUp = true;
        public static bool scraping365 = false;
        public static bool scrapingSmarkets = false;
        public static bool bttsEnabled = false;
        public static bool pathListPopulated = false;
        //public static MongoClient clientDb;

        public static void Main()
        {
            ScrapeSelector valueOf = new ScrapeSelector(); //First selection user makes. Passed into HandleSelection.
            //clientDb = new MongoClient("mongodb + srv://Randy:M7bkD0xFr91G0DfA@clusterme.lfzcj.mongodb.net/matchEdge?retryWrites=true&w=majority");

            while (settingUp) { 
                    ScrapeSelector.HandleSelection(valueOf.SelectedSite());
                settingUp = false;
            }

            while (scraping365) {
                    Bet365.Scrape();
                    pathListPopulated = true;
                    Bet365.DisplaySummary();
                while(bttsEnabled && pathListPopulated) {
                    Bet365.CollectData();
                }
            }

            while (scrapingSmarkets)
            {
                Smarkets.ScrapeLeague();
            }
        }
    }
}
