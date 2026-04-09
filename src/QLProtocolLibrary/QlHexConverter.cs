namespace QLProtocolLibrary
{
    using System;
    using System.Globalization;
    using System.Text;

    public static class QlHexConverter
    {
        public static byte[] FromHexString(string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex));
            }

            string[] parts = hex.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes = new byte[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                bytes[i] = byte.Parse(parts[i], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }

            return bytes;
        }

        public static string ToHexString(byte[] data, bool withSpaces = true)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(data.Length * (withSpaces ? 3 : 2));
            for (int i = 0; i < data.Length; i++)
            {
                if (withSpaces && i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
