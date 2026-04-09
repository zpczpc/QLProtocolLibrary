namespace QLProtocolLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public static class QlPayloadCodec
    {
        public static byte[] EncodeUInt16(ushort value)
        {
            return new[]
            {
                (byte)((value >> 8) & 0xFF),
                (byte)(value & 0xFF)
            };
        }

        public static ushort DecodeUInt16(byte[] data, int offset = 0)
        {
            ValidateSlice(data, offset, 2);
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        public static byte[] EncodeUInt32(uint value)
        {
            return new[]
            {
                (byte)((value >> 24) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)(value & 0xFF)
            };
        }

        public static uint DecodeUInt32(byte[] data, int offset = 0)
        {
            ValidateSlice(data, offset, 4);
            return ((uint)data[offset] << 24)
                | ((uint)data[offset + 1] << 16)
                | ((uint)data[offset + 2] << 8)
                | data[offset + 3];
        }

        public static byte[] EncodeSingle(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        public static float DecodeSingle(byte[] data, int offset = 0)
        {
            ValidateSlice(data, offset, 4);
            byte[] buffer = new byte[4];
            Array.Copy(data, offset, buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            return BitConverter.ToSingle(buffer, 0);
        }

        public static IReadOnlyList<float> DecodeSingles(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length % 4 != 0)
            {
                throw new QlProtocolException("Float payload length must be a multiple of 4 bytes.");
            }

            List<float> values = new List<float>(data.Length / 4);
            for (int offset = 0; offset < data.Length; offset += 4)
            {
                values.Add(DecodeSingle(data, offset));
            }

            return values;
        }

        public static byte[] EncodeUtf8(string value, int fixedByteLength = 0, bool padToEven = true)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int targetLength = bytes.Length;

            if (fixedByteLength > 0)
            {
                if (bytes.Length > fixedByteLength)
                {
                    throw new QlProtocolException("The encoded string is longer than the requested fixed length.");
                }

                targetLength = fixedByteLength;
            }
            else if (padToEven && (targetLength % 2 != 0))
            {
                targetLength++;
            }

            if (targetLength == bytes.Length)
            {
                return bytes;
            }

            byte[] result = new byte[targetLength];
            Array.Copy(bytes, result, bytes.Length);
            return result;
        }

        public static string DecodeUtf8(byte[] data, bool trimTrailingNulls = true)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string value = Encoding.UTF8.GetString(data);
            return trimTrailingNulls ? value.TrimEnd('\0') : value;
        }

        public static byte[] EncodeBcdDateTime(DateTime value)
        {
            return new[]
            {
                ToBcd(value.Year % 100),
                ToBcd(value.Month),
                ToBcd(value.Day),
                ToBcd(value.Hour),
                ToBcd(value.Minute),
                ToBcd(value.Second)
            };
        }

        public static DateTime DecodeBcdDateTime(byte[] data, int offset = 0)
        {
            ValidateSlice(data, offset, 6);

            int year = 2000 + FromBcd(data[offset]);
            int month = FromBcd(data[offset + 1]);
            int day = FromBcd(data[offset + 2]);
            int hour = FromBcd(data[offset + 3]);
            int minute = FromBcd(data[offset + 4]);
            int second = FromBcd(data[offset + 5]);

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
        }

        public static string DecodeBcdDateTimeText(byte[] data, int offset = 0)
        {
            DateTime value = DecodeBcdDateTime(data, offset);
            return value.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        private static byte ToBcd(int value)
        {
            if (value < 0 || value > 99)
            {
                throw new QlProtocolException("BCD value must be between 0 and 99.");
            }

            return (byte)(((value / 10) << 4) | (value % 10));
        }

        private static int FromBcd(byte value)
        {
            return ((value >> 4) * 10) + (value & 0x0F);
        }

        private static void ValidateSlice(byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (offset < 0 || count < 0 || offset + count > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
        }
    }
}
