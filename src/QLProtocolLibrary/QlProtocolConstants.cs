namespace QLProtocolLibrary
{
    public static class QlProtocolConstants
    {
        public const byte EnvelopeHeader1 = 0xC6;
        public const byte EnvelopeHeader2 = 0xF4;
        public const byte EnvelopeHeader3 = 0xC2;
        public const byte EnvelopeHeader4 = 0xCC;
        public const byte EnvelopeFooter1 = 0x0D;
        public const byte EnvelopeFooter2 = 0x0A;

        public const int DeviceAddressByteLength = 4;
        public const int FunctionCodeByteLength = 1;
        public const int CrcByteLength = 2;
        public const int RegisterByteLength = 4;
        public const int BareMinimumFrameLength = DeviceAddressByteLength + FunctionCodeByteLength + CrcByteLength;
        public const int WrappedMinimumFrameLength = 4 + 2 + BareMinimumFrameLength + 2;

        public const ushort SetTimeAddress = 0x00D0;
        public const ushort SetTimeRegisterCount = 2;
    }
}
