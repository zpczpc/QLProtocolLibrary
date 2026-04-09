namespace QLProtocolLibrary
{
    public static class QlProtocolConstants
    {
        public const byte HeaderHigh = 0xAA;
        public const byte HeaderLow = 0x55;
        public const byte FooterHigh = 0xBB;
        public const byte FooterLow = 0x55;
        public const ushort SetTimeAddress = 0x00D0;
        public const ushort SetTimeRegisterCount = 3;
        public const int MnByteLength = 8;
        public const int MinimumFrameLength = 18;
    }
}
