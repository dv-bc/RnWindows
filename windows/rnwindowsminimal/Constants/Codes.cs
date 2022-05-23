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
        #endregion

    }
    public class Codes
    {
        public static readonly Guid CalcServiceUuid = Guid.Parse("caecface-e1d9-11e6-bf01-fe55135034f0");
        public static readonly Guid Op1CharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f1");
        public static readonly Guid Op2CharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f2");
        public static readonly Guid OperatorCharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f3");
        public static readonly Guid ResultCharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f4");
    }
}