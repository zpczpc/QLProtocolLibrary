namespace QLProtocolLibrary
{
    using System;

    /// <summary>
    /// Provides high-level command builders that hide register addresses from callers.
    /// </summary>
    public static class QlProtocolKnownCommands
    {
        public static byte[] BuildReadDeviceNo(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.DeviceNo);

        public static byte[] BuildReadAnalyzerCode(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.AnalyzerCode);

        public static byte[] BuildReadDeviceTime(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.DeviceTime);

        public static byte[] BuildReadRunStatus(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.RunStatus);

        public static byte[] BuildReadMeasureResult(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.MeasureResult);

        public static byte[] BuildReadConcentration(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.Concentration);

        public static byte[] BuildReadKbInfo(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.KbInfo);

        public static byte[] BuildReadMeterStrongLight(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.MeterStrongLight);

        public static byte[] BuildReadVersionBundle(uint deviceAddress) => QlProtocolCommandBuilder.BuildRead(deviceAddress, QlKnownRegisters.VersionBundle);

        public static byte[] BuildSetDeviceTime(uint deviceAddress, DateTime value) => QlProtocolCommandBuilder.BuildSetTime(deviceAddress, value);

        public static byte[] BuildWriteDeviceNo(uint deviceAddress, string deviceNo, int fixedByteLength = 16) =>
            QlProtocolCommandBuilder.BuildWriteUtf8(deviceAddress, QlKnownRegisters.DeviceNo.Address, deviceNo, fixedByteLength);

        public static byte[] BuildWriteAnalyzerCode(uint deviceAddress, string analyzerCode, int fixedByteLength = 16) =>
            QlProtocolCommandBuilder.BuildWriteUtf8(deviceAddress, QlKnownRegisters.AnalyzerCode.Address, analyzerCode, fixedByteLength);

        public static string BuildReadDeviceNoHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadDeviceNo(deviceAddress));

        public static string BuildReadAnalyzerCodeHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadAnalyzerCode(deviceAddress));

        public static string BuildReadDeviceTimeHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadDeviceTime(deviceAddress));

        public static string BuildReadRunStatusHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadRunStatus(deviceAddress));

        public static string BuildReadMeasureResultHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadMeasureResult(deviceAddress));

        public static string BuildReadConcentrationHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadConcentration(deviceAddress));

        public static string BuildReadKbInfoHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadKbInfo(deviceAddress));

        public static string BuildReadMeterStrongLightHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadMeterStrongLight(deviceAddress));

        public static string BuildReadVersionBundleHex(uint deviceAddress) => QlHexConverter.ToHexString(BuildReadVersionBundle(deviceAddress));

        public static string BuildSetDeviceTimeHex(uint deviceAddress, DateTime value) => QlHexConverter.ToHexString(BuildSetDeviceTime(deviceAddress, value));
    }
}
