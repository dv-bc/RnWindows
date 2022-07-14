using Ardalis.SmartEnum;

namespace Dorsavi.Win.Mongo.Common
{


    public class DbRegion : SmartEnum<DbRegion>
    {
        public DbRegion(string name, int value) : base(name, value)
        {
        }

        public static readonly DbRegion AU = new DbRegion("AU", 1);

        public static readonly DbRegion US = new DbRegion("US", 2);
    }


}
