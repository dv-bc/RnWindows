using System.Collections.Generic;

namespace Dorsavi.Windows.Bluetooth.Common
{
    public class PropertyHelper
    {
        /// <inheritdoc/>
        public T GetProperty<T>(string propertyName, IReadOnlyDictionary<string, object> properties)
        {
            object property;
            if (properties.TryGetValue(propertyName, out property))
            {
                try
                {
                    return (T)property;
                }
                catch
                {
                    // Should only fail if casting is incorrect.
                }
            }

            return default;
        }

    }
}
