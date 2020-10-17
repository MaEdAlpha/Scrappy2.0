﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Console.WriteLine("Choose from the following options: \n 1. Bet365 \n 2. SMarkets \n 3.Quit");
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
                RootClass.GetRootPage("https://www.bet365.com/#/AS/B1/");
                Bet365.LeagueSelection();
                Program.settingUp = false;
            }
            if (_selection == 2)
            {
                //SmarketsCode Initialize
            }
            if (_selection == 3)
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