namespace Dorsavi.Win.Bluetooth.Constants
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
}
