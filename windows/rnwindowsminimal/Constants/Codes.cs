using Ardalis.SmartEnum;

namespace rnwindowsminimal.Constants
{
    public class ErrorCodes
    {
        #region Error Codes

        public const int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        public const int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        public const int E_ACCESSDENIED = unchecked((int)0x80070005);
        public const int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)

        #endregion Error Codes
    }
    public class V6CServiceUuId : SmartEnum<V6CServiceUuId>
    {
        public string UUid { get; }

        private V6CServiceUuId(string name, string value, int id) : base(name, id)
        {
            this.UUid = value;
        }

        public static readonly V6CServiceUuId GenericAccess = new V6CServiceUuId("GenericAccess", "00001800-0000-1000-8000-00805f9b34fb", 1);
        public static readonly V6CServiceUuId DeviceInformation = new V6CServiceUuId("DeviceInformation", "0000180a-0000-1000-8000-00805f9b34fb", 2);
        public static readonly V6CServiceUuId HeartRate = new V6CServiceUuId("HeartRate", "0000180d-0000-1000-8000-00805f9b34fb", 3);
        public static readonly V6CServiceUuId Custom = new V6CServiceUuId("Custom", "00000005-0008-a8ba-e311-f48c90364d99", 4);


    }
    public class V6CCharacteristic : SmartEnum<V6CCharacteristic>
    {
        public string UUid { get; }

        private V6CCharacteristic(string name, string value, int id) : base(name, id)
        {
            this.UUid = value;
        }

        public static readonly V6CCharacteristic Primary = new V6CCharacteristic("Primary", "0x180D", 1);
        public static readonly V6CCharacteristic Secondary = new V6CCharacteristic("Secondary", "00000005-0008-a8ba-e311-f48c90364d99", 2);
        public static readonly V6CCharacteristic CMD = new V6CCharacteristic("CMD", "00000006-0008-a8ba-e311-f48c90364d99", 3);
        public static readonly V6CCharacteristic LED = new V6CCharacteristic("LED", "00000007-0008-a8ba-e311-f48c90364d99", 4);
        public static readonly V6CCharacteristic MAG_COEF = new V6CCharacteristic("MAG_COEF", "00000008-0008-a8ba-e311-f48c90364d99", 5);

        public static readonly V6CCharacteristic STREAMING_DATA = new V6CCharacteristic("STREAMING_DATA", "00000010-0008-a8ba-e311-f48c90364d99", 6);
        public static readonly V6CCharacteristic BOA_CONTROL = new V6CCharacteristic("BOA_CONTROL", "00000011-0008-a8ba-e311-f48c90364d99", 7);
        public static readonly V6CCharacteristic BOA_DIP_COMP_ACCEL_OFFLOAD = new V6CCharacteristic("BOA_DIP_COMP_ACCEL_OFFLOAD", "00000012-0008-a8ba-e311-f48c90364d99", 8);
        public static readonly V6CCharacteristic ACCEL_COEF = new V6CCharacteristic("ACCEL_COEF", "00000014-0008-a8ba-e311-f48c90364d99", 9);
        public static readonly V6CCharacteristic GYRO_COEF = new V6CCharacteristic("GYRO_COEF", "00000015-0008-a8ba-e311-f48c90364d99", 10);

        public static readonly V6CCharacteristic HIGHG_ACCEL_COEF = new V6CCharacteristic("HIGHG_ACCEL_COEF", "00000016-0008-a8ba-e311-f48c90364d99", 11);
        public static readonly V6CCharacteristic HIGHSPEED_GYRO_COEF = new V6CCharacteristic("HIGHSPEED_GYRO_COEF", "00000017-0008-a8ba-e311-f48c90364d99", 12);
        public static readonly V6CCharacteristic EVENT_INFO = new V6CCharacteristic("EVENT_INFO", "00000018-0008-a8ba-e311-f48c90364d99", 13);
        public static readonly V6CCharacteristic MAG_COEF_RESERVED_RIG = new V6CCharacteristic("MAG_COEF_RESERVED_RIG", "00000019-0008-a8ba-e311-f48c90364d99", 14);
        public static readonly V6CCharacteristic SAMPLING_FREQUENCY = new V6CCharacteristic("SAMPLING_FREQUENCY", "0000000a-0008-a8ba-e311-f48c90364d99", 15);

        public static readonly V6CCharacteristic DEVICEID = new V6CCharacteristic("DEVICEID", "0000000b-0008-a8ba-e311-f48c90364d99", 16);
        public static readonly V6CCharacteristic SESSION_CONTROL = new V6CCharacteristic("SESSION_CONTROL", "0000000c-0008-a8ba-e311-f48c90364d99", 17);
        public static readonly V6CCharacteristic STATISTICS = new V6CCharacteristic("STATISTICS", "0000000e-0008-a8ba-e311-f48c90364d99", 18);
        public static readonly V6CCharacteristic CALIBERATE_TIME = new V6CCharacteristic("CALIBERATE_TIME", "0000000f-0008-a8ba-e311-f48c90364d99", 19);
        public static readonly V6CCharacteristic BODY_LOCATION = new V6CCharacteristic("BODY_LOCATION", "0000001A-0008-a8ba-e311-f48c90364d99", 20);
    }

    public class CharacteristicPropertiesEnum : SmartEnum<CharacteristicPropertiesEnum>
    {
        public uint CharacteristicValue { get; }

        private CharacteristicPropertiesEnum(string name, uint value, int id) : base(name, id)
        {
            this.CharacteristicValue = value;
        }

        //
        // Summary:
        //     The characteristic doesn’t have any properties that apply.
        public static readonly CharacteristicPropertiesEnum None = new CharacteristicPropertiesEnum("None", 0x0u, 1);

        /// <summary>
        /// The characteristic supports broadcasting
        /// </summary>
        public static readonly CharacteristicPropertiesEnum Broadcast = new CharacteristicPropertiesEnum("Broadcast", 0x1u, 2);

        /// <summary>
        /// The characteristic is readable
        /// </summary>
        public static readonly CharacteristicPropertiesEnum Read = new CharacteristicPropertiesEnum("Read", 0x2u, 3);

        /// <summary>
        /// The characteristic supports Write Without Response
        /// </summary>
        public static readonly CharacteristicPropertiesEnum WriteWithoutResponse = new CharacteristicPropertiesEnum("WriteWithoutResponse", 0x4u, 4);

        /// <summary>
        /// The characteristic is writable
        /// </summary>

        public static readonly CharacteristicPropertiesEnum Write = new CharacteristicPropertiesEnum("Write", 0x8u, 5);
        /// <summary>
        /// The characteristic is notifiable
        /// </summary>
        public static readonly CharacteristicPropertiesEnum Notify = new CharacteristicPropertiesEnum("Notify", 0x10u, 6);

        /// <summary>
        /// The characteristic is indicatable
        /// </summary>
        public static readonly CharacteristicPropertiesEnum Indicate = new CharacteristicPropertiesEnum("Indicate", 0x20u, 7);

        /// <summary>
        /// The characteristic supports signed writes
        /// </summary>
        public static readonly CharacteristicPropertiesEnum AuthenticatedSignedWrites = new CharacteristicPropertiesEnum("AuthenticatedSignedWrites", 0x40u, 8);

        /// <summary>
        /// The ExtendedProperties Descriptor is present
        /// </summary>
        public static readonly CharacteristicPropertiesEnum ExtendedProperties = new CharacteristicPropertiesEnum("ExtendedProperties", 0x80u, 9);

        /// <summary>
        /// The characteristic supports reliable writes
        /// </summary>
        public static readonly CharacteristicPropertiesEnum ReliableWrites = new CharacteristicPropertiesEnum("ReliableWrites", 0x100u, 10);

        /// <summary>
        /// The characteristic has writable auxiliaries
        /// </summary>
        public static readonly CharacteristicPropertiesEnum WritableAuxiliaries = new CharacteristicPropertiesEnum("WritableAuxiliaries", 0x200u, 11);




    }
}