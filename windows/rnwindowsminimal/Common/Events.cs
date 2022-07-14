using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Framework.PubSub;
using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using System;

namespace rnwindowsminimal.Common
{
    public class Events : BaseSubscriber
    {
        public Events() : base()
        {
            _subscriber.NotificationReceived += NotificationReceived;
        }

        [ReactEvent]
        public Action<string> Event { get; set; }

        [ReactEvent]
        public Action<string> Notification { get; set; }

        private void NotificationReceived(object sender, EventArgs e)
        {
            if (e.GetType() == typeof(NotificationEvent))
            {
                var notificationEvent = (NotificationEvent)e;
                if (notificationEvent.PublisherType == PublisherType.EventPublisher)
                {
                    Event(JsonConvert.SerializeObject(notificationEvent));
                }
                else if (notificationEvent.PublisherType == PublisherType.NotificationPublisher)
                {
                    Notification(JsonConvert.SerializeObject(notificationEvent));
                }
            }
        }
    }
}
