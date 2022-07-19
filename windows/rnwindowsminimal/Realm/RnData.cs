using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Dorsavi.Win.Mongo.Common;
using Dorsavi.Win.Mongo.Data;
using Dorsavi.Win.Mongo.Interface;
using Dorsavi.Win.Mongo.Models;
using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace rnwindowsminimal.Realm
{
    [ReactModule]
    public class RnData : BasePublisher
    {
        #region Events

        [ReactEvent]
        public Action<string> RealmChanged { get; set; }

        private void NotificationReceived(object sender, EventArgs e)
        {
            if (e.GetType() == typeof(NotificationEvent))
            {
                var notificationEvent = (NotificationEvent)e;
                if (notificationEvent.PublisherType == PublisherType.NewMongo)
                {
                    try
                    {
                        RealmChanged(JsonConvert.SerializeObject(notificationEvent.Content));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        #endregion Events

        private IDatabaseStrategy db { get; set; }

        public RnData()
        {
            db= new DatabaseStrategy();
            _subscriber.NotificationReceived += NotificationReceived;
        }

        [ReactMethod("Init")]
        public async Task<string> Init(string region, string apiId, string apiKey, string partitionConfig)
        {
            var resp = new ServiceResponse();

            if (!DbRegion.TryFromName(region, out DbRegion regionResult))
            {
                resp.ToInvalidRequest($"Region not matched, please select from available region : {string.Join(",", DbRegion.List.Select(x => x.Name))}");
                return JsonConvert.SerializeObject(resp);
            }

            return JsonConvert.SerializeObject(await db.InitialiseDatabases(regionResult, apiId, apiKey, partitionConfig));
        }

        [ReactMethod("GetAll")]
        public async Task<string> Get(string region, string type)
        {
            var resp = new ServiceResponse();

            if (!DbRegion.TryFromName(region, out DbRegion regionResult))
            {
                resp.ToInvalidRequest($"Region not matched, please select from available region : {string.Join(",", DbRegion.List.Select(x => x.Name))}");
                return JsonConvert.SerializeObject(resp);
            }

            return JsonConvert.SerializeObject(db.Read<subject>(regionResult));
        }

        [ReactMethod("Get")]
        public async Task<string> Get(string region, string type, int Id)
        {
            var resp = new ServiceResponse();

            if (!DbRegion.TryFromName(region, out DbRegion regionResult))
            {
                resp.ToInvalidRequest($"Region not matched, please select from available region : {string.Join(",", DbRegion.List.Select(x => x.Name))}");
                return JsonConvert.SerializeObject(resp);
            }


            Type dataType = Type.GetType(type);
            object instance = Activator.CreateInstance(dataType);


            return JsonConvert.SerializeObject(db.Read<instance.GetType()>(regionResult));
        }

        [ReactMethod("Update")]
        public async Task<string> Get(string region, string type, int Id, string jsonData)
        {
            var resp = new ServiceResponse();

            if (!DbRegion.TryFromName(region, out DbRegion regionResult))
            {
                resp.ToInvalidRequest($"Region not matched, please select from available region : {string.Join(",", DbRegion.List.Select(x => x.Name))}");
                return JsonConvert.SerializeObject(resp);
            }

            return JsonConvert.SerializeObject(db.Read<subject>(regionResult));
        }

        [ReactMethod("Delete")]
        public async Task<string> Delete(string region, string type, int Id)
        {
            var resp = new ServiceResponse();

            if (!DbRegion.TryFromName(region, out DbRegion regionResult))
            {
                resp.ToInvalidRequest($"Region not matched, please select from available region : {string.Join(",", DbRegion.List.Select(x => x.Name))}");
                return JsonConvert.SerializeObject(resp);
            }

            return JsonConvert.SerializeObject(db.Read<subject>(regionResult));
        }
    }
}