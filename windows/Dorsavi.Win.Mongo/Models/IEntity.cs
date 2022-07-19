using MongoDB.Bson;
using System;

namespace Dorsavi.Win.Mongo.Models
{
    public interface IEntity
    {
        ObjectId Id { get; set; }
    }

    public interface IMetadata
    {
        string CreatedBy { get; set; }
        DateTimeOffset CreatedUtc { get; set; }
        string CreatedUtcLocale { get; set; }
        string ModifiedBy { get; set; }
        DateTimeOffset ModifiedUtc { get; set; }
        string ModifiedUtcLocale { get; set; }




    }
}
