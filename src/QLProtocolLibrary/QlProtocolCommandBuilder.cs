namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builds complete QL protocol frames for common read and write scenarios.
    /// </summary>
    public static class QlProtocolCommandBuilder
    {
        /// <summary>
        /// Builds a read command from raw register information.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registerCount">Number of registers to read.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildRead(string mn, ushort address, ushort registerCount)
        {
            return BuildRead(QlProtocolParser.ParseMn(mn), address, registerCount);
        }

        /// <summary>
        /// Builds a read command from a known register definition.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="register">Known register metadata.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildRead(string mn, QlRegisterDefinition register)
        {
            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            return BuildRead(mn, register.Address, register.RegisterCount);
        }

        /// <summary>
        /// Builds a read command from a numeric MN value.
        /// </summary>
        /// <param name="mn">Device MN numeric value.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registerCount">Number of registers to read.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildRead(ulong mn, ushort address, ushort registerCount)
        {
            byte[] body = Concat(
                QlProtocolParser.EncodeMnBytes(mn),
                new[] { (byte)QlFunctionCode.Read },
                QlPayloadCodec.EncodeUInt16(address),
                QlPayloadCodec.EncodeUInt16(registerCount));

            return Wrap(body);
        }

        /// <summary>
        /// Builds a write command from raw register information and payload bytes.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registerCount">Number of target registers.</param>
        /// <param name="payload">Encoded payload bytes.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildWrite(string mn, ushort address, ushort registerCount, byte[] payload)
        {
            return BuildWrite(QlProtocolParser.ParseMn(mn), address, registerCount, payload);
        }

        /// <summary>
        /// Builds a write command from a known register definition and payload bytes.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="register">Known register metadata.</param>
        /// <param name="payload">Encoded payload bytes.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildWrite(string mn, QlRegisterDefinition register, byte[] payload)
        {
            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            return BuildWrite(mn, register.Address, register.RegisterCount, payload);
        }

        /// <summary>
        /// Builds a write command from a numeric MN value.
        /// </summary>
        /// <param name="mn">Device MN numeric value.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registerCount">Number of target registers.</param>
        /// <param name="payload">Encoded payload bytes.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildWrite(ulong mn, ushort address, ushort registerCount, byte[] payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.Length > byte.MaxValue)
            {
                throw new QlProtocolException("Payload is too large for a single frame.");
            }

            byte[] body = Concat(
                QlProtocolParser.EncodeMnBytes(mn),
                new[] { (byte)QlFunctionCode.Write },
                QlPayloadCodec.EncodeUInt16(address),
                QlPayloadCodec.EncodeUInt16(registerCount),
                new[] { (byte)payload.Length },
                payload);

            return Wrap(body);
        }

        /// <summary>
        /// Builds a write command from 16-bit register values.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registers">Register values in protocol order.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildWriteRegisters(string mn, ushort address, params ushort[] registers)
        {
            if (registers == null)
            {
                throw new ArgumentNullException(nameof(registers));
            }

            List<byte> payload = new List<byte>(registers.Length * 2);
            for (int i = 0; i < registers.Length; i++)
            {
                payload.AddRange(QlPayloadCodec.EncodeUInt16(registers[i]));
            }

            return BuildWrite(mn, address, (ushort)registers.Length, payload.ToArray());
        }

        /// <summary>
        /// Builds a write command from one or more floating-point values.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="values">Floating-point values to encode as payload.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildWriteFloat(string mn, ushort address, params float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            List<byte> payload = new List<byte>(values.Length * 4);
            for (int i = 0; i < values.Length; i++)
            {
                payload.AddRange(QlPayloadCodec.EncodeSingle(values[i]));
            }

            return BuildWrite(mn, address, (ushort)(values.Length * 2), payload.ToArray());
        }

        /// <summary>
        /// Builds a write command from a UTF-8 string payload.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="value">String to encode.</param>
        /// <param name="fixedByteLength">Optional fixed payload length. Use 0 to keep the natural encoded length.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildWriteUtf8(string mn, ushort address, string value, int fixedByteLength = 0)
        {
            byte[] payload = QlPayloadCodec.EncodeUtf8(value, fixedByteLength, padToEven: true);
            ushort registerCount = (ushort)(payload.Length / 2);
            return BuildWrite(mn, address, registerCount, payload);
        }

        /// <summary>
        /// Builds the standard device-time write command.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="value">Time value to encode in BCD format.</param>
        /// <returns>A complete protocol frame that can be sent directly over TCP.</returns>
        public static byte[] BuildSetTime(string mn, DateTime value)
        {
            byte[] payload = QlPayloadCodec.EncodeBcdDateTime(value);
            return BuildWrite(mn, QlProtocolConstants.SetTimeAddress, QlProtocolConstants.SetTimeRegisterCount, payload);
        }

        /// <summary>
        /// Builds a read command and returns it as a hex string.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registerCount">Number of registers to read.</param>
        /// <returns>A space-separated hex string.</returns>
        public static string BuildReadHex(string mn, ushort address, ushort registerCount)
        {
            return QlHexConverter.ToHexString(BuildRead(mn, address, registerCount));
        }

        /// <summary>
        /// Builds a read command from a known register definition and returns it as a hex string.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="register">Known register metadata.</param>
        /// <returns>A space-separated hex string.</returns>
        public static string BuildReadHex(string mn, QlRegisterDefinition register)
        {
            return QlHexConverter.ToHexString(BuildRead(mn, register));
        }

        /// <summary>
        /// Builds a write command and returns it as a hex string.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="address">Register start address.</param>
        /// <param name="registerCount">Number of target registers.</param>
        /// <param name="payload">Encoded payload bytes.</param>
        /// <returns>A space-separated hex string.</returns>
        public static string BuildWriteHex(string mn, ushort address, ushort registerCount, byte[] payload)
        {
            return QlHexConverter.ToHexString(BuildWrite(mn, address, registerCount, payload));
        }

        /// <summary>
        /// Builds the standard device-time write command and returns it as a hex string.
        /// </summary>
        /// <param name="mn">Device MN text.</param>
        /// <param name="value">Time value to encode in BCD format.</param>
        /// <returns>A space-separated hex string.</returns>
        public static string BuildSetTimeHex(string mn, DateTime value)
        {
            return QlHexConverter.ToHexString(BuildSetTime(mn, value));
        }

        private static byte[] Wrap(byte[] body)
        {
            ushort crc = QlProtocolCrc16.Compute(body);
            byte[] crcBytes = QlProtocolCrc16.GetBytesLowHigh(crc);

            return Concat(
                new[] { QlProtocolConstants.HeaderHigh, QlProtocolConstants.HeaderLow },
                body,
                crcBytes,
                new[] { QlProtocolConstants.FooterHigh, QlProtocolConstants.FooterLow });
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
