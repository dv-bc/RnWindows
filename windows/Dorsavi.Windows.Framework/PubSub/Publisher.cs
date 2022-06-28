using Dorsavi.Windows.Framework.Model;
using System;

namespace Dorsavi.Windows.Framework.PubSub
{
    public class Publisher
    {

        //publishers name
        public string PublisherName { get; private set; }

        //publishers notification interval
        public int NotificationInterval { get; private set; }

        // declare a delegate function
        public delegate void Notify(Publisher p, NotificationEvent e);

        // declare an event variable of the delegate function
        public event Notify OnPublish;

        // class constructor
        public Publisher(string _publisherName, int _notificationInterval)
        {
            PublisherName = _publisherName;
            NotificationInterval = _notificationInterval;
        }

        //publish function publishes a Notification Event
        public void Publish(string message)
        {
            // fire event after certain interval
            //Thread.Sleep(NotificationInterval);

            if (OnPublish != null)
            {
                NotificationEvent notificationObj = new NotificationEvent(DateTime.Now, message);
                OnPublish(this, notificationObj);
            }
            //Thread.Yield();

        }
    }
}
