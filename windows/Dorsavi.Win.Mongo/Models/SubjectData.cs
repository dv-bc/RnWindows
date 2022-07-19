using MongoDB.Bson;
using Realms;
using System;
using System.Collections.Generic;

namespace Dorsavi.Win.Mongo.Models
{
    public class subject : RealmObject, IEntity, IMetadata
    {
        public subject()
        {
        }

        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("SubjectId")]
        public string SubjectId { get; set; }

        [MapTo("Active")]
        public bool Active { get; set; }

        [MapTo("SiteId")]
        [Required]
        public string SiteId { get; set; }

        [MapTo("Iv")]
        public string Iv { get; set; }

        [MapTo("SubjectData")]
        public IList<subject_SubjectData> SubjectData { get; }

        [MapTo("CreatedBy")]
        [Required]
        public string CreatedBy { get; set; }

        [MapTo("CreatedUtc")]
        public DateTimeOffset CreatedUtc { get; set; }

        [MapTo("CreatedUtcLocale")]
        [Required]
        public string CreatedUtcLocale { get; set; }

        [MapTo("ModifiedBy")]
        [Required]
        public string ModifiedBy { get; set; }

        [MapTo("ModifiedUtc")]
        public DateTimeOffset ModifiedUtc { get; set; }

        [MapTo("ModifiedUtcLocale")]
        [Required]
        public string ModifiedUtcLocale { get; set; }
    }

    public class subject_SubjectData : EmbeddedObject
    {
        [MapTo("Name")]
        public string Name { get; set; }

        [MapTo("Value")]
        public string Value { get; set; }
    }


    public class GraphData : EmbeddedObject
    {
        public string label { get; set; }

        public string key { get; set; }

        public string color { get; set; }
    }

    public class TestInProgress : EmbeddedObject
    {
        public int startTimeStamp { get; set; }


        public int stopTimeStamp { get; set; }


        public IList<int> startIndex { get;  }


        public IList<int> stopIndex { get;  }


        public string videoURI { get; set; }


        public bool? lob { get; set; }
    }

    

    public class MovementRepSchema : RealmObject, IEntity
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();


        public string AssessmentId { get; set; }


        public int? Rep { get; set; }


        public string Name { get; set; }


        public string Result { get; set; }


        public string CompressBy { get; set; }


        public string SiteId { get; set; }
    }
    public class AppSession : RealmObject, IEntity
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string value { get; set; }
    }
}