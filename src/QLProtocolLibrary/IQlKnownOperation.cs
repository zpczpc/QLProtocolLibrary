namespace QLProtocolLibrary
{
    public interface IQlKnownOperation
    {
        string Name { get; }

        QlRegisterDefinition Register { get; }

        byte[] BuildRead(string mn);

        string BuildReadHex(string mn);

        bool TryParse(QlProtocolFrame frame, out QlKnownParseResult? result);
    }
}
