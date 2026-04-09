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

        /// <summary>
        /// Gets the register start address.
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Gets the stable register key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the readable register name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the expected register count.
        /// </summary>
        public ushort RegisterCount { get; }

        /// <summary>
        /// Gets the payload decoding type.
        /// </summary>
        public QlPayloadType PayloadType { get; }

        /// <summary>
        /// Gets the optional business description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the expected payload byte length.
        /// </summary>
        public int PayloadByteLength => RegisterCount * 2;

        /// <inheritdoc />
        public override string ToString()
        {
            return Key + " - " + Name + " (" + Address + ")";
        }
    }
}
