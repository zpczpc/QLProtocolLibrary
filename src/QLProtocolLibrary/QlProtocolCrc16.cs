namespace QLProtocolLibrary
{
    using System;

    public static class QlProtocolCrc16
    {
        public static ushort Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x0001) == 0x0001)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        public static byte[] GetBytesLowHigh(ushort crc)
        {
            return new[]
            {
                (byte)(crc & 0x00FF),
                (byte)((crc >> 8) & 0x00FF)
            };
        }
    }
}
