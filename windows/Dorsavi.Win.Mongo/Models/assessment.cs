using MongoDB.Bson;
using Realms;
using System;
using System.Collections.Generic;

namespace Dorsavi.Win.Mongo.Models
{
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


        public DateTimeOffset? CreatedUtc { get; set; }


        public string CreatedUtcLocale { get; set; }


        public string Description { get; set; }


        public DateTimeOffset? StartUtc { get; set; }


        public string StartUtcLocale { get; set; }


        public DateTimeOffset? EndUtc { get; set; }


        public string EndUtcLocale { get; set; }


        public string ModifiedBy { get; set; }


        public DateTimeOffset? ModifiedUtc { get; set; }


        public string ModifiedUtcLocale { get; set; }


        public string ModuleName { get; set; }


        public string Name { get; set; }
        [Required]
        public string SiteId { get; set; }

        public string SiteName { get; set; }


        public string Status { get; set; }


        public int? StatusId { get; set; }


        public string SubjectId { get; set; }


        public IList<assessment_Movement> Movement { get;  }



        public assessment_Overview Overview { get; set; }
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
}