using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrappy2._0
{
    class MatchPath
    {
        public string country { get; set; }
        public string league { get; set; }
        public string mTime { get; set; }
        public string date { get; set; }
        public string homeT { get; set; }
        public string awayT { get; set; }
        public bool? isPostponed { get; set; }

        public static List<MatchPath> PathList { get; set; } // Used to find the specific match you want to open with Selenium

        public MatchPath(string c, string l, string mT, string d, string h, string a, bool? p)
        {
            country = c;
            league = l;
            mTime = mT;
            date = d;
            homeT = h;
            awayT = a;
            isPostponed = p;
        }

        public static void SaveXpath(MatchPath obj)
        {
            PathList.Add(obj);
        }

        public static List<MatchPath> GetXpathList()
        {
            return PathList;
        }

        public static void CreateList()
        {
            PathList = new List<MatchPath>();
        }

        public static void ResetList()
        {
            PathList.Clear();
        }

        public static void PrintList()
        {
            foreach (MatchPath item in PathList)
            {
                Console.WriteLine("\n-------------------\n League: {1}, {0} |{2}| @ {3} H:{4} A:{5} isPostponed: {6}\n-----------------------", item.country, item.league, item.date, item.mTime, item.homeT, item.awayT, item.isPostponed);
            }
            Console.WriteLine("Total List count: {0}\n Total Path count: {1}", MatchPath.PathList.Count(), 9999);
        }
    }
}
