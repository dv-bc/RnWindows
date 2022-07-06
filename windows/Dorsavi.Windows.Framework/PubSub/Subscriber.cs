using Dorsavi.Windows.Framework.Model;
using System;

namespace Dorsavi.Windows.Framework.PubSub
{
    public class Subscriber
    {
        public string SubscriberName { get; private set; }

        public delegate void Notice(Publisher p, NotificationEvent e);

        public event Notice NotificationReceived;

        public Subscriber(string _subscriberName)
        {
            SubscriberName = _subscriberName;
        }

        public void Subscribe(Publisher p)
        {
            p.OnPublish += OnNotificationReceived;  // multicast delegate
        }

        public void Unsubscribe(Publisher p)
        {
            p.OnPublish -= OnNotificationReceived;  // multicast delegate
        }

        protected virtual void OnNotificationReceived(Publisher p, NotificationEvent e)
        {
            NotificationReceived(p, e);
            //Console.WriteLine(SubscriberName + ", " + e.NotificationMessage + " - " + p.PublisherName + " at " + e.NotificationDate);
        }
    }
}