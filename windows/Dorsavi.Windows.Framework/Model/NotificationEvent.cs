using Dorsavi.Win.Framework.PubSub;
using System;

namespace Dorsavi.Win.Framework.Model
{
    public class NotificationEvent : EventArgs
    {
        public string NotificationMessage { get; private set; }

        public DateTime NotificationDate { get; private set; }

        public string PublisherName { get; private set; }
        public PublisherType PublisherType { get; private set; }
        public Object Content { get; private set; }

        public NotificationEvent(DateTime _dateTime, string _message, PublisherType publisherType, string publisherName)
        {
            NotificationDate = _dateTime;
            NotificationMessage = _message;
            PublisherType = publisherType;
            PublisherName = publisherName;
        }
        public NotificationEvent(DateTime _dateTime, string _message, PublisherType publisherType, string publisherName, object content) : this(_dateTime, _message, publisherType, publisherName)
        {
            Content = content;
        }
    }


}