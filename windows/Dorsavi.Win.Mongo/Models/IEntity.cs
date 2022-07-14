using MongoDB.Bson;

namespace Dorsavi.Win.Mongo.Models
{
    public interface IEntity
    {
        ObjectId Id { get; set; }
    }

    public interface IMetadata
    {
        string CreatedBy { get; set; }
        BsonDateTime CreatedUtc { get; set; }
        string CreatedUtcLocale { get; set; }
        string ModifiedBy { get; set; }
        BsonDateTime ModifiedUtc { get; set; }
        string ModifiedUtcLocale { get; set; }




    }
}
