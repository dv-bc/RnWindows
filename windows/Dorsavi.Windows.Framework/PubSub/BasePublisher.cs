using Dorsavi.Win.Framework.Common;
using Dorsavi.Win.Framework.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Dorsavi.Win.Framework.PubSub
{
    public class BasePublisher
    {
        protected readonly List<Publisher> _publishers;
        protected readonly Subscriber _subscriber;

        public BasePublisher()
        {
            _publishers = Singleton<List<Publisher>>.Instance;
            _subscriber = Singleton<Subscriber>.Instance;

            if (!_publishers.Any(x => x.PublisherName == Constants.NotificationPublisher))
            {
                var publisher = new Publisher(Constants.NotificationPublisher, PublisherType.NotificationPublisher);
                _publishers.Add(publisher);
            }

            if (!_publishers.Any(x => x.PublisherName == Constants.EventPublisher))
            {
                var publisher = new Publisher(Constants.EventPublisher, PublisherType.EventPublisher);
                _publishers.Add(publisher);
            }
        }

        protected bool ModifyPublisher(string publisherName, PublisherType publisherType, bool unsubscribe = false)
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherName == publisherName || x.PublisherType == publisherType);
            if (publisher == null)
            {
                publisher = new Publisher(publisherName, PublisherType.NewMongo);

                if (!unsubscribe)
                {
                    _subscriber.Subscribe(publisher);
                    _publishers.Add(publisher);
                }
                else
                {
                    _subscriber.Unsubscribe(publisher);
                    _publishers.Remove(publisher);
                }
                return true;
            }
            //Cannot find publisher
            return false;
        }


        protected bool Publish(PublisherType publisherType, string message)
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherType == publisherType);

            if (publisher != null)
            {
                publisher.Publish(message);
                return true;
            }
            return false;
        }

        protected bool Publish(string publisherName, string message, object sender)
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherName.ToUpperInvariant() == publisherName.ToUpperInvariant());

            if (publisher != null)
            {
                publisher.Publish(message, sender);
                return true;
            }
            return false;
        }

        protected bool Publish(string publisherName, string message)
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherName.ToUpperInvariant() == publisherName.ToUpperInvariant());

            if (publisher != null)
            {
                publisher.Publish(message);
                return true;
            }
            return false;
        }
    }

}