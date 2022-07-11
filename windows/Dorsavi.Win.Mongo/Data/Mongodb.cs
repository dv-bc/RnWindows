using Dorsavi.Win.Framework.Infrastructure;
using Dorsavi.Win.Framework.PubSub;
using Realms;
using Realms.Sync;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dorsavi.Win.Mongo.Data
{
    public class MongoDb
    {
        private Realm connection { get; set; }

        private readonly List<Publisher> _publishers;
        private readonly Subscriber _subscriber;

        public MongoDb()
        {
            _publishers = Singleton<List<Publisher>>.Instance;
            _subscriber = Singleton<Subscriber>.Instance;

            var publisher = new Publisher(this.GetType().Name, PublisherType.NewMongo);
            _subscriber.Subscribe(publisher);
            _publishers.Add(publisher);


            connection.RealmChanged += Connection_RealmChanged;

        }
        public async Task InitialiseAsync()
        {
            try
            {
                var app = App.Create("");
                var user = await app.LogInAsync(Credentials.Anonymous());
                var config = new PartitionSyncConfiguration("myPart", user);
                connection = await Realm.GetInstanceAsync(config);

            }
            catch (System.Exception ex)
            {
            }

        }

        private void Connection_RealmChanged(object sender, System.EventArgs e)
        {
            var publisher = _publishers.FirstOrDefault(x => x.PublisherName == this.GetType().Name);
            if (publisher != null)
            {
                publisher.Publish("Realm updated, see content for details", sender);
            }
        }

        public void Create<T>(T data) where T : RealmObject
        {
            //if (connection == null)
            //    return null;

            connection.Write(() =>
            {
                connection.Add(data);
            });
        }

        public List<T> Read<T>() where T : RealmObject
        {
            var result = connection.All<T>();
            return result.ToList();
        }

        public T Read<T>(long Id) where T : RealmObject
        {
            return connection.Find<T>(Id);
        }

        public void Update<T>(long Id) where T : RealmObject
        {
            var result = connection.Find<T>(Id);
            //connection.Write(() =>
            //{
            //    result.IsValid
            //});
        }

        public void Delete<T>(long Id) where T : RealmObject
        {
            var result = connection.Find<T>(Id);
            if (result != null)
                connection.Write(() =>
                {
                    connection.Remove(result);
                });
        }


    }
}