using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scrappy2._0
{
    class TeamNamesModel
    {
        [BsonId]
        public string _id { get; set; }
        public string Alias { get; set; }
      }

}
