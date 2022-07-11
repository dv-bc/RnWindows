using Ardalis.SmartEnum;
using Dorsavi.Win.Framework.Model;
using System;

namespace Dorsavi.Win.Framework.PubSub
{
    public class PublisherType : SmartEnum<PublisherType>
    {
        private PublisherType(string name, int value) : base(name, value)
        {
        }

        public static readonly PublisherType PropertyChanged = new PublisherType("PropertyChanged", 1);
        public static readonly PublisherType SubscriptionValue = new PublisherType("SubscriptionValue", 2);
        public static readonly PublisherType ConnectedDevice = new PublisherType("ConnectedDevice", 3);
        public static readonly PublisherType NewMongo = new PublisherType("ConnectedDevice", 3);
    }

    public class Publisher
    {
        public string PublisherName { get; private set; }

        public PublisherType PublisherType { get; private set; }

        public int NotificationInterval { get; private set; }

        public delegate void Notify(Publisher p, NotificationEvent e);

        public event Notify OnPublish;

        public Publisher(string _publisherName, PublisherType publisherType) : this(_publisherName, publisherType, 1000)
        {
        }

        public Publisher(string _publisherName, PublisherType publisherType, int _notificationInterval)
        {
            PublisherName = _publisherName;
            PublisherType = publisherType;
            NotificationInterval = _notificationInterval;
        }

        //publish function publishes a Notification Event
        public void Publish(string message)
        {
            // fire event after certain interval
            //Thread.Sleep(NotificationInterval);

            if (OnPublish != null)
            {
                NotificationEvent notificationObj = new NotificationEvent(DateTime.Now, message, PublisherType, PublisherName);
                OnPublish(this, notificationObj);
            }
            //Thread.Yield();
        }

        public void Publish(string message, object Content)
        {
            // fire event after certain interval
            //Thread.Sleep(NotificationInterval);

            if (OnPublish != null)
            {
                NotificationEvent notificationObj = new NotificationEvent(DateTime.Now, message, PublisherType, PublisherName, Content);
                OnPublish(this, notificationObj);
            }
            //Thread.Yield();
        }
    }
}