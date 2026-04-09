namespace QLProtocolLibrary
{
    using System;

    /// <summary>
    /// Parses complete QL protocol frames into structured objects.
    /// </summary>
    public static class QlProtocolParser
    {
        /// <summary>
        /// Parses a complete protocol frame.
        /// </summary>
        /// <param name="frameBytes">The full frame bytes including header, CRC, and footer.</param>
        /// <returns>A parsed <see cref="QlProtocolFrame"/>.</returns>
        /// <exception cref="QlProtocolException">Thrown when the frame format is invalid.</exception>
        public static QlProtocolFrame Parse(byte[] frameBytes)
        {
            if (frameBytes == null)
            {
                throw new ArgumentNullException(nameof(frameBytes));
            }

            if (frameBytes.Length < QlProtocolConstants.MinimumFrameLength)
            {
                throw new QlProtocolException("Frame is shorter than the protocol minimum length.");
            }

            if (frameBytes[0] != QlProtocolConstants.HeaderHigh || frameBytes[1] != QlProtocolConstants.HeaderLow)
            {
                throw new QlProtocolException("Frame header is invalid.");
            }

            if (frameBytes[frameBytes.Length - 2] != QlProtocolConstants.FooterHigh
                || frameBytes[frameBytes.Length - 1] != QlProtocolConstants.FooterLow)
            {
                throw new QlProtocolException("Frame footer is invalid.");
            }

            ulong mn = DecodeMn(frameBytes, 2);
            byte rawFunctionCode = frameBytes[10];
            ushort address = QlPayloadCodec.DecodeUInt16(frameBytes, 11);
            ushort receivedCrc = (ushort)((frameBytes[frameBytes.Length - 3] << 8) | frameBytes[frameBytes.Length - 4]);

            byte[] crcSource = new byte[frameBytes.Length - 6];
            Array.Copy(frameBytes, 2, crcSource, 0, crcSource.Length);
            ushort computedCrc = QlProtocolCrc16.Compute(crcSource);

            ParsedBody body = ParseBody(frameBytes, rawFunctionCode, address);

            byte[] rawCopy = new byte[frameBytes.Length];
            Array.Copy(frameBytes, rawCopy, frameBytes.Length);

            return new QlProtocolFrame(
                rawCopy,
                mn,
                rawFunctionCode,
                body.Kind,
                address,
                body.RegisterCount,
                body.Payload,
                body.ByteCount,
                body.ErrorCode,
                receivedCrc,
                computedCrc);
        }

        /// <summary>
        /// Parses a complete protocol frame represented as a space-separated hex string.
        /// </summary>
        /// <param name="hex">Hex string containing the full frame.</param>
        /// <returns>A parsed <see cref="QlProtocolFrame"/>.</returns>
        public static QlProtocolFrame ParseHex(string hex)
        {
            return Parse(QlHexConverter.FromHexString(hex));
        }

        /// <summary>
        /// Tries to parse a complete protocol frame without throwing on failure.
        /// </summary>
        /// <param name="frameBytes">The full frame bytes including header, CRC, and footer.</param>
        /// <param name="frame">Parsed frame when successful; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> when parsing succeeds; otherwise <c>false</c>.</returns>
        public static bool TryParse(byte[] frameBytes, out QlProtocolFrame? frame)
        {
            try
            {
                frame = Parse(frameBytes);
                return true;
            }
            catch
            {
                frame = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to parse a complete protocol frame from a hex string without throwing on failure.
        /// </summary>
        /// <param name="hex">Hex string containing the full frame.</param>
        /// <param name="frame">Parsed frame when successful; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> when parsing succeeds; otherwise <c>false</c>.</returns>
        public static bool TryParseHex(string hex, out QlProtocolFrame? frame)
        {
            try
            {
                frame = ParseHex(hex);
                return true;
            }
            catch
            {
                frame = null;
                return false;
            }
        }

        internal static QlFunctionCode ToFunctionCode(byte rawFunctionCode)
        {
            switch (rawFunctionCode)
            {
                case 0x03:
                    return QlFunctionCode.Read;
                case 0x06:
                    return QlFunctionCode.SingleWriteSuccess;
                case 0x10:
                    return QlFunctionCode.Write;
                case 0x83:
                    return QlFunctionCode.ReadFailed;
                case 0x86:
                    return QlFunctionCode.SingleWriteFailed;
                case 0x90:
                    return QlFunctionCode.WriteFailed;
                default:
                    return QlFunctionCode.Unknown;
            }
        }

        internal static byte[] EncodeMnBytes(ulong mn)
        {
            byte[] bytes = BitConverter.GetBytes(mn);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        internal static ulong ParseMn(string mnText)
        {
            if (string.IsNullOrWhiteSpace(mnText))
            {
                throw new QlProtocolException("MN cannot be empty.");
            }

            if (!ulong.TryParse(mnText, out ulong mn))
            {
                throw new QlProtocolException("MN must be a positive integer string.");
            }

            return mn;
        }

        private static ulong DecodeMn(byte[] frameBytes, int offset)
        {
            byte[] mnBytes = new byte[QlProtocolConstants.MnByteLength];
            Array.Copy(frameBytes, offset, mnBytes, 0, mnBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(mnBytes);
            }

            return BitConverter.ToUInt64(mnBytes, 0);
        }

        private static ParsedBody ParseBody(byte[] frameBytes, byte rawFunctionCode, ushort address)
        {
            switch (rawFunctionCode)
            {
                case 0x03:
                    return ParseRead(frameBytes);
                case 0x10:
                    return ParseWrite(frameBytes);
                case 0x06:
                    return ParseSingleWriteResponse(frameBytes);
                case 0x83:
                case 0x86:
                case 0x90:
                    return ParseError(frameBytes, address);
                default:
                    return ParseFallback(frameBytes);
            }
        }

        private static ParsedBody ParseRead(byte[] frameBytes)
        {
            if (frameBytes.Length == 19 && frameBytes[13] == 0x00)
            {
                return new ParsedBody(QlProtocolFrameKind.ReadRequest, QlPayloadCodec.DecodeUInt16(frameBytes, 13), Array.Empty<byte>(), null, null);
            }

            byte byteCount = frameBytes[13];
            int expectedLength = 18 + byteCount;
            if (frameBytes.Length != expectedLength)
            {
                throw new QlProtocolException("Read response length does not match its byte count.");
            }

            byte[] payload = new byte[byteCount];
            Array.Copy(frameBytes, 14, payload, 0, byteCount);
            ushort? registerCount = byteCount % 2 == 0 ? (ushort?)(byteCount / 2) : null;
            return new ParsedBody(QlProtocolFrameKind.ReadResponse, registerCount, payload, byteCount, null);
        }

        private static ParsedBody ParseWrite(byte[] frameBytes)
        {
            if (frameBytes.Length == 19)
            {
                return new ParsedBody(QlProtocolFrameKind.WriteResponse, QlPayloadCodec.DecodeUInt16(frameBytes, 13), Array.Empty<byte>(), null, null);
            }

            byte byteCount = frameBytes[15];
            int expectedLength = 20 + byteCount;
            if (frameBytes.Length != expectedLength)
            {
                throw new QlProtocolException("Write request length does not match its byte count.");
            }

            ushort registerCount = QlPayloadCodec.DecodeUInt16(frameBytes, 13);
            byte[] payload = new byte[byteCount];
            Array.Copy(frameBytes, 16, payload, 0, byteCount);
            return new ParsedBody(QlProtocolFrameKind.WriteRequest, registerCount, payload, byteCount, null);
        }

        private static ParsedBody ParseSingleWriteResponse(byte[] frameBytes)
        {
            if (frameBytes.Length != 19)
            {
                throw new QlProtocolException("Single write response must be 19 bytes long.");
            }

            byte[] payload = new byte[2];
            Array.Copy(frameBytes, 13, payload, 0, payload.Length);
            return new ParsedBody(QlProtocolFrameKind.SingleWriteResponse, 1, payload, 2, null);
        }

        private static ParsedBody ParseError(byte[] frameBytes, ushort address)
        {
            if (frameBytes.Length != 18)
            {
                throw new QlProtocolException("Error response must be 18 bytes long.");
            }

            byte errorCode = frameBytes[13];
            return new ParsedBody(QlProtocolFrameKind.ErrorResponse, null, Array.Empty<byte>(), 1, errorCode);
        }

        private static ParsedBody ParseFallback(byte[] frameBytes)
        {
            if (frameBytes.Length <= 17)
            {
                return new ParsedBody(QlProtocolFrameKind.Unknown, null, Array.Empty<byte>(), null, null);
            }

            int payloadLength = frameBytes.Length - 17;
            byte[] payload = new byte[payloadLength];
            Array.Copy(frameBytes, 13, payload, 0, payload.Length);
            return new ParsedBody(QlProtocolFrameKind.Unknown, null, payload, (byte)payload.Length, null);
        }

        private sealed class ParsedBody
        {
            public ParsedBody(QlProtocolFrameKind kind, ushort? registerCount, byte[] payload, byte? byteCount, byte? errorCode)
            {
                Kind = kind;
                RegisterCount = registerCount;
                Payload = payload;
                ByteCount = byteCount;
                ErrorCode = errorCode;
            }

            public QlProtocolFrameKind Kind { get; }

            public ushort? RegisterCount { get; }

            public byte[] Payload { get; }

            public byte? ByteCount { get; }

            public byte? ErrorCode { get; }
        }
    }
}
