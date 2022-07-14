using MongoDB.Bson;
using Realms;
using System.Collections.Generic;

namespace Dorsavi.Win.Mongo.Models
{
    public class subject : RealmObject, IEntity, IMetadata
    {
        public subject()
        {
            SubjectData = new List<SubjectData>();
        }

        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("SubjectId")]
        public string SubjectId { get; set; }

        [MapTo("Active")]
        public bool Active { get; set; }

        [MapTo("SiteId")]
        public string SiteId { get; set; }

        [MapTo("Iv")]
        public string Iv { get; set; }

        [MapTo("SubjectData")]
        public List<SubjectData> SubjectData { get; set; }

        [MapTo("CreatedBy")]
        public string CreatedBy { get; set; }

        [MapTo("CreatedUtc")]
        public BsonDateTime CreatedUtc { get; set; }

        [MapTo("CreatedUtcLocale")]
        public string CreatedUtcLocale { get; set; }

        [MapTo("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [MapTo("ModifiedUtc")]
        public BsonDateTime ModifiedUtc { get; set; }

        [MapTo("ModifiedUtcLocale")]
        public string ModifiedUtcLocale { get; set; }
    }

    public class SubjectData : EmbeddedObject
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


        public List<int> startIndex { get; set; }


        public List<int> stopIndex { get; set; }


        public string videoURI { get; set; }


        public bool? lob { get; set; }
    }

    public class assessment_Movement : EmbeddedObject
    {
        public string Name { get; set; }


        public string Title { get; set; }


        public string TestInProgressData { get; set; }


        public int? Score { get; set; }
    }

    public class assessment_Overview : EmbeddedObject
    {
        public double? AmiRatingAgg { get; set; }



        public double? MovementScoreAgg { get; set; }



        public double? LSIRatingAgg { get; set; }



        public double? LobRatingAgg { get; set; }



        public double? InjuryAgg { get; set; }



        public string InjuryName { get; set; }


        public double? SportAgg { get; set; }



        public string SportName { get; set; }


        public double? ConcussionAgg { get; set; }



        public string ConcussionName { get; set; }


        public string Gender { get; set; }


        public double? Tsk11 { get; set; }
    }

    //== MAIN ==


    public class assessment : RealmObject, IEntity
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string AccountId { get; set; }


        public string AccountName { get; set; }


        public string AssessmentId { get; set; }


        public string AssessmentType { get; set; }


        public int? AssessmentTypeId { get; set; }


        public string CreatedBy { get; set; }


        public BsonDateTime CreatedUtc { get; set; }


        public string CreatedUtcLocale { get; set; }


        public string Description { get; set; }


        public BsonDateTime StartUtc { get; set; }


        public string StartUtcLocale { get; set; }


        public BsonDateTime EndUtc { get; set; }


        public string EndUtcLocale { get; set; }


        public string ModifiedBy { get; set; }


        public BsonDateTime ModifiedUtc { get; set; }


        public string ModifiedUtcLocale { get; set; }


        public string ModuleName { get; set; }


        public string Name { get; set; }
        public string SiteId { get; set; }

        public string SiteName { get; set; }


        public string Status { get; set; }


        public int? StatusId { get; set; }


        public string SubjectId { get; set; }


        public assessment_Movement Movement { get; set; }



        public assessment_Overview Overview { get; set; }
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