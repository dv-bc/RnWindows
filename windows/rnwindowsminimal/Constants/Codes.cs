using Ardalis.SmartEnum;
using System;

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

    public class Codes
    {
        public static readonly Guid CalcServiceUuid = Guid.Parse("caecface-e1d9-11e6-bf01-fe55135034f0");
        public static readonly Guid Op1CharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f1");
        public static readonly Guid Op2CharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f2");
        public static readonly Guid OperatorCharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f3");
        public static readonly Guid ResultCharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f4");
    }

    public class V6CCharacteristicUuId : SmartEnum<V6CCharacteristicUuId>
    {
        public string UUid { get; }

        private V6CCharacteristicUuId(string name, string value, int id) : base(name, id)
        {
            this.UUid = value;
        }

        public static readonly V6CCharacteristicUuId Primary = new V6CCharacteristicUuId("Primary", "0x180D", 1);
        public static readonly V6CCharacteristicUuId Secondary = new V6CCharacteristicUuId("Secondary", "00000005-0008-a8ba-e311-f48c90364d99", 2);
        public static readonly V6CCharacteristicUuId CMD = new V6CCharacteristicUuId("CMD", "00000006-0008-a8ba-e311-f48c90364d99", 3);
        public static readonly V6CCharacteristicUuId LED = new V6CCharacteristicUuId("LED", "00000007-0008-a8ba-e311-f48c90364d99", 4);
        public static readonly V6CCharacteristicUuId MAG_COEF = new V6CCharacteristicUuId("MAG_COEF", "00000008-0008-a8ba-e311-f48c90364d99", 5);

        public static readonly V6CCharacteristicUuId STREAMING_DATA = new V6CCharacteristicUuId("STREAMING_DATA", "00000010-0008-a8ba-e311-f48c90364d99", 6);
        public static readonly V6CCharacteristicUuId BOA_CONTROL = new V6CCharacteristicUuId("BOA_CONTROL", "00000011-0008-a8ba-e311-f48c90364d99", 7);
        public static readonly V6CCharacteristicUuId BOA_DIP_COMP_ACCEL_OFFLOAD = new V6CCharacteristicUuId("BOA_DIP_COMP_ACCEL_OFFLOAD", "00000012-0008-a8ba-e311-f48c90364d99", 8);
        public static readonly V6CCharacteristicUuId ACCEL_COEF = new V6CCharacteristicUuId("ACCEL_COEF", "00000014-0008-a8ba-e311-f48c90364d99", 9);
        public static readonly V6CCharacteristicUuId GYRO_COEF = new V6CCharacteristicUuId("GYRO_COEF", "00000015-0008-a8ba-e311-f48c90364d99", 10);

        public static readonly V6CCharacteristicUuId HIGHG_ACCEL_COEF = new V6CCharacteristicUuId("HIGHG_ACCEL_COEF", "00000016-0008-a8ba-e311-f48c90364d99", 11);
        public static readonly V6CCharacteristicUuId HIGHSPEED_GYRO_COEF = new V6CCharacteristicUuId("HIGHSPEED_GYRO_COEF", "00000017-0008-a8ba-e311-f48c90364d99", 12);
        public static readonly V6CCharacteristicUuId EVENT_INFO = new V6CCharacteristicUuId("EVENT_INFO", "00000018-0008-a8ba-e311-f48c90364d99", 13);
        public static readonly V6CCharacteristicUuId MAG_COEF_RESERVED_RIG = new V6CCharacteristicUuId("MAG_COEF_RESERVED_RIG", "00000019-0008-a8ba-e311-f48c90364d99", 14);
        public static readonly V6CCharacteristicUuId SAMPLING_FREQUENCY = new V6CCharacteristicUuId("SAMPLING_FREQUENCY", "0000000a-0008-a8ba-e311-f48c90364d99", 15);

        public static readonly V6CCharacteristicUuId DEVICEID = new V6CCharacteristicUuId("DEVICEID", "0000000b-0008-a8ba-e311-f48c90364d99", 16);
        public static readonly V6CCharacteristicUuId SESSION_CONTROL = new V6CCharacteristicUuId("SESSION_CONTROL", "0000000c-0008-a8ba-e311-f48c90364d99", 17);
        public static readonly V6CCharacteristicUuId STATISTICS = new V6CCharacteristicUuId("STATISTICS", "0000000e-0008-a8ba-e311-f48c90364d99", 18);
        public static readonly V6CCharacteristicUuId CALIBERATE_TIME = new V6CCharacteristicUuId("CALIBERATE_TIME", "0000000f-0008-a8ba-e311-f48c90364d99", 19);
        public static readonly V6CCharacteristicUuId BODY_LOCATION = new V6CCharacteristicUuId("BODY_LOCATION", "0000001A-0008-a8ba-e311-f48c90364d99", 20);
    }
}