# Scrappy2.0

/*  Things to Do*/

- SMarkets 
- Handle Format Exception in date conversion section of bet365
- Add in BTS and over2.5
- Aggressiveness factor (allow for a simple way to change 'speed setting' on scraper)


Questions for DBrooks

Where: Bet365.cs CollectData() method. Final IF statement ~ line:365 IF statement comparing homeT awayT

Why: We will pull the wrong match details given the case below. 

What: In Canadian Hockey..Finals, we have a playoff scenario where two teams play a best of 5...This creates a condition with the  same teams playing multiple games over a short period of time.
      If we encounter any  cases like this in UK footie it will ALWAYS select the first homeT vs. awayT match detail. Can we safely say this scenario will never happen in football?               



     -------------------------------- xPath Note --------------------------------------------------------
Example xPath in sMarkets code:     //ul[contains(@class,'event-list list-view  football']//li[contains(@class,'item-tile event-tile  upcoming layout-row ')] 
                                    //<element>[contains(@class, 'attribute')] <<----Very flexible, will not break if they add extra whitespace (which seems to be what happened with old sMarket Code);

                                    BU