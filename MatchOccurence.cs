using System;
using System.Collections.Generic;
using System.Text;

namespace Scrappy2._0
{
    class MatchOccurence
    {
        public static void GetOccurrences(MatchesModel CurrentMatch)
        {
            //    'Run a check on all the odds to see what the occurance of 2up FTA is

            //Dim dOccuranceHome As Single
            float dOccurrenceHome;

            //Dim dOccuranceAway As Single
            float dOccurrenceAway;

            //Dim bIsSuperSweetSpot As Boolean
            Boolean bIsSuperSweetSpot = true;

            //Dim bIsSweetSpot As Boolean
            Boolean bIsSweetSpot = true;

            //'Set occurrence to original value
            //dOccuranceHome = 55
            dOccurrenceHome = 55;
            //dOccuranceAway = 75
            dOccurrenceAway = 75;


            //Home Odds
            if (CurrentMatch.B365HomeOdds < 1.4)

            {
                dOccurrenceHome = dOccurrenceHome + 20;
            }
            else if (CurrentMatch.B365HomeOdds < 1.501)
            {
                dOccurrenceHome = dOccurrenceHome + 5;
            }
            else if (CurrentMatch.B365HomeOdds < 1.7)
            {

            }
            else if (CurrentMatch.B365HomeOdds < 1.9)
            {
                dOccurrenceHome = dOccurrenceHome - 3;
            }

            else if (CurrentMatch.B365HomeOdds < 3.001)
            {
                dOccurrenceHome = dOccurrenceHome - 6;
            }
            else if (CurrentMatch.B365HomeOdds < 3.501)
            {
                dOccurrenceHome = dOccurrenceHome - 3;
            }
            else if (CurrentMatch.B365HomeOdds < 5.001)
            {

            }
            else if (CurrentMatch.B365HomeOdds < 7.001)
            {
                dOccurrenceHome = dOccurrenceHome + 5;
            }
            else if (CurrentMatch.B365HomeOdds < 9.999)
            {
                dOccurrenceHome = dOccurrenceHome + 10;
            }
            else
            {
                dOccurrenceHome = dOccurrenceHome + 15;
            }


            //Draw Odds
            if (CurrentMatch.B365DrawOdds < 3.399)
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365DrawOdds < 3.799)
            {
                dOccurrenceHome = (float)(dOccurrenceHome - 2.5);
                dOccurrenceAway = (float)(dOccurrenceAway - 2.5);
            }
            else if (CurrentMatch.B365DrawOdds < 6.999)
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else
            {
                dOccurrenceHome = (float)(dOccurrenceHome + 2.5);
                dOccurrenceAway = (float)(dOccurrenceAway + 2.5);
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }

            //Away Odds
            if (CurrentMatch.B365AwayOdds < 1.4)
            {
                dOccurrenceAway = dOccurrenceAway + 20;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365AwayOdds < 1.501)
            {
                dOccurrenceAway = dOccurrenceAway + 5;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365AwayOdds < 1.7)
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365AwayOdds < 1.9)
            {
                dOccurrenceAway = dOccurrenceAway - 3;
                bIsSuperSweetSpot = false;
            }
            else if (CurrentMatch.B365AwayOdds < 3.001)
            {
                dOccurrenceAway = dOccurrenceAway - 6;
            }
            else if (CurrentMatch.B365AwayOdds < 3.501)
            {
                dOccurrenceAway = dOccurrenceAway - 3;
            }
            else if (CurrentMatch.B365AwayOdds < 5.001)
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365AwayOdds < 7.001)
            {
                dOccurrenceAway = dOccurrenceAway + 5;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365AwayOdds < 10)
            {
                dOccurrenceAway = dOccurrenceAway + 10;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else
            {
                dOccurrenceAway = dOccurrenceAway + 15;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }



            //BTTS
            if (CurrentMatch.B365BTTSOdds < 1.499)
            {
                dOccurrenceHome = dOccurrenceHome - 12;
                dOccurrenceAway = dOccurrenceAway - 12;
            }
            else if (CurrentMatch.B365BTTSOdds < 1.599)
            {
                dOccurrenceHome = dOccurrenceHome - 9;
                dOccurrenceAway = dOccurrenceAway - 9;
            }
            else if (CurrentMatch.B365BTTSOdds < 1.7)
            {
                dOccurrenceHome = dOccurrenceHome - 6;
                dOccurrenceAway = dOccurrenceAway - 6;
                bIsSuperSweetSpot = false;
            }
            else if (CurrentMatch.B365BTTSOdds < 1.749)
            {
                dOccurrenceHome = dOccurrenceHome - 6;
                dOccurrenceAway = dOccurrenceAway - 6;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365BTTSOdds < 1.949)
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365BTTSOdds < 2.049)
            {
                dOccurrenceHome = dOccurrenceHome + 5;
                dOccurrenceAway = dOccurrenceAway + 5;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else
            {
                dOccurrenceHome = dOccurrenceHome + 10;
                dOccurrenceAway = dOccurrenceAway + 10;
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }

            //O2.5
            if (CurrentMatch.B365O25GoalsOdds < 1.7)
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }
            else if (CurrentMatch.B365O25GoalsOdds < 1.9)
            {
                dOccurrenceHome = (float)(dOccurrenceHome - 2.5);
                dOccurrenceAway = (float)(dOccurrenceAway - 2.5);
            }
            else
            {
                bIsSuperSweetSpot = false;
                bIsSweetSpot = false;
            }


            //Apply sweet spot boosts
            if (bIsSuperSweetSpot == true)
            {
                dOccurrenceAway = dOccurrenceAway - 7;
            }
            else if (bIsSweetSpot == true)
            {
                dOccurrenceAway = dOccurrenceAway - 4;
            }

            CurrentMatch.OccurrenceHome = dOccurrenceHome;
            CurrentMatch.OccurrenceAway = dOccurrenceAway;

            //                'If super sweet spot colour dark blue
            //                If dOccuranceAway< 50 Then


            //                'If sweet spot colour light blue
            //                ElseIf dOccuranceAway< 60 Then
            //          

            //                'If normal colour green
            //                ElseIf dOccuranceAway< 70 Then
            //                  

            //               'If not great colour yellow
            //                ElseIf dOccuranceAway< 80 Then
            //                  
            //                Else
            //                 'it's the shit spot so colour red
            //                  

        }
    }
}
