using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Dorsavi.Win.Mongo.Common;
using Dorsavi.Win.Mongo.Interface;
using Realms;
using Realms.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dorsavi.Win.Mongo.Data
{
    public class DatabaseStrategy : IDatabaseStrategy
    {
        private static List<MongoDb> Realms { get; set; }

        public Func<DbRegion, IDatabaseContext> db = processor =>
        {
            return Realms.FirstOrDefault(x => x.AppliesTo(processor));
        };

        public DatabaseStrategy()
        {
        }

        public ServiceResponse Create<T>(DbRegion region, T data) where T : RealmObject
        {
            return db(region).Create(data);
        }

        public List<T> Read<T>(DbRegion region) where T : RealmObject
        {
            return db(region).Read<T>();
        }

        public T Read<T>(DbRegion region, long Id) where T : RealmObject
        {
            return db(region).Read<T>(Id);
        }

        public void Update<T>(DbRegion region, long Id) where T : RealmObject
        {
            db(region).Update<T>(Id);
        }

        public void Delete<T>(DbRegion region, long Id) where T : RealmObject
        {
            db(region).Delete<T>(Id);
        }

        public async Task<ServiceResponse> InitialiseDatabases(DbRegion region, string apiId, string apiKey, string partitionConfig)
        {
            return await db(region).InitialiseAsync(apiId, apiKey, partitionConfig, region);
        }
    }

    public class MongoDb : BasePublisher, IDisposable, IDatabaseContext
    {
        private Realm connection { get; set; }
        private string publisherName { get; set; }

        public bool AppliesTo(DbRegion dbRegion)
        {
            return dbRegion == Region;
        }

        public MongoDb()
        {
            publisherName = this.GetType().Name;
            ModifyPublisher(publisherName, PublisherType.NewMongo);
            connection.RealmChanged += Connection_RealmChanged;
        }

        public DbRegion Region { get; set; }

        public async Task<ServiceResponse> InitialiseAsync(string apiId, string apiKey, string partitionConfig, DbRegion region)
        {
            var resp = new ServiceResponse();
            try
            {

                var app = App.Create(apiId);
                var user = await app.LogInAsync(Credentials.ApiKey(apiKey));
                var config = new PartitionSyncConfiguration(partitionConfig, user);
                connection = await Realm.GetInstanceAsync(config);
                Region = region;
                return resp;
            }
            catch (System.Exception ex)
            {
                Publish(PublisherType.ErrorPublisher, ex.Message);
                resp = resp.ToInvalidRequest(ex.Message);
            }
            return resp;
        }

        private void Connection_RealmChanged(object sender, System.EventArgs e)
        {
            Publish(publisherName, $"Region : {Region}", sender);
        }

        public ServiceResponse Create<T>(T data) where T : RealmObject
        {
            var resp = new ServiceResponse();
            if (connection == null)
            {
                resp.ToInvalidRequest("Realm not initialised");
                return resp;
            }

            try
            {
                connection.Write(() =>
                {
                    connection.Add(data);
                });
                resp.Valid = true;
            }
            catch (Exception ex)
            {
                resp.ToInvalidRequest("Realm not initialised");
                return resp;
            }
            return resp;
        }

        public List<T> Read<T>() where T : RealmObject
        {
            var result = connection.All<T>();
            return result.ToList();
        }

        public T Read<T>(long Id) where T : RealmObject
        {
            return connection.Find<T>(Id);
        }

        public void Update<T>(long Id) where T : RealmObject
        {
            var result = connection.Find<T>(Id);
            //connection.Write(() =>
            //{
            //    result.IsValid
            //});
        }

        public void Delete<T>(long Id) where T : RealmObject
        {
            var result = connection.Find<T>(Id);
            if (result != null)
                connection.Write(() =>
                {
                    connection.Remove(result);
                });
        }

        public void Dispose()
        {
            ModifyPublisher(publisherName, PublisherType.NewMongo, true);
            this.Dispose();
        }
    }
}