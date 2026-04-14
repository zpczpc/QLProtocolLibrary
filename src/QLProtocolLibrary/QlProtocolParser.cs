namespace QLProtocolLibrary
{
    using System;

    /// <summary>
    /// Parses QL protocol packets into structured objects.
    /// In real communication scenarios, callers should usually pass the raw received <see cref="byte"/> array directly.
    /// </summary>
    public static class QlProtocolParser
    {
        public static QlProtocolFrame Parse(byte[] frameBytes)
        {
            if (frameBytes == null)
            {
                throw new ArgumentNullException(nameof(frameBytes));
            }

            if (frameBytes.Length < QlProtocolConstants.BareMinimumFrameLength)
            {
                throw new QlProtocolException("Frame is shorter than the protocol minimum length.");
            }

            bool hasEnvelope = HasEnvelope(frameBytes);
            byte[] packetBytes = ExtractPacket(frameBytes, hasEnvelope);

            if (packetBytes.Length < QlProtocolConstants.BareMinimumFrameLength)
            {
                throw new QlProtocolException("Packet is shorter than the protocol minimum length.");
            }

            uint deviceAddress = DecodeDeviceAddress(packetBytes, 0);
            byte rawFunctionCode = packetBytes[QlProtocolConstants.DeviceAddressByteLength];
            ushort receivedCrc = DecodeCrc(packetBytes, packetBytes.Length - QlProtocolConstants.CrcByteLength);

            byte[] crcSource = new byte[packetBytes.Length - QlProtocolConstants.CrcByteLength];
            Array.Copy(packetBytes, crcSource, crcSource.Length);
            ushort computedCrc = QlProtocolCrc16.Compute(crcSource);

            byte[] functionData = new byte[packetBytes.Length - QlProtocolConstants.DeviceAddressByteLength - QlProtocolConstants.FunctionCodeByteLength - QlProtocolConstants.CrcByteLength];
            Array.Copy(packetBytes, QlProtocolConstants.DeviceAddressByteLength + QlProtocolConstants.FunctionCodeByteLength, functionData, 0, functionData.Length);

            ParsedBody body = ParseBody(rawFunctionCode, functionData);

            byte[] rawCopy = new byte[frameBytes.Length];
            Array.Copy(frameBytes, rawCopy, frameBytes.Length);

            return new QlProtocolFrame(
                rawCopy,
                hasEnvelope,
                deviceAddress,
                rawFunctionCode,
                body.Kind,
                body.Address,
                body.RegisterCount,
                body.Payload,
                body.ByteCount,
                body.DataLength,
                body.ResponseCode,
                receivedCrc,
                computedCrc);
        }

        /// <summary>
        /// Parses a packet represented as a hex string.
        /// This helper is mainly intended for debugging, tests, and documentation examples.
        /// </summary>
        public static QlProtocolFrame ParseHex(string hex)
        {
            return Parse(QlHexConverter.FromHexString(hex));
        }

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
        /// Tries to parse a packet represented as a hex string.
        /// This helper is mainly intended for debugging, tests, and documentation examples.
        /// </summary>
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
                    return QlFunctionCode.Write;
                case 0x08:
                    return QlFunctionCode.Operation;
                case 0x09:
                    return QlFunctionCode.Bootloader;
                case 0x23:
                    return QlFunctionCode.ReadLog;
                case 0x26:
                    return QlFunctionCode.WriteLog;
                case 0x30:
                    return QlFunctionCode.TfRead;
                case 0x32:
                    return QlFunctionCode.Forward;
                case 0x33:
                    return QlFunctionCode.Database;
                default:
                    return QlFunctionCode.Unknown;
            }
        }

        internal static byte[] EncodeDeviceAddressBytes(uint deviceAddress)
        {
            return new[]
            {
                (byte)((deviceAddress >> 24) & 0xFF),
                (byte)((deviceAddress >> 16) & 0xFF),
                (byte)((deviceAddress >> 8) & 0xFF),
                (byte)(deviceAddress & 0xFF)
            };
        }

        internal static uint DecodeDeviceAddress(byte[] data, int offset)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (offset < 0 || offset + 4 > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return ((uint)data[offset] << 24)
                | ((uint)data[offset + 1] << 16)
                | ((uint)data[offset + 2] << 8)
                | data[offset + 3];
        }

        private static bool HasEnvelope(byte[] frameBytes)
        {
            return frameBytes.Length >= 4
                && frameBytes[0] == QlProtocolConstants.EnvelopeHeader1
                && frameBytes[1] == QlProtocolConstants.EnvelopeHeader2
                && frameBytes[2] == QlProtocolConstants.EnvelopeHeader3
                && frameBytes[3] == QlProtocolConstants.EnvelopeHeader4;
        }

        private static byte[] ExtractPacket(byte[] frameBytes, bool hasEnvelope)
        {
            if (!hasEnvelope)
            {
                byte[] rawPacket = new byte[frameBytes.Length];
                Array.Copy(frameBytes, rawPacket, frameBytes.Length);
                return rawPacket;
            }

            if (frameBytes.Length < QlProtocolConstants.WrappedMinimumFrameLength)
            {
                throw new QlProtocolException("Wrapped frame is shorter than the protocol minimum length.");
            }

            ushort declaredLength = QlPayloadCodec.DecodeUInt16(frameBytes, 4);
            int expectedLength = 4 + 2 + declaredLength + 2;
            if (frameBytes.Length != expectedLength)
            {
                throw new QlProtocolException("Wrapped frame length does not match its declared length.");
            }

            if (frameBytes[frameBytes.Length - 2] != QlProtocolConstants.EnvelopeFooter1
                || frameBytes[frameBytes.Length - 1] != QlProtocolConstants.EnvelopeFooter2)
            {
                throw new QlProtocolException("Frame footer is invalid.");
            }

            byte[] wrappedPacket = new byte[declaredLength];
            Array.Copy(frameBytes, 6, wrappedPacket, 0, wrappedPacket.Length);
            return wrappedPacket;
        }

        private static ushort DecodeCrc(byte[] frameBytes, int offset)
        {
            return (ushort)(frameBytes[offset] | (frameBytes[offset + 1] << 8));
        }

        private static ParsedBody ParseBody(byte rawFunctionCode, byte[] functionData)
        {
            switch (rawFunctionCode)
            {
                case 0x03:
                    return ParseRead(functionData);
                case 0x06:
                    return ParseWrite(functionData);
                case 0x08:
                    return ParseOperation(functionData);
                case 0x23:
                case 0x26:
                case 0x30:
                case 0x32:
                case 0x33:
                    return ParseLengthPrefixed(functionData);
                case 0x09:
                    return new ParsedBody(QlProtocolFrameKind.BootloaderFrame, 0, null, functionData, null, (ushort)functionData.Length, null);
                default:
                    return new ParsedBody(QlProtocolFrameKind.Unknown, 0, null, functionData, null, null, null);
            }
        }

        private static ParsedBody ParseRead(byte[] functionData)
        {
            if (functionData.Length < 4)
            {
                throw new QlProtocolException("Read frame payload is too short.");
            }

            ushort address = QlPayloadCodec.DecodeUInt16(functionData, 0);
            if (functionData.Length == 4)
            {
                return new ParsedBody(
                    QlProtocolFrameKind.ReadRequest,
                    address,
                    QlPayloadCodec.DecodeUInt16(functionData, 2),
                    Array.Empty<byte>(),
                    null,
                    null,
                    null);
            }

            byte byteCount = functionData[2];
            if (functionData.Length != 3 + byteCount)
            {
                throw new QlProtocolException("Read response length does not match its byte count.");
            }

            if (byteCount == 1)
            {
                return new ParsedBody(
                    QlProtocolFrameKind.ErrorResponse,
                    address,
                    null,
                    Array.Empty<byte>(),
                    byteCount,
                    null,
                    functionData[3]);
            }

            byte[] payload = new byte[byteCount];
            Array.Copy(functionData, 3, payload, 0, payload.Length);
            ushort? registerCount = byteCount % QlProtocolConstants.RegisterByteLength == 0
                ? (ushort?)(byteCount / QlProtocolConstants.RegisterByteLength)
                : null;

            return new ParsedBody(QlProtocolFrameKind.ReadResponse, address, registerCount, payload, byteCount, null, null);
        }

        private static ParsedBody ParseWrite(byte[] functionData)
        {
            if (functionData.Length == 4)
            {
                ushort address = QlPayloadCodec.DecodeUInt16(functionData, 0);
                byte byteCount = functionData[2];
                return new ParsedBody(
                    QlProtocolFrameKind.WriteResponse,
                    address,
                    null,
                    Array.Empty<byte>(),
                    byteCount,
                    null,
                    functionData[3]);
            }

            if (functionData.Length < 5)
            {
                throw new QlProtocolException("Write frame payload is too short.");
            }

            ushort addressValue = QlPayloadCodec.DecodeUInt16(functionData, 0);
            ushort registerCountValue = QlPayloadCodec.DecodeUInt16(functionData, 2);
            byte byteCountValue = functionData[4];

            if (functionData.Length != 5 + byteCountValue)
            {
                throw new QlProtocolException("Write request length does not match its byte count.");
            }

            byte[] payload = new byte[byteCountValue];
            Array.Copy(functionData, 5, payload, 0, payload.Length);
            return new ParsedBody(QlProtocolFrameKind.WriteRequest, addressValue, registerCountValue, payload, byteCountValue, null, null);
        }

        private static ParsedBody ParseOperation(byte[] functionData)
        {
            if (functionData.Length < 3)
            {
                throw new QlProtocolException("Operation frame payload is too short.");
            }

            byte byteCount = functionData[0];
            if (functionData.Length != 1 + byteCount)
            {
                throw new QlProtocolException("Operation frame length does not match its byte count.");
            }

            byte[] payload = new byte[functionData.Length - 1];
            Array.Copy(functionData, 1, payload, 0, payload.Length);
            if (functionData.Length == 3)
            {
                return new ParsedBody(QlProtocolFrameKind.OperationResponse, 0, null, payload, byteCount, null, functionData[2]);
            }

            return new ParsedBody(QlProtocolFrameKind.OperationRequest, 0, null, payload, byteCount, null, null);
        }

        private static ParsedBody ParseLengthPrefixed(byte[] functionData)
        {
            if (functionData.Length < 2)
            {
                throw new QlProtocolException("Length-prefixed frame payload is too short.");
            }

            ushort dataLength = QlPayloadCodec.DecodeUInt16(functionData, 0);
            if (functionData.Length != 2 + dataLength)
            {
                throw new QlProtocolException("Length-prefixed frame length does not match its declared payload length.");
            }

            byte[] payload = new byte[dataLength];
            Array.Copy(functionData, 2, payload, 0, payload.Length);
            return new ParsedBody(QlProtocolFrameKind.LengthPrefixedFrame, 0, null, payload, null, dataLength, null);
        }

        private sealed class ParsedBody
        {
            public ParsedBody(
                QlProtocolFrameKind kind,
                ushort address,
                ushort? registerCount,
                byte[] payload,
                byte? byteCount,
                ushort? dataLength,
                byte? responseCode)
            {
                Kind = kind;
                Address = address;
                RegisterCount = registerCount;
                Payload = payload;
                ByteCount = byteCount;
                DataLength = dataLength;
                ResponseCode = responseCode;
            }

            public QlProtocolFrameKind Kind { get; }

            public ushort Address { get; }

            public ushort? RegisterCount { get; }

            public byte[] Payload { get; }

            public byte? ByteCount { get; }

            public ushort? DataLength { get; }

            public byte? ResponseCode { get; }
        }
    }
}
