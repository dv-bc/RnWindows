using Dorsavi.Win.Framework.Infrastructure;
using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Dorsavi.Win.Mongo.Common;
using Dorsavi.Win.Mongo.Interface;
using Dorsavi.Win.Mongo.Models;
using Realms;
using Realms.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
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
            if (Realms == null)
            {
                Realms = new List<MongoDb>();
                foreach (var region in DbRegion.List)
                {
                    Realms.Add(new MongoDb(region));
                }
            }
        }

        public ServiceResponse Create<T>(DbRegion region, T data) where T : RealmObject
        {
            return db(region).Create(data);
        }

        public async Task<ServiceResponse<List<T>>> Read<T>(DbRegion region) where T : RealmObject
        {
            return await db(region).Read<T>();
        }

        public T Read<T>(DbRegion region, long Id) where T : RealmObject
        {
            return db(region).Read<T>(Id);
        }

        public ServiceResponse Update<T>(DbRegion region, long Id, T data) where T : RealmObject
        {
            return db(region).Update<T>(Id, data);
        }

        public ServiceResponse Delete<T>(DbRegion region, long Id) where T : RealmObject
        {
            return db(region).Delete<T>(Id);
        }

        public async Task<ServiceResponse> InitialiseDatabases(DbRegion region, string apiId, string apiKey, string partitionConfig)
        {
            return await db(region).InitialiseAsync(apiId, apiKey, partitionConfig, region);
        }
    }

    public class MongoDb : BasePublisher, IDisposable, IDatabaseContext
    {
        private Realm connection { get; set; }
        private PartitionSyncConfiguration Config { get; set; }

        private string publisherName { get; set; }

        public bool AppliesTo(DbRegion dbRegion)
        {
            return dbRegion == Region;
        }

        public MongoDb(DbRegion region)
        {
            Region = region;
            publisherName = this.GetType().Name;
            ModifyPublisher(publisherName, PublisherType.NewMongo);
        }

        public DbRegion Region { get; set; }

        public async Task<ServiceResponse> InitialiseAsync(string apiId, string apiKey, string partitionConfig, DbRegion region)
        {
            var resp = new ServiceResponse();
            try
            {
                if (connection != null)
                {
                    resp.Message.Add($"realm {region.Name} is initialised");
                    return resp;
                }

                var app = App.Create(apiId);
                var user = await app.LogInAsync(Credentials.ApiKey(apiKey));
                Config = new PartitionSyncConfiguration(partitionConfig, user);
                connection = await Realm.GetInstanceAsync(Config);
                connection.RealmChanged += Connection_RealmChanged;
                var current = Thread.CurrentThread;
                return resp;
            }
            catch (System.Exception ex)
            {
                Publish(PublisherType.NotificationPublisher, ex.Message);
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

        public async Task<ServiceResponse<List<T>>> Read<T>() where T : RealmObject
        {
            var resp = new ServiceResponse<List<T>>();
            connection = await Realm.GetInstanceAsync(Config);
            try
            {
                var result = connection.All<T>();
                resp.Content = result.ToList();
            }
            catch (Exception ex)
            {
                resp = resp.ToInvalidRequest(ex.Message);
            }
            return resp;
        }

        public T Read<T>(long Id) where T : RealmObject
        {
            return connection.Find<T>(Id);
        }

        public ServiceResponse Update<T>(long Id, T data) where T : RealmObject
        {
            var resp = new ServiceResponse();
            try
            {
                var dbData = connection.Find<Site>(Id);
                connection.Write(() =>
                {
                    data.CopyProperties(dbData);
                });
                resp.Valid = true;
            }
            catch (Exception ex)
            {
                resp = resp.ToInvalidRequest(ex.Message);
            }
            return resp;
        }

        public ServiceResponse Delete<T>(long Id) where T : RealmObject
        {
            var resp = new ServiceResponse();
            try
            {
                var result = connection.Find<T>(Id);
                if (result != null)
                    connection.Write(() =>
                    {
                        connection.Remove(result);
                    });
                resp.Valid = true;
            }
            catch (Exception ex)
            {
                resp = resp.ToInvalidRequest(ex.Message);
            }
            return resp;



        }

        public void Dispose()
        {
            ModifyPublisher(publisherName, PublisherType.NewMongo, true);
            this.Dispose();
        }
    }
}