namespace QLProtocolLibrary
{
    public interface IQlKnownOperation
    {
        string Name { get; }

        QlRegisterDefinition Register { get; }

        byte[] BuildRead(uint deviceAddress);

        string BuildReadHex(uint deviceAddress);

        bool TryParse(QlProtocolFrame frame, out QlKnownParseResult? result);
    }
}
