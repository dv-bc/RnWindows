using MongoDB.Bson;
using Realms;
using System.Collections.Generic;

namespace Dorsavi.Win.Mongo.Models
{
    public class User : RealmObject, IEntity
    {
        public User()
        {
            Resources = new List<string>();
        }

        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("pin")]
        public string Pin { get; set; }

        [MapTo("dorsaviUserId")]
        public string DorsaviUserId { get; set; }

        [MapTo("firstName")]
        public string FirstName { get; set; }

        [MapTo("lastName")]
        public string LastName { get; set; }

        [MapTo("emailAddress")]
        public string EmailAddress { get; set; }

        [MapTo("resources")]
        public List<string> Resources { get; set; }
    }
}