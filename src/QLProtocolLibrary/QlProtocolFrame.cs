namespace QLProtocolLibrary
{
    using System.Globalization;

    /// <summary>
    /// Represents a parsed QL protocol frame.
    /// </summary>
    public sealed class QlProtocolFrame
    {
        internal QlProtocolFrame(
            byte[] rawBytes,
            bool hasEnvelope,
            uint deviceAddress,
            byte rawFunctionCode,
            QlProtocolFrameKind kind,
            ushort address,
            ushort? registerCount,
            byte[] payload,
            byte? byteCount,
            ushort? dataLength,
            byte? responseCode,
            ushort crc,
            ushort computedCrc)
        {
            RawBytes = rawBytes ?? throw new System.ArgumentNullException(nameof(rawBytes));
            HasEnvelope = hasEnvelope;
            DeviceAddress = deviceAddress;
            RawFunctionCode = rawFunctionCode;
            FunctionCode = QlProtocolParser.ToFunctionCode(rawFunctionCode);
            Kind = kind;
            Address = address;
            RegisterCount = registerCount;
            Payload = payload ?? System.Array.Empty<byte>();
            ByteCount = byteCount;
            DataLength = dataLength;
            ResponseCode = responseCode;
            Crc = crc;
            ComputedCrc = computedCrc;
        }

        public byte[] RawBytes { get; }

        public bool HasEnvelope { get; }

        public uint DeviceAddress { get; }

        public string DeviceAddressHex =>
            ((DeviceAddress >> 24) & 0xFF).ToString("X2", CultureInfo.InvariantCulture) + " "
            + ((DeviceAddress >> 16) & 0xFF).ToString("X2", CultureInfo.InvariantCulture) + " "
            + ((DeviceAddress >> 8) & 0xFF).ToString("X2", CultureInfo.InvariantCulture) + " "
            + (DeviceAddress & 0xFF).ToString("X2", CultureInfo.InvariantCulture);

        public byte RawFunctionCode { get; }

        public QlFunctionCode FunctionCode { get; }

        public QlProtocolFrameKind Kind { get; }

        public ushort Address { get; }

        public ushort? RegisterCount { get; }

        public byte[] Payload { get; }

        public byte? ByteCount { get; }

        public ushort? DataLength { get; }

        public byte? ResponseCode { get; }

        public byte? ErrorCode => ResponseCode;

        public ushort Crc { get; }

        public ushort ComputedCrc { get; }

        public bool IsCrcValid => Crc == ComputedCrc;

        public string ToHexString(bool withSpaces = true)
        {
            return QlHexConverter.ToHexString(RawBytes, withSpaces);
        }
    }
}
