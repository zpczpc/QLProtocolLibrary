namespace QLProtocolLibrary
{
    public sealed class QlMeterStrongLightInfo
    {
        public ushort Channel1 { get; set; }

        public ushort Channel2 { get; set; }

        public override string ToString()
        {
            return "Channel1=" + Channel1 + ", Channel2=" + Channel2;
        }
    }
}
