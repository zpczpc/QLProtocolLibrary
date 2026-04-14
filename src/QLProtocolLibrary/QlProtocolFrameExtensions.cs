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
        public static ushort ReadUInt16(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeValueUInt16(GetPayload(frame), payloadOffset);
        }

        public static uint ReadUInt32(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeUInt32(GetPayload(frame), payloadOffset);
        }

        public static float ReadSingle(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeSingle(GetPayload(frame), payloadOffset);
        }

        public static IReadOnlyList<float> ReadSingles(this QlProtocolFrame frame)
        {
            return QlPayloadCodec.DecodeSingles(GetPayload(frame));
        }

        public static string ReadUtf8(this QlProtocolFrame frame, bool trimTrailingNulls = true)
        {
            return QlPayloadCodec.DecodeUtf8(GetPayload(frame), trimTrailingNulls);
        }

        public static string ReadAscii(this QlProtocolFrame frame, bool trimTrailingNulls = true)
        {
            string value = Encoding.ASCII.GetString(GetPayload(frame));
            return trimTrailingNulls ? value.TrimEnd('\0') : value;
        }

        public static DateTime ReadBcdDateTime(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeBcdDateTime(GetPayload(frame), payloadOffset);
        }

        public static string ReadBcdDateTimeText(this QlProtocolFrame frame, int payloadOffset = 0)
        {
            return QlPayloadCodec.DecodeBcdDateTimeText(GetPayload(frame), payloadOffset);
        }

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
                values.Add(QlPayloadCodec.DecodeValueUInt16(payload, offset));
            }

            return values;
        }

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
