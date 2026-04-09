namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides strongly typed payload readers for parsed protocol frames.
    /// </summary>
    public static class QlProtocolFrameExtensions
    {
        /// <summary>
        /// Reads the payload as a 16-bit unsigned integer.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="payloadOffset">Byte offset inside the payload.</param>
        /// <returns>The decoded 16-bit unsigned integer.</returns>
        public static ushort ReadUInt16(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeUInt16(GetPayload(frame), payloadOffset);
        }

        /// <summary>
        /// Reads the payload as a 32-bit unsigned integer.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="payloadOffset">Byte offset inside the payload.</param>
        /// <returns>The decoded 32-bit unsigned integer.</returns>
        public static uint ReadUInt32(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeUInt32(GetPayload(frame), payloadOffset);
        }

        /// <summary>
        /// Reads the payload as a single floating-point value.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="payloadOffset">Byte offset inside the payload.</param>
        /// <returns>The decoded floating-point value.</returns>
        public static float ReadSingle(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeSingle(GetPayload(frame), payloadOffset);
        }

        /// <summary>
        /// Reads the entire payload as a sequence of floating-point values.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <returns>A read-only list of floating-point values.</returns>
        public static IReadOnlyList<float> ReadSingles(this QlProtocolFrame frame)
        {
            return QlPayloadCodec.DecodeSingles(GetPayload(frame));
        }

        /// <summary>
        /// Reads the payload as a UTF-8 string.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="trimTrailingNulls">Whether to remove trailing null characters.</param>
        /// <returns>The decoded string.</returns>
        public static string ReadUtf8(this QlProtocolFrame frame, bool trimTrailingNulls = true)
        {
            return QlPayloadCodec.DecodeUtf8(GetPayload(frame), trimTrailingNulls);
        }

        /// <summary>
        /// Reads the payload as an ASCII string.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="trimTrailingNulls">Whether to remove trailing null characters.</param>
        /// <returns>The decoded string.</returns>
        public static string ReadAscii(this QlProtocolFrame frame, bool trimTrailingNulls = true)
        {
            string value = Encoding.ASCII.GetString(GetPayload(frame));
            return trimTrailingNulls ? value.TrimEnd('\0') : value;
        }

        /// <summary>
        /// Reads the payload as a BCD-encoded date and time.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="payloadOffset">Byte offset inside the payload.</param>
        /// <returns>The decoded <see cref="DateTime"/> value.</returns>
        public static DateTime ReadBcdDateTime(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeBcdDateTime(GetPayload(frame), payloadOffset);
        }

        /// <summary>
        /// Reads the payload as a BCD-encoded date and time string in <c>yyMMddHHmmss</c> format.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="payloadOffset">Byte offset inside the payload.</param>
        /// <returns>The decoded time string.</returns>
        public static string ReadBcdDateTimeText(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeBcdDateTimeText(GetPayload(frame), payloadOffset);
        }

        /// <summary>
        /// Reads the payload as an array of 16-bit unsigned integers.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <returns>A read-only list of decoded values.</returns>
        public static IReadOnlyList<ushort> ReadUInt16Array(this QlProtocolFrame frame)
        {
            byte[] payload = GetPayload(frame);
            if (payload.Length % 2 != 0)
            {
                throw new QlProtocolException("UInt16 array payload length must be a multiple of 2 bytes.");
            }

            List<ushort> values = new List<ushort>(payload.Length / 2);
            for (int offset = 0; offset < payload.Length; offset += 2)
            {
                values.Add(QlPayloadCodec.DecodeUInt16(payload, offset));
            }

            return values;
        }

        /// <summary>
        /// Decodes the payload according to a register definition.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="register">Register definition that describes the payload type.</param>
        /// <returns>A decoded register value wrapper.</returns>
        public static QlDecodedRegisterValue Decode(this QlProtocolFrame frame, QlRegisterDefinition register)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            object value;
            switch (register.PayloadType)
            {
                case QlPayloadType.UInt16:
                    value = frame.ReadUInt16();
                    break;
                case QlPayloadType.UInt32:
                    value = frame.ReadUInt32();
                    break;
                case QlPayloadType.Single:
                    value = frame.ReadSingle();
                    break;
                case QlPayloadType.Utf8:
                    value = frame.ReadUtf8();
                    break;
                case QlPayloadType.Ascii:
                    value = frame.ReadAscii();
                    break;
                case QlPayloadType.BcdDateTime:
                    value = frame.ReadBcdDateTime();
                    break;
                case QlPayloadType.UInt16Array:
                    value = frame.ReadUInt16Array();
                    break;
                case QlPayloadType.SingleArray:
                    value = frame.ReadSingles();
                    break;
                case QlPayloadType.Raw:
                default:
                    value = frame.Payload;
                    break;
            }

            return new QlDecodedRegisterValue(register, value);
        }

        /// <summary>
        /// Attempts to decode the payload using the built-in known register catalog.
        /// </summary>
        /// <param name="frame">Parsed protocol frame.</param>
        /// <param name="decoded">Decoded value when the address exists in the known register catalog; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> when the address is known and decoding succeeds; otherwise <c>false</c>.</returns>
        public static bool TryDecodeKnownRegister(this QlProtocolFrame frame, out QlDecodedRegisterValue? decoded)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (QlKnownRegisters.TryGet(frame.Address, out QlRegisterDefinition? register) && register != null)
            {
                decoded = frame.Decode(register);
                return true;
            }

            decoded = null;
            return false;
        }

        private static byte[] GetPayload(QlProtocolFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            return frame.Payload ?? Array.Empty<byte>();
        }
    }
}
