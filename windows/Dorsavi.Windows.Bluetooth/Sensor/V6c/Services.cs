using Ardalis.SmartEnum;
using System;
using System.Linq;

namespace Dorsavi.Windows.Bluetooth.Sensor.V6c
{
    public class V6CServiceUuId : SmartEnum<V6CServiceUuId>
    {
        public string UUid { get; }

        private V6CServiceUuId(string name, string value, int id) : base(name, id)
        {
            this.UUid = value;
        }

        public new static V6CServiceUuId FromUuidValue(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                throw new ArgumentNullException(nameof(uuid));
            }
            return List.FirstOrDefault(x => x.UUid == uuid);
        }

        public static readonly V6CServiceUuId GenericAccess = new V6CServiceUuId("GenericAccess", "00001800-0000-1000-8000-00805f9b34fb", 1);
        public static readonly V6CServiceUuId DeviceInformation = new V6CServiceUuId("DeviceInformation", "0000180a-0000-1000-8000-00805f9b34fb", 2);
        public static readonly V6CServiceUuId HeartRate = new V6CServiceUuId("HeartRate", "0000180d-0000-1000-8000-00805f9b34fb", 3);
        public static readonly V6CServiceUuId Custom = new V6CServiceUuId("Custom", "00000005-0008-a8ba-e311-f48c90364d99", 4);
    }
}