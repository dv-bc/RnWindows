using Dorsavi.Win.Framework.Infrastructure;
using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Dorsavi.Win.Mongo.Data;
using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace rnwindowsminimal.Realm
{
    [ReactModule]
    public class RnData
    {
        [ReactEvent]
        public Action<string> RealmChanged { get; set; }

        private readonly Subscriber _subscriber;

        private MongoDb db { get; set; }
        public RnData()
        {
            _subscriber = Singleton<Subscriber>.Instance;
            _subscriber.NotificationReceived += NotificationReceived;
            db = new MongoDb();
            Task.Run(() => db.InitialiseAsync());
        }



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
    }
}
