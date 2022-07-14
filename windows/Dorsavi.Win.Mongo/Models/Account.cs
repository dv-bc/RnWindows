using Dorsavi.Win.Mongo.Models;
using MongoDB.Bson;
using Realms;

public class Account : RealmObject, IEntity
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("accountId")]
    public string AccountId { get; set; }

    [MapTo("reference")]
    public string Reference { get; set; }

    [MapTo("description")]
    public string Description { get; set; }

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("active")]
    public bool? Active { get; set; }

    [MapTo("statusId")]
    public int? StatusId { get; set; }

    [MapTo("region")]
    public string Region { get; set; }

    [MapTo("createdUtc")]
    public string CreatedUtc { get; set; }

    [MapTo("modifiedUtc")]
    public string ModifiedUtc { get; set; }
}