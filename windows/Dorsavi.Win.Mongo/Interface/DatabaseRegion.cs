using Dorsavi.Win.Framework.Model;
using Dorsavi.Win.Mongo.Common;
using Realms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dorsavi.Win.Mongo.Interface
{
    public interface IDatabaseContext
    {
        Task<ServiceResponse> InitialiseAsync(string apiId, string apiKey, string partitionConfig, DbRegion region);
        bool AppliesTo(DbRegion dbRegion);

        ServiceResponse Create<T>(T data) where T : RealmObject;

        List<T> Read<T>() where T : RealmObject;

        T Read<T>(long Id) where T : RealmObject;

        void Update<T>(long Id) where T : RealmObject;

        void Delete<T>(long Id) where T : RealmObject;

    }

    public interface IDatabaseStrategy
    {
        Task<ServiceResponse> InitialiseDatabases(DbRegion region, string apiId, string apiKey, string partitionConfig);
        ServiceResponse Create<T>(DbRegion region, T data) where T : RealmObject;

        List<T> Read<T>(DbRegion region) where T : RealmObject;

        T Read<T>(DbRegion region, long Id) where T : RealmObject;

        void Update<T>(DbRegion region, long Id) where T : RealmObject;

        void Delete<T>(DbRegion region, long Id) where T : RealmObject;
    }
}
