using Dorsavi.Windows.Bluetooth.Ble;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Dorsavi.Windows.Bluetooth.Sensor.V6c
{
    public class V6cDeviceServices : BleDeviceServices
    {
        public V6cDeviceServices(GattDeviceService gattDeviceService, string deviceId) : base(gattDeviceService, deviceId)
        {
        }

        public override string Name => V6CServiceUuId.FromUuidValue(UUid)?.Name ?? base.Name;
    }

    public class V6cDeviceCharacteristic : BleDeviceCharacteristic
    {
        public V6cDeviceCharacteristic(GattCharacteristic gattCharacteristic, string deviceId, string serviceId) : base(gattCharacteristic, deviceId, serviceId)
        {
        }

        public override string Name => V6CCharacteristic.FromUuidValue(Uuid.ToString())?.Name ?? base.Name;
    }
}