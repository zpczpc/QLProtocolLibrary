namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;

    public sealed class QlMeasureResultInfo
    {
        public string Component { get; set; } = string.Empty;

        public DateTime? MeasureTime { get; set; }

        public float Value { get; set; }

        public string Flag { get; set; } = string.Empty;

        public float Absorbancy { get; set; }

        public IReadOnlyList<float> Energies { get; set; } = Array.Empty<float>();

        public ushort WorkType { get; set; }

        public override string ToString()
        {
            return "Component=" + Component + ", MeasureTime=" + (MeasureTime.HasValue ? MeasureTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty) + ", Value=" + Value + ", Flag=" + Flag + ", Absorbancy=" + Absorbancy + ", WorkType=" + WorkType;
        }
    }
}
