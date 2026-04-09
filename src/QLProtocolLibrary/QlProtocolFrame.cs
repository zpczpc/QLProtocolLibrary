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
            ulong mn,
            byte rawFunctionCode,
            QlProtocolFrameKind kind,
            ushort address,
            ushort? registerCount,
            byte[] payload,
            byte? byteCount,
            byte? errorCode,
            ushort crc,
            ushort computedCrc)
        {
            RawBytes = rawBytes ?? throw new System.ArgumentNullException(nameof(rawBytes));
            Mn = mn;
            RawFunctionCode = rawFunctionCode;
            FunctionCode = QlProtocolParser.ToFunctionCode(rawFunctionCode);
            Kind = kind;
            Address = address;
            RegisterCount = registerCount;
            Payload = payload ?? System.Array.Empty<byte>();
            ByteCount = byteCount;
            ErrorCode = errorCode;
            Crc = crc;
            ComputedCrc = computedCrc;
        }

        /// <summary>
        /// Gets the original complete frame bytes.
        /// </summary>
        public byte[] RawBytes { get; }

        /// <summary>
        /// Gets the numeric MN value.
        /// </summary>
        public ulong Mn { get; }

        /// <summary>
        /// Gets the MN value as plain text.
        /// </summary>
        public string MnText => Mn.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the raw function code byte.
        /// </summary>
        public byte RawFunctionCode { get; }

        /// <summary>
        /// Gets the function code as an enum.
        /// </summary>
        public QlFunctionCode FunctionCode { get; }

        /// <summary>
        /// Gets the parsed frame kind.
        /// </summary>
        public QlProtocolFrameKind Kind { get; }

        /// <summary>
        /// Gets the register address.
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Gets the register count when it can be inferred from the frame.
        /// </summary>
        public ushort? RegisterCount { get; }

        /// <summary>
        /// Gets the business payload bytes.
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        /// Gets the payload byte count when it exists in the frame.
        /// </summary>
        public byte? ByteCount { get; }

        /// <summary>
        /// Gets the error code for error frames.
        /// </summary>
        public byte? ErrorCode { get; }

        /// <summary>
        /// Gets the CRC value carried by the frame.
        /// </summary>
        public ushort Crc { get; }

        /// <summary>
        /// Gets the CRC value computed from the parsed frame body.
        /// </summary>
        public ushort ComputedCrc { get; }

        /// <summary>
        /// Gets whether the received CRC matches the computed CRC.
        /// </summary>
        public bool IsCrcValid => Crc == ComputedCrc;

        /// <summary>
        /// Converts the full frame to a hex string.
        /// </summary>
        /// <param name="withSpaces">Whether to include spaces between bytes.</param>
        /// <returns>A hex string representation of the full frame.</returns>
        public string ToHexString(bool withSpaces = true)
        {
            return QlHexConverter.ToHexString(RawBytes, withSpaces);
        }
    }
}
