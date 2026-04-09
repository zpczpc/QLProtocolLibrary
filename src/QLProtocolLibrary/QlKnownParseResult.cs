namespace QLProtocolLibrary
{
    public sealed class QlKnownParseResult
    {
        public QlKnownParseResult(string name, QlRegisterDefinition register, object value)
        {
            Name = name;
            Register = register;
            Value = value;
        }

        public string Name { get; }

        public QlRegisterDefinition Register { get; }

        public object Value { get; }

        public T GetValue<T>()
        {
            return (T)Value;
        }

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}
