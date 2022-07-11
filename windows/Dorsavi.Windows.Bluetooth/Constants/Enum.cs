using Ardalis.SmartEnum;

namespace Dorsavi.Win.Bluetooth.Constants
{
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

    /// <summary>
    ///     This enum assists in finding a string representation of a BT SIG assigned value for Service UUIDS
    ///     Reference: https://developer.bluetooth.org/gatt/services/Pages/ServicesHome.aspx
    /// </summary>
    public enum GattNativeServiceUuid : ushort
    {
        None = 0,
        AlertNotification = 0x1811,
        Battery = 0x180F,
        BloodPressure = 0x1810,
        CurrentTimeService = 0x1805,
        CyclingSpeedandCadence = 0x1816,
        DeviceInformation = 0x180A,
        GenericAccess = 0x1800,
        GenericAttribute = 0x1801,
        Glucose = 0x1808,
        HealthThermometer = 0x1809,
        HeartRate = 0x180D,
        HumanInterfaceDevice = 0x1812,
        ImmediateAlert = 0x1802,
        LinkLoss = 0x1803,
        NextDSTChange = 0x1807,
        PhoneAlertStatus = 0x180E,
        ReferenceTimeUpdateService = 0x1806,
        RunningSpeedandCadence = 0x1814,
        ScanParameters = 0x1813,
        TxPower = 0x1804,
        SimpleKeyService = 0xFFE0
    }

    /// <summary>
    ///     This enum is nice for finding a string representation of a BT SIG assigned value for Characteristic UUIDs
    ///     Reference: https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicsHome.aspx
    /// </summary>
    public enum GattNativeCharacteristicUuid : ushort
    {
        None = 0,
        AlertCategoryID = 0x2A43,
        AlertCategoryIDBitMask = 0x2A42,
        AlertLevel = 0x2A06,
        AlertNotificationControlPoint = 0x2A44,
        AlertStatus = 0x2A3F,
        Appearance = 0x2A01,
        BatteryLevel = 0x2A19,
        BloodPressureFeature = 0x2A49,
        BloodPressureMeasurement = 0x2A35,
        BodySensorLocation = 0x2A38,
        BootKeyboardInputReport = 0x2A22,
        BootKeyboardOutputReport = 0x2A32,
        BootMouseInputReport = 0x2A33,
        CSCFeature = 0x2A5C,
        CSCMeasurement = 0x2A5B,
        CurrentTime = 0x2A2B,
        DateTime = 0x2A08,
        DayDateTime = 0x2A0A,
        DayofWeek = 0x2A09,
        DeviceName = 0x2A00,
        DSTOffset = 0x2A0D,
        ExactTime256 = 0x2A0C,
        FirmwareRevisionString = 0x2A26,
        GlucoseFeature = 0x2A51,
        GlucoseMeasurement = 0x2A18,
        GlucoseMeasurementContext = 0x2A34,
        HardwareRevisionString = 0x2A27,
        HeartRateControlPoint = 0x2A39,
        HeartRateMeasurement = 0x2A37,
        HIDControlPoint = 0x2A4C,
        HIDInformation = 0x2A4A,
        IEEE11073_20601RegulatoryCertificationDataList = 0x2A2A,
        IntermediateCuffPressure = 0x2A36,
        IntermediateTemperature = 0x2A1E,
        LocalTimeInformation = 0x2A0F,
        ManufacturerNameString = 0x2A29,
        MeasurementInterval = 0x2A21,
        ModelNumberString = 0x2A24,
        NewAlert = 0x2A46,
        PeripheralPreferredConnectionParameters = 0x2A04,
        PeripheralPrivacyFlag = 0x2A02,
        PnPID = 0x2A50,
        ProtocolMode = 0x2A4E,
        ReconnectionAddress = 0x2A03,
        RecordAccessControlPoint = 0x2A52,
        ReferenceTimeInformation = 0x2A14,
        Report = 0x2A4D,
        ReportMap = 0x2A4B,
        RingerControlPoint = 0x2A40,
        RingerSetting = 0x2A41,
        RSCFeature = 0x2A54,
        RSCMeasurement = 0x2A53,
        SCControlPoint = 0x2A55,
        ScanIntervalWindow = 0x2A4F,
        ScanRefresh = 0x2A31,
        SensorLocation = 0x2A5D,
        SerialNumberString = 0x2A25,
        ServiceChanged = 0x2A05,
        SoftwareRevisionString = 0x2A28,
        SupportedNewAlertCategory = 0x2A47,
        SupportedUnreadAlertCategory = 0x2A48,
        SystemID = 0x2A23,
        TemperatureMeasurement = 0x2A1C,
        TemperatureType = 0x2A1D,
        TimeAccuracy = 0x2A12,
        TimeSource = 0x2A13,
        TimeUpdateControlPoint = 0x2A16,
        TimeUpdateState = 0x2A17,
        TimewithDST = 0x2A11,
        TimeZone = 0x2A0E,
        TxPowerLevel = 0x2A07,
        UnreadAlertStatus = 0x2A45,
        AggregateInput = 0x2A5A,
        AnalogInput = 0x2A58,
        AnalogOutput = 0x2A59,
        CyclingPowerControlPoint = 0x2A66,
        CyclingPowerFeature = 0x2A65,
        CyclingPowerMeasurement = 0x2A63,
        CyclingPowerVector = 0x2A64,
        DigitalInput = 0x2A56,
        DigitalOutput = 0x2A57,
        ExactTime100 = 0x2A0B,
        LNControlPoint = 0x2A6B,
        LNFeature = 0x2A6A,
        LocationandSpeed = 0x2A67,
        Navigation = 0x2A68,
        NetworkAvailability = 0x2A3E,
        PositionQuality = 0x2A69,
        ScientificTemperatureinCelsius = 0x2A3C,
        SecondaryTimeZone = 0x2A10,
        String = 0x2A3D,
        TemperatureinCelsius = 0x2A1F,
        TemperatureinFahrenheit = 0x2A20,
        TimeBroadcast = 0x2A15,
        BatteryLevelState = 0x2A1B,
        BatteryPowerState = 0x2A1A,
        PulseOximetryContinuousMeasurement = 0x2A5F,
        PulseOximetryControlPoint = 0x2A62,
        PulseOximetryFeatures = 0x2A61,
        PulseOximetryPulsatileEvent = 0x2A60,
        SimpleKeyState = 0xFFE1
    }

    /// <summary>
    ///     This enum assists in finding a string representation of a BT SIG assigned value for Descriptor UUIDs
    ///     Reference: https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx
    /// </summary>
    public enum GattNativeDescriptorUuid : ushort
    {
        CharacteristicExtendedProperties = 0x2900,
        CharacteristicUserDescription = 0x2901,
        ClientCharacteristicConfiguration = 0x2902,
        ServerCharacteristicConfiguration = 0x2903,
        CharacteristicPresentationFormat = 0x2904,
        CharacteristicAggregateFormat = 0x2905,
        ValidRange = 0x2906,
        ExternalReportReference = 0x2907,
        ReportReference = 0x2908
    }
}
