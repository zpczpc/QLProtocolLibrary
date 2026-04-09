namespace QLProtocolLibrary
{
    public enum QlFunctionCode : byte
    {
        Unknown = 0x00,
        Read = 0x03,
        SingleWriteSuccess = 0x06,
        Write = 0x10,
        ReadFailed = 0x83,
        SingleWriteFailed = 0x86,
        WriteFailed = 0x90
    }
}
