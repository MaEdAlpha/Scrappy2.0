using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scrappy2._0
{
        class MatchesModel
        {
            [BsonId]
            //public Guid Id { get; set; }
            public string RefTag { get; set; }
            public string HomeTeamName { get; set; }
            public string AwayTeamName { get; set; }
            public string SmarketsHomeOdds { get; set; }
            public string SmarketsAwayOdds { get; set; }
            public double B365HomeOdds { get; set; }
            public double B365DrawOdds { get; set; }
            public double B365AwayOdds { get; set; }
            public double B365BTTSOdds { get; set; }
            public double B365O25GoalsOdds { get; set; }
            public string StartDateTime { get; set; }
            public string League { get; set; }
            public double OccurrenceHome { get; set; }
            public double OccurrenceAway { get; set; }

    }

    class OddsRecordModel
    {
        [BsonId]
        public Guid Id { get; set; }
        public string RefTag { get; set; }
        public string TeamName { get; set; }
        public string OddsType { get; set; }
        public string Odds { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }


    }


