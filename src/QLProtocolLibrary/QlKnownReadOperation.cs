namespace QLProtocolLibrary
{
    public delegate bool QlKnownParser<T>(QlProtocolFrame frame, out T value);

    public sealed class QlKnownReadOperation<T> : IQlKnownOperation
    {
        private readonly QlKnownParser<T> _parser;

        public QlKnownReadOperation(string name, QlRegisterDefinition register, QlKnownParser<T> parser)
        {
            Name = name;
            Register = register;
            _parser = parser;
        }

        public string Name { get; }

        public QlRegisterDefinition Register { get; }

        public byte[] BuildRead(string mn)
        {
            return QlProtocolCommandBuilder.BuildRead(mn, Register);
        }

        public string BuildReadHex(string mn)
        {
            return QlHexConverter.ToHexString(BuildRead(mn));
        }

        public bool TryParse(QlProtocolFrame frame, out T value)
        {
            return _parser(frame, out value);
        }

        public T Parse(QlProtocolFrame frame)
        {
            if (!TryParse(frame, out T value))
            {
                throw new QlProtocolException("Frame cannot be parsed as known operation '" + Name + "'.");
            }

            return value;
        }

        public bool TryParse(QlProtocolFrame frame, out QlKnownParseResult? result)
        {
            if (_parser(frame, out T value))
            {
                result = new QlKnownParseResult(Name, Register, value!);
                return true;
            }

            result = null;
            return false;
        }
    }
}
