using MongoDB.Bson;
using Realms;

namespace Dorsavi.Win.Mongo.Models
{
    public class Site : RealmObject, IEntity
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("accountId")]
        public string AccountId { get; set; }

        [MapTo("siteId")]
        public string SiteId { get; set; }

        [MapTo("name")]
        public string Name { get; set; }

        [MapTo("description")]
        public string Description { get; set; }

        [MapTo("active")]
        public bool Active { get; set; }

        [MapTo("region")]
        public string Region { get; set; }

        [MapTo("createdUtc")]
        public string CreatedUtc { get; set; }

        [MapTo("modifiedUtc")]
        public string ModifiedUtc { get; set; }
    }
}
