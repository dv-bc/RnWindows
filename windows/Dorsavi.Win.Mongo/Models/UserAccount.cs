using MongoDB.Bson;
using Realms;

namespace Dorsavi.Win.Mongo.Models
{
    public class UserAccount : RealmObject, IEntity
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("accounts")]
        public string Accounts { get; set; }
    }



}