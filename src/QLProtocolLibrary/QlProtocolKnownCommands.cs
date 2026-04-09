namespace QLProtocolLibrary
{
    using System;

    /// <summary>
    /// Provides high-level command builders that hide register addresses from callers.
    /// </summary>
    public static class QlProtocolKnownCommands
    {
        public static byte[] BuildReadDeviceNo(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.DeviceNo);

        public static byte[] BuildReadAnalyzerCode(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.AnalyzerCode);

        public static byte[] BuildReadDeviceTime(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.DeviceTime);

        public static byte[] BuildReadRunStatus(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.RunStatus);

        public static byte[] BuildReadMeasureResult(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.MeasureResult);

        public static byte[] BuildReadConcentration(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.Concentration);

        public static byte[] BuildReadKbInfo(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.KbInfo);

        public static byte[] BuildReadMeterStrongLight(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.MeterStrongLight);

        public static byte[] BuildReadVersionBundle(string mn) => QlProtocolCommandBuilder.BuildRead(mn, QlKnownRegisters.VersionBundle);

        public static byte[] BuildSetDeviceTime(string mn, DateTime value) => QlProtocolCommandBuilder.BuildSetTime(mn, value);

        public static byte[] BuildWriteDeviceNo(string mn, string deviceNo, int fixedByteLength = 16) =>
            QlProtocolCommandBuilder.BuildWriteUtf8(mn, QlKnownRegisters.DeviceNo.Address, deviceNo, fixedByteLength);

        public static byte[] BuildWriteAnalyzerCode(string mn, string analyzerCode, int fixedByteLength = 16) =>
            QlProtocolCommandBuilder.BuildWriteUtf8(mn, QlKnownRegisters.AnalyzerCode.Address, analyzerCode, fixedByteLength);

        public static string BuildReadDeviceNoHex(string mn) => QlHexConverter.ToHexString(BuildReadDeviceNo(mn));

        public static string BuildReadAnalyzerCodeHex(string mn) => QlHexConverter.ToHexString(BuildReadAnalyzerCode(mn));

        public static string BuildReadDeviceTimeHex(string mn) => QlHexConverter.ToHexString(BuildReadDeviceTime(mn));

        public static string BuildReadRunStatusHex(string mn) => QlHexConverter.ToHexString(BuildReadRunStatus(mn));

        public static string BuildReadMeasureResultHex(string mn) => QlHexConverter.ToHexString(BuildReadMeasureResult(mn));

        public static string BuildReadConcentrationHex(string mn) => QlHexConverter.ToHexString(BuildReadConcentration(mn));

        public static string BuildReadKbInfoHex(string mn) => QlHexConverter.ToHexString(BuildReadKbInfo(mn));

        public static string BuildReadMeterStrongLightHex(string mn) => QlHexConverter.ToHexString(BuildReadMeterStrongLight(mn));

        public static string BuildReadVersionBundleHex(string mn) => QlHexConverter.ToHexString(BuildReadVersionBundle(mn));

        public static string BuildSetDeviceTimeHex(string mn, DateTime value) => QlHexConverter.ToHexString(BuildSetDeviceTime(mn, value));
    }
}
