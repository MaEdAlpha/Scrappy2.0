using System;
using System.Collections.Generic;
using System.Text;
using DnsClient.Protocol;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Scrappy2._0
{
    class MongoCRUD
    {
        private IMongoDatabase db;

        //Atlas code
        public MongoCRUD(string database)
        {
            var client = new MongoClient();
            db = client.GetDatabase(database);
        }

        public void InsertRecord<T>(string table, T record)
        {
            var collection = db.GetCollection<T>(table);
            collection.InsertOne(record);
        }

        public List<T> LoadRecords<T>(string table)
        {
            var collection = db.GetCollection<T>(table);

            return collection.Find(new BsonDocument()).ToList();
        }

        public T LoadRecordById<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return collection.Find(filter).First();
        }

        public T LoadRecordByRefTag<T>(string table, string RefTag)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("RefTag", RefTag);

            return collection.Find(filter).First();
        }

        public long CountRecordsByRefTag<T>(string table, string RefTag)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("RefTag", RefTag);

            return collection.Find(filter).CountDocuments();
        }


        public void UpsertRecord<T>(string table, Guid id, T record)
        {
            // Create the collection
            var collection = db.GetCollection<T>(table);
            // Create the filter
            var filter = Builders<T>.Filter.Eq("Id", id);
            // Replace the record via an upsert
            // Upsert is an update if the filter finds something or
            // an insert if there is nothing matching
            collection.ReplaceOne(
                filter,
                record,
                new ReplaceOptions { IsUpsert = true });
        }

        public void UpsertRecordByRefTag<T>(string table, T record, string RefTag)
        {
            // Create the collection
            var collection = db.GetCollection<T>(table);
            // Create the filter
            var filter = Builders<T>.Filter.Eq("RefTag", RefTag);
            // Replace the record via an upsert
            // Upsert is an update if the filter finds something or
            // an insert if there is nothing matching
            collection.ReplaceOne(
                filter,
                record,
                new ReplaceOptions { IsUpsert = true });
        }

        public void DeleteRecord<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            collection.DeleteOne(filter);
        }
    }
}
