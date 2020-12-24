using System;
using System.Collections.Generic;
using System.Text;

namespace Scrappy2._0
{
    class XpathDirectory : RootClass
    {

        public static string GetLeagueXpath(string _divisionTitle, string _leagueTitle)
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
        public static string GetMatchXpath(string matchVariableXpath)
        {
            //div[contains(@class, 'rcl-ParticipantFixtureDetails_TeamNames')] / child::div[@class = 'rcl-ParticipantFixtureDetails_TeamWrapper '] / child::div[contains(text(), '

            string start = " //div[contains(@class, 'rcl-ParticipantFixtureDetails_TeamNames')] / child::div[@class = 'rcl-ParticipantFixtureDetails_TeamWrapper '] / child::div[contains(text(), \"";
            string middle = matchVariableXpath;
            string end = "\")]";

            string xPath = start + middle + end;
            return xPath;
        }

        public static string GetOddsXpath(string index)
        {
            //
            string start = "//div[contains(@class, 'sgl-MarketOddsExpand gl-Market_General gl-Market_General-columnheader ')]/div['";
            string middle = index;
            string end = "']/span";

            string path = start + middle + end;
            return path;
        }

        public static bool ScrapeThisMatch(int webElementIndex)
        {
            string inPlayClockPath = "//div[contains(@class, 'rcl-ParticipantFixtureDetails_Details ')]["+ webElementIndex +"]/div[contains(@class,'rcl-ParticipantFixtureDetails_Clock rcl-ParticipantFixtureDetails_Clock-wide pi-CouponParticipantClockInPlay ')]";
 
            bool scrapeThisMatch = AWebElement(inPlayClockPath) == null ? true : false;
            return scrapeThisMatch;
        }

        //div[contains(@class, 'rcl-ParticipantFixtureDetails_Details ')]/div[contains(@class,'rcl-ParticipantFixtureDetails_Clock rcl-ParticipantFixtureDetails_Clock-wide pi-CouponParticipantClockInPlay ')]
    }
}
