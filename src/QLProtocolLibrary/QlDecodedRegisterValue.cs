namespace QLProtocolLibrary
{
    using System;

    public sealed class QlDecodedRegisterValue
    {
        public QlDecodedRegisterValue(QlRegisterDefinition register, object value)
        {
            Register = register ?? throw new ArgumentNullException(nameof(register));
            Value = value;
        }

        public QlRegisterDefinition Register { get; }

        public object Value { get; }

        public T GetValue<T>()
        {
            return (T)Value;
        }

        public override string ToString()
        {
            return Register.Name + "=" + (Value ?? string.Empty);
        }
    }
}
