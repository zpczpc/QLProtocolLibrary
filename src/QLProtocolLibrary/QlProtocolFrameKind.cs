namespace QLProtocolLibrary
{
    public enum QlProtocolFrameKind
    {
        Unknown = 0,
        ReadRequest,
        ReadResponse,
        WriteRequest,
        WriteResponse,
        OperationRequest,
        OperationResponse,
        LengthPrefixedFrame,
        BootloaderFrame,
        ErrorResponse
    }
}
