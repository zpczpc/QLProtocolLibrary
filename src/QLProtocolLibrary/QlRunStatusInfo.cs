namespace QLProtocolLibrary
{
    using System;

    public sealed class QlRunStatusInfo
    {
        public ushort Status { get; set; }

        public ushort SubStatus { get; set; }

        public ushort RunMode { get; set; }

        public ushort MeasureMode { get; set; }

        public uint WarnCode { get; set; }

        public uint FaultCode { get; set; }

        public override string ToString()
        {
            return "Status=" + Status + ", SubStatus=" + SubStatus + ", RunMode=" + RunMode + ", MeasureMode=" + MeasureMode + ", WarnCode=" + WarnCode + ", FaultCode=" + FaultCode;
        }
    }
}
