using Ardalis.SmartEnum;
using System;
using System.Linq;

namespace Dorsavi.Windows.Bluetooth.Sensor.V6c
{



    public class V6CCharacteristic : SmartEnum<V6CCharacteristic>
    {
        public string UUid { get; }

        private V6CCharacteristic(string name, string value, int id) : base(name, id)
        {
            this.UUid = value;
        }

        public static new V6CCharacteristic FromUuidValue(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                throw new ArgumentNullException(nameof(uuid));
            }
            return List.FirstOrDefault(x => x.UUid == uuid);
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
}
