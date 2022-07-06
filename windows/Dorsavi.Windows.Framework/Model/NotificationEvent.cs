using Dorsavi.Windows.Framework.PubSub;
using System;

namespace Dorsavi.Windows.Framework.Model
{
    public class NotificationEvent : EventArgs
    {
        public string NotificationMessage { get; private set; }

        public DateTime NotificationDate { get; private set; }

        public string PublisherName { get; private set; }
        public PublisherType PublisherType { get; private set; }

        public NotificationEvent(DateTime _dateTime, string _message, PublisherType publisherType, string publisherName)
        {
            NotificationDate = _dateTime;
            NotificationMessage = _message;
            PublisherType = publisherType;
            PublisherName = publisherName;
        }
    }
}