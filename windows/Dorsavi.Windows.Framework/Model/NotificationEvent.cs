using System;

namespace Dorsavi.Windows.Framework.Model
{
    public class NotificationEvent : EventArgs
    {

        public string NotificationMessage { get; private set; }

        public DateTime NotificationDate { get; private set; }

        public NotificationEvent(DateTime _dateTime, string _message)
        {
            NotificationDate = _dateTime;
            NotificationMessage = _message;
        }

    }
}
