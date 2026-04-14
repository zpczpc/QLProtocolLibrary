namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builds QL protocol frames directly from device address, function code, and payload bytes.
    /// </summary>
    public static class QlProtocolCommandBuilder
    {
        public static byte[] BuildPacket(uint deviceAddress, byte rawFunctionCode, byte[] functionData, bool includeEnvelope = false)
        {
            if (functionData == null)
            {
                throw new ArgumentNullException(nameof(functionData));
            }

            byte[] body = Concat(
                QlProtocolParser.EncodeDeviceAddressBytes(deviceAddress),
                new[] { rawFunctionCode },
                functionData);

            ushort crc = QlProtocolCrc16.Compute(body);
            byte[] packet = Concat(body, QlProtocolCrc16.GetBytesLowHigh(crc));

            return includeEnvelope ? Wrap(packet) : packet;
        }

        public static byte[] BuildRead(uint deviceAddress, ushort address, ushort registerCount, bool includeEnvelope = false)
        {
            return BuildPacket(
                deviceAddress,
                (byte)QlFunctionCode.Read,
                Concat(QlPayloadCodec.EncodeUInt16(address), QlPayloadCodec.EncodeUInt16(registerCount)),
                includeEnvelope);
        }

        public static byte[] BuildRead(uint deviceAddress, QlRegisterDefinition register, bool includeEnvelope = false)
        {
            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            return BuildRead(deviceAddress, register.Address, register.RegisterCount, includeEnvelope);
        }

        public static byte[] BuildWrite(uint deviceAddress, ushort address, ushort registerCount, byte[] payload, bool includeEnvelope = false)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.Length > byte.MaxValue)
            {
                throw new QlProtocolException("Payload is too large for a single frame.");
            }

            if (registerCount == 0)
            {
                throw new QlProtocolException("Register count must be greater than zero.");
            }

            if (payload.Length != registerCount * QlProtocolConstants.RegisterByteLength)
            {
                throw new QlProtocolException("Write payload length must equal registerCount * 4 bytes.");
            }

            return BuildPacket(
                deviceAddress,
                (byte)QlFunctionCode.Write,
                Concat(
                    QlPayloadCodec.EncodeUInt16(address),
                    QlPayloadCodec.EncodeUInt16(registerCount),
                    new[] { (byte)payload.Length },
                    payload),
                includeEnvelope);
        }

        public static byte[] BuildWrite(uint deviceAddress, QlRegisterDefinition register, byte[] payload, bool includeEnvelope = false)
        {
            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            return BuildWrite(deviceAddress, register.Address, register.RegisterCount, payload, includeEnvelope);
        }

        public static byte[] BuildWriteRegisters(uint deviceAddress, ushort address, params ushort[] registers)
        {
            if (registers == null)
            {
                throw new ArgumentNullException(nameof(registers));
            }

            List<byte> payload = new List<byte>(registers.Length * QlProtocolConstants.RegisterByteLength);
            for (int i = 0; i < registers.Length; i++)
            {
                payload.AddRange(QlPayloadCodec.EncodeUInt32(registers[i]));
            }

            return BuildWrite(deviceAddress, address, (ushort)registers.Length, payload.ToArray());
        }

        public static byte[] BuildWriteFloat(uint deviceAddress, ushort address, params float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            List<byte> payload = new List<byte>(values.Length * QlProtocolConstants.RegisterByteLength);
            for (int i = 0; i < values.Length; i++)
            {
                payload.AddRange(QlPayloadCodec.EncodeSingle(values[i]));
            }

            return BuildWrite(deviceAddress, address, (ushort)values.Length, payload.ToArray());
        }

        public static byte[] BuildWriteUtf8(uint deviceAddress, ushort address, string value, int fixedByteLength = 0)
        {
            byte[] payload = QlPayloadCodec.EncodeUtf8(value, fixedByteLength, padToEven: false);
            payload = PadToRegisterBoundary(payload);
            ushort registerCount = (ushort)(payload.Length / QlProtocolConstants.RegisterByteLength);
            return BuildWrite(deviceAddress, address, registerCount, payload);
        }

        public static byte[] BuildSetTime(uint deviceAddress, DateTime value)
        {
            byte[] payload = PadToRegisterBoundary(QlPayloadCodec.EncodeBcdDateTime(value));
            ushort registerCount = (ushort)(payload.Length / QlProtocolConstants.RegisterByteLength);
            return BuildWrite(deviceAddress, QlProtocolConstants.SetTimeAddress, registerCount, payload);
        }

        public static string BuildReadHex(uint deviceAddress, ushort address, ushort registerCount, bool includeEnvelope = false)
        {
            return QlHexConverter.ToHexString(BuildRead(deviceAddress, address, registerCount, includeEnvelope));
        }

        public static string BuildReadHex(uint deviceAddress, QlRegisterDefinition register, bool includeEnvelope = false)
        {
            return QlHexConverter.ToHexString(BuildRead(deviceAddress, register, includeEnvelope));
        }

        public static string BuildWriteHex(uint deviceAddress, ushort address, ushort registerCount, byte[] payload, bool includeEnvelope = false)
        {
            return QlHexConverter.ToHexString(BuildWrite(deviceAddress, address, registerCount, payload, includeEnvelope));
        }

        public static string BuildSetTimeHex(uint deviceAddress, DateTime value)
        {
            return QlHexConverter.ToHexString(BuildSetTime(deviceAddress, value));
        }

        private static byte[] Wrap(byte[] packet)
        {
            if (packet.Length > ushort.MaxValue)
            {
                throw new QlProtocolException("Wrapped packet is too large.");
            }

            return Concat(
                new[]
                {
                    QlProtocolConstants.EnvelopeHeader1,
                    QlProtocolConstants.EnvelopeHeader2,
                    QlProtocolConstants.EnvelopeHeader3,
                    QlProtocolConstants.EnvelopeHeader4
                },
                QlPayloadCodec.EncodeUInt16((ushort)packet.Length),
                packet,
                new[] { QlProtocolConstants.EnvelopeFooter1, QlProtocolConstants.EnvelopeFooter2 });
        }

        private static byte[] PadToRegisterBoundary(byte[] payload)
        {
            if (payload.Length == 0)
            {
                return Array.Empty<byte>();
            }

            int registerSize = QlProtocolConstants.RegisterByteLength;
            int targetLength = ((payload.Length + registerSize - 1) / registerSize) * registerSize;
            if (targetLength == payload.Length)
            {
                return payload;
            }

            byte[] result = new byte[targetLength];
            Array.Copy(payload, result, payload.Length);
            return result;
        }

        private static byte[] Concat(params byte[][] segments)
        {
            int totalLength = 0;
            for (int i = 0; i < segments.Length; i++)
            {
                totalLength += segments[i].Length;
            }

            byte[] result = new byte[totalLength];
            int offset = 0;
            for (int i = 0; i < segments.Length; i++)
            {
                Array.Copy(segments[i], 0, result, offset, segments[i].Length);
                offset += segments[i].Length;
            }

            return result;
        }
    }
}
