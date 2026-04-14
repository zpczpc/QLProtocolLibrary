namespace QLProtocolLibrary
{
    public enum QlFunctionCode : byte
    {
        Unknown = 0x00,
        Read = 0x03,
        Write = 0x06,
        Operation = 0x08,
        Bootloader = 0x09,
        ReadLog = 0x23,
        WriteLog = 0x26,
        TfRead = 0x30,
        Forward = 0x32,
        Database = 0x33
    }
}
