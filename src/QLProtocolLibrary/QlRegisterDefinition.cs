namespace QLProtocolLibrary
{
    using System;

    /// <summary>
    /// Describes a register address, expected register count, and payload type.
    /// </summary>
    public sealed class QlRegisterDefinition
    {
        /// <summary>
        /// Initializes a new register definition.
        /// </summary>
        /// <param name="address">Register start address.</param>
        /// <param name="key">Stable register key.</param>
        /// <param name="name">Readable register name.</param>
        /// <param name="registerCount">Expected register count.</param>
        /// <param name="payloadType">Payload decoding type.</param>
        /// <param name="description">Optional business description.</param>
        public QlRegisterDefinition(
            ushort address,
            string key,
            string name,
            ushort registerCount,
            QlPayloadType payloadType,
            string description = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Register key cannot be empty.", nameof(key));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Register name cannot be empty.", nameof(name));
            }

            Address = address;
            Key = key;
            Name = name;
            RegisterCount = registerCount;
            PayloadType = payloadType;
            Description = description ?? string.Empty;
        }

        public ushort Address { get; }

        public string Key { get; }

        public string Name { get; }

        public ushort RegisterCount { get; }

        public QlPayloadType PayloadType { get; }

        public string Description { get; }

        public int PayloadByteLength => RegisterCount * QlProtocolConstants.RegisterByteLength;

        public override string ToString()
        {
            return Key + " - " + Name + " (" + Address + ")";
        }
    }
}
