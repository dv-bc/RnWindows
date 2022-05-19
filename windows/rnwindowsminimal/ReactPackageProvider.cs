using Microsoft.ReactNative;
using Microsoft.ReactNative.Managed;

namespace rnwindowsminimal
{
    public partial class ReactPackageProvider : IReactPackageProvider
    {
        public void CreatePackage(IReactPackageBuilder packageBuilder)
        {

            // not sure if we need to add this
            packageBuilder.AddAttributedModules();
            CreatePackageImplementation(packageBuilder);
        }

        /// <summary>
        /// This method is implemented by the C# code generator
        /// </summary>
        partial void CreatePackageImplementation(IReactPackageBuilder packageBuilder);
    }
}
