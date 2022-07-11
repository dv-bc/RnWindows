namespace Dorsavi.Win.Bluetooth.Models
{
    public class BleDeviceCharacteristicDescriptor
    {
        public string DeviceId { get; set; }
        public string ServiceId { get; set; }
        public string CharacteristicUuid { get; set; }
        public string Value { get; set; }
        public bool SubscriptionState { get; set; }
    }

    public class ServiceName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


}
