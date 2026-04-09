namespace QLProtocolLibrary
{
    using System;

    public sealed class QlKbInfo
    {
        public DateTime SampleTime { get; set; }

        public string MeasureNumberText { get; set; } = string.Empty;

        public int? MeasureNumber { get; set; }

        public float KValue { get; set; }

        public float BValue { get; set; }

        public float FValue { get; set; }

        public float AmendmentK { get; set; }

        public float AmendmentB { get; set; }

        public override string ToString()
        {
            return "SampleTime=" + SampleTime.ToString("yyyy-MM-dd HH:mm:ss") + ", MeasureNumber=" + MeasureNumberText + ", K=" + KValue + ", B=" + BValue + ", F=" + FValue + ", AmendmentK=" + AmendmentK + ", AmendmentB=" + AmendmentB;
        }
    }
}
