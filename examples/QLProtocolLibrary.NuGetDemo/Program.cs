using QLProtocolLibrary;

var mn = "1001";

Console.WriteLine("=== 1. Install From NuGet ===");
Console.WriteLine("dotnet add package QLProtocolLibrary --version 0.3.1");

Console.WriteLine();
Console.WriteLine("=== 2. Build Commands ===");
byte[] readDeviceTimeCommand = QlProtocolKnownCommands.BuildReadDeviceTime(mn);
byte[] readRunStatusCommand = QlKnownOperations.RunStatus.BuildRead(mn);
Console.WriteLine("Read device time: " + QlHexConverter.ToHexString(readDeviceTimeCommand));
Console.WriteLine("Read run status:  " + QlHexConverter.ToHexString(readRunStatusCommand));

Console.WriteLine();
Console.WriteLine("=== 3. Parse A Device Time Response ===");
QlProtocolFrame deviceTimeFrame = QlProtocolParser.ParseHex(
    "AA 55 00 00 00 00 00 00 03 E9 03 00 D0 06 26 04 09 08 30 45 36 87 BB 55");

if (QlProtocolKnownParsers.TryParseDeviceTime(deviceTimeFrame, out DateTime deviceTime))
{
    Console.WriteLine("Device time: " + deviceTime.ToString("yyyy-MM-dd HH:mm:ss"));
}

Console.WriteLine();
Console.WriteLine("=== 4. Parse A Run Status Response ===");
byte[] runStatusPayload = Combine(
    QlPayloadCodec.EncodeUInt16(2),
    QlPayloadCodec.EncodeUInt16(7),
    QlPayloadCodec.EncodeUInt16(1),
    QlPayloadCodec.EncodeUInt16(3),
    QlPayloadCodec.EncodeUInt32(1001),
    QlPayloadCodec.EncodeUInt32(0),
    new byte[4]);
QlProtocolFrame runStatusFrame = QlProtocolParser.Parse(
    BuildReadResponse(mn, QlKnownRegisters.RunStatus.Address, runStatusPayload));

if (QlKnownOperations.RunStatus.TryParse(runStatusFrame, out QlRunStatusInfo? runStatus) && runStatus != null)
{
    Console.WriteLine(runStatus);
}

Console.WriteLine();
Console.WriteLine("=== 5. Route Known Responses Automatically ===");
if (QlProtocolKnownRouter.TryParse(runStatusFrame, out QlKnownParseResult? routed) && routed != null)
{
    Console.WriteLine(routed.Name + " => " + routed.Value);
}

Console.WriteLine();
Console.WriteLine("=== 6. Decode A TCP Sticky Packet Stream ===");
QlProtocolStreamDecoder decoder = new QlProtocolStreamDecoder();
byte[] stickyBytes = Combine(deviceTimeFrame.RawBytes, runStatusFrame.RawBytes);
IReadOnlyList<QlProtocolFrame> frames = decoder.Append(stickyBytes);
Console.WriteLine("Decoded frame count: " + frames.Count);

static byte[] BuildReadResponse(string mnText, ushort address, byte[] payload)
{
    byte[] body = Combine(
        ToMnBytes(mnText),
        new byte[] { (byte)QlFunctionCode.Read },
        QlPayloadCodec.EncodeUInt16(address),
        new byte[] { (byte)payload.Length },
        payload);

    ushort crc = QlProtocolCrc16.Compute(body);
    return Combine(
        new byte[] { QlProtocolConstants.HeaderHigh, QlProtocolConstants.HeaderLow },
        body,
        QlProtocolCrc16.GetBytesLowHigh(crc),
        new byte[] { QlProtocolConstants.FooterHigh, QlProtocolConstants.FooterLow });
}

static byte[] ToMnBytes(string mnText)
{
    ulong mnValue = ulong.Parse(mnText);
    byte[] bytes = BitConverter.GetBytes(mnValue);
    if (BitConverter.IsLittleEndian)
    {
        Array.Reverse(bytes);
    }

    return bytes;
}

static byte[] Combine(params byte[][] arrays)
{
    int totalLength = 0;
    for (int i = 0; i < arrays.Length; i++)
    {
        totalLength += arrays[i].Length;
    }

    byte[] result = new byte[totalLength];
    int offset = 0;
    for (int i = 0; i < arrays.Length; i++)
    {
        Array.Copy(arrays[i], 0, result, offset, arrays[i].Length);
        offset += arrays[i].Length;
    }

    return result;
}
