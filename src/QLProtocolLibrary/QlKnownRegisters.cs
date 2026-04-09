namespace QLProtocolLibrary
{
    using System.Collections.Generic;

    public static class QlKnownRegisters
    {
        public static readonly QlRegisterDefinition DeviceNo =
            new QlRegisterDefinition(76, "76", "DeviceNo", 8, QlPayloadType.Utf8, "设备编号");

        public static readonly QlRegisterDefinition MeasureResult =
            new QlRegisterDefinition(94, "94", "MeasureResult", 44, QlPayloadType.Raw, "测量结果复合报文");

        public static readonly QlRegisterDefinition RunStatus =
            new QlRegisterDefinition(200, "200", "RunStatus", 15, QlPayloadType.Raw, "运行状态复合报文");

        public static readonly QlRegisterDefinition SubStatus =
            new QlRegisterDefinition(201, "201", "SubStatus", 1, QlPayloadType.UInt16, "子状态");

        public static readonly QlRegisterDefinition RunMode =
            new QlRegisterDefinition(202, "202", "RunMode", 1, QlPayloadType.UInt16, "运行模式");

        public static readonly QlRegisterDefinition MeasureMode =
            new QlRegisterDefinition(203, "203", "MeasureMode", 1, QlPayloadType.UInt16, "测量模式");

        public static readonly QlRegisterDefinition WarnCode =
            new QlRegisterDefinition(204, "204", "WarnCode", 2, QlPayloadType.UInt32, "告警信息");

        public static readonly QlRegisterDefinition FaultCode =
            new QlRegisterDefinition(205, "205", "FaultCode", 2, QlPayloadType.UInt32, "故障信息");

        public static readonly QlRegisterDefinition DeviceTime =
            new QlRegisterDefinition(208, "208", "DeviceTime", 3, QlPayloadType.BcdDateTime, "设备时间");

        public static readonly QlRegisterDefinition Concentration =
            new QlRegisterDefinition(238, "238", "Concentration", 2, QlPayloadType.Single, "单浮点参数");

        public static readonly QlRegisterDefinition WorkStateFlag =
            new QlRegisterDefinition(248, "248", "WorkStateFlag", 1, QlPayloadType.UInt16, "单字节状态或标志值");

        public static readonly QlRegisterDefinition KbInfo =
            new QlRegisterDefinition(312, "312", "KbInfo", 14, QlPayloadType.Raw, "K/B/F 标定参数复合报文");

        public static readonly QlRegisterDefinition MeterStrongLight =
            new QlRegisterDefinition(460, "460", "AbsorbLightIntensity", 2, QlPayloadType.UInt16Array, "吸收光强");

        public static readonly QlRegisterDefinition AnalyzerCode =
            new QlRegisterDefinition(464, "464", "AnalyzerCode", 8, QlPayloadType.Utf8, "仪表编号");

        public static readonly QlRegisterDefinition VersionBundle =
            new QlRegisterDefinition(709, "709", "VersionBundle", 112, QlPayloadType.Raw, "版本信息复合报文");

        public static readonly IReadOnlyList<QlRegisterDefinition> All = new[]
        {
            DeviceNo,
            MeasureResult,
            RunStatus,
            SubStatus,
            RunMode,
            MeasureMode,
            WarnCode,
            FaultCode,
            DeviceTime,
            Concentration,
            WorkStateFlag,
            KbInfo,
            MeterStrongLight,
            AnalyzerCode,
            VersionBundle
        };

        public static bool TryGet(ushort address, out QlRegisterDefinition? definition)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Address == address)
                {
                    definition = All[i];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public static QlRegisterDefinition GetOrCreateRaw(ushort address, ushort registerCount = 0, string? name = null)
        {
            if (TryGet(address, out QlRegisterDefinition? definition))
            {
                return definition!;
            }

            return new QlRegisterDefinition(address, address.ToString(), name ?? ("Register_" + address), registerCount, QlPayloadType.Raw);
        }
    }
}


