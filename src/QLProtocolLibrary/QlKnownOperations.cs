namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;

    public static class QlKnownOperations
    {
        public static readonly QlKnownReadOperation<string?> DeviceNo =
            new QlKnownReadOperation<string?>("DeviceNo", QlKnownRegisters.DeviceNo, QlProtocolKnownParsers.TryParseDeviceNo);

        public static readonly QlKnownReadOperation<string?> AnalyzerCode =
            new QlKnownReadOperation<string?>("AnalyzerCode", QlKnownRegisters.AnalyzerCode, QlProtocolKnownParsers.TryParseAnalyzerCode);

        public static readonly QlKnownReadOperation<DateTime> DeviceTime =
            new QlKnownReadOperation<DateTime>("DeviceTime", QlKnownRegisters.DeviceTime, QlProtocolKnownParsers.TryParseDeviceTime);

        public static readonly QlKnownReadOperation<float> Concentration =
            new QlKnownReadOperation<float>("Concentration", QlKnownRegisters.Concentration, QlProtocolKnownParsers.TryParseConcentration);

        public static readonly QlKnownReadOperation<QlRunStatusInfo?> RunStatus =
            new QlKnownReadOperation<QlRunStatusInfo?>("RunStatus", QlKnownRegisters.RunStatus, QlProtocolKnownParsers.TryParseRunStatus);

        public static readonly QlKnownReadOperation<QlMeasureResultInfo?> MeasureResult =
            new QlKnownReadOperation<QlMeasureResultInfo?>("MeasureResult", QlKnownRegisters.MeasureResult, QlProtocolKnownParsers.TryParseMeasureResult);

        public static readonly QlKnownReadOperation<QlKbInfo?> KbInfo =
            new QlKnownReadOperation<QlKbInfo?>("KbInfo", QlKnownRegisters.KbInfo, QlProtocolKnownParsers.TryParseKbInfo);

        public static readonly QlKnownReadOperation<QlMeterStrongLightInfo?> MeterStrongLight =
            new QlKnownReadOperation<QlMeterStrongLightInfo?>("MeterStrongLight", QlKnownRegisters.MeterStrongLight, QlProtocolKnownParsers.TryParseMeterStrongLight);

        public static readonly QlKnownReadOperation<QlVersionBundleInfo?> VersionBundle =
            new QlKnownReadOperation<QlVersionBundleInfo?>("VersionBundle", QlKnownRegisters.VersionBundle, QlProtocolKnownParsers.TryParseVersionBundle);

        public static readonly IReadOnlyList<IQlKnownOperation> All = new IQlKnownOperation[]
        {
            DeviceNo,
            AnalyzerCode,
            DeviceTime,
            Concentration,
            RunStatus,
            MeasureResult,
            KbInfo,
            MeterStrongLight,
            VersionBundle
        };
    }
}
