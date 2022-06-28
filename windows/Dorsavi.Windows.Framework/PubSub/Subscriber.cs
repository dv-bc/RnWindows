﻿using Dorsavi.Windows.Framework.Model;
using System;

namespace Dorsavi.Windows.Framework.PubSub
{
    public class Subscriber
    {

        public string SubscriberName { get; private set; }

        public event EventHandler NotificationReceived;

        public Subscriber(string _subscriberName)
        {
            SubscriberName = _subscriberName;
        }

        // This function subscribe to the events of the publisher
        public void Subscribe(Publisher p)
        {

            // register OnNotificationReceived with publisher event
            p.OnPublish += OnNotificationReceived;  // multicast delegate 

        }

        // This function unsubscribe from the events of the publisher
        public void Unsubscribe(Publisher p)
        {

            // unregister OnNotificationReceived from publisher
            p.OnPublish -= OnNotificationReceived;  // multicast delegate 
        }

        // It get executed when the event published by the Publisher
        protected virtual void OnNotificationReceived(Publisher p, NotificationEvent e)
        {
            NotificationReceived?.Invoke(this, e);
            Console.WriteLine(SubscriberName + ", " + e.NotificationMessage + " - " + p.PublisherName + " at " + e.NotificationDate);
        }
    }
}
