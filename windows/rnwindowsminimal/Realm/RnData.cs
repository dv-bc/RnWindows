using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Dorsavi.Win.Mongo.Common;
using Dorsavi.Win.Mongo.Data;
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
                    RealmChanged(JsonConvert.SerializeObject(notificationEvent.Content));
                }
            }
        }

        #endregion Events

        private DatabaseStrategy db { get; set; }

        public RnData()
        {
            _subscriber.NotificationReceived += NotificationReceived;
        }

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
    }
}