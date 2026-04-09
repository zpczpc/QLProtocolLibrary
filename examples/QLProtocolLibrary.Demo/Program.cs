using QLProtocolLibrary;

var mn = "1001";

Console.WriteLine("=== High-Level Commands (No Address Needed) ===");
Console.WriteLine(QlProtocolKnownCommands.BuildReadDeviceTimeHex(mn));
Console.WriteLine(QlKnownOperations.DeviceTime.BuildReadHex(mn));
Console.WriteLine(QlKnownOperations.RunStatus.BuildReadHex(mn));

Console.WriteLine();
Console.WriteLine("=== Generic Typed Parsing ===");
byte[] deviceTimeResponse = BuildReadResponse(mn, QlKnownRegisters.DeviceTime.Address, QlPayloadCodec.EncodeBcdDateTime(new DateTime(2026, 4, 9, 8, 30, 45)));
QlProtocolFrame deviceTimeFrame = QlProtocolParser.Parse(deviceTimeResponse);
Console.WriteLine($"Address={deviceTimeFrame.Address}, CRC={deviceTimeFrame.IsCrcValid}, DeviceTime={deviceTimeFrame.ReadBcdDateTime():yyyy-MM-dd HH:mm:ss}");

byte[] deviceNoPayload = QlPayloadCodec.EncodeUtf8("DEVICE-001", fixedByteLength: 16);
byte[] deviceNoResponse = BuildReadResponse(mn, QlKnownRegisters.DeviceNo.Address, deviceNoPayload);
QlProtocolFrame deviceNoFrame = QlProtocolParser.Parse(deviceNoResponse);
QlDecodedRegisterValue decoded = deviceNoFrame.Decode(QlKnownRegisters.DeviceNo);
Console.WriteLine($"DeviceNo={decoded.GetValue<string>()}");

Console.WriteLine();
Console.WriteLine("=== High-Level Business Parsing ===");
byte[] runStatusPayload = Combine(
    QlPayloadCodec.EncodeUInt16(2),
    QlPayloadCodec.EncodeUInt16(7),
    QlPayloadCodec.EncodeUInt16(1),
    QlPayloadCodec.EncodeUInt16(3),
    QlPayloadCodec.EncodeUInt32(1001),
    QlPayloadCodec.EncodeUInt32(0),
    new byte[14]);
byte[] runStatusResponse = BuildReadResponse(mn, QlKnownRegisters.RunStatus.Address, runStatusPayload);
QlProtocolFrame runStatusFrame = QlProtocolParser.Parse(runStatusResponse);
if (QlKnownOperations.RunStatus.TryParse(runStatusFrame, out QlRunStatusInfo? runStatus) && runStatus != null)
{
    Console.WriteLine(runStatus);
}

byte[] versionPayload = Combine(
    FixedUtf8(32, "Android-1.0"),
    FixedUtf8(32, "CommBoard-1.1"),
    FixedUtf8(32, "Core-2.0"),
    FixedUtf8(32, "ORP-1.0"),
    FixedUtf8(32, "Meter-1.2"),
    FixedUtf8(32, "Pump-1.3"),
    FixedUtf8(32, "Spectrometer-3.0"));
byte[] versionResponse = BuildReadResponse(mn, QlKnownRegisters.VersionBundle.Address, versionPayload);
QlProtocolFrame versionFrame = QlProtocolParser.Parse(versionResponse);
if (QlKnownOperations.VersionBundle.TryParse(versionFrame, out QlVersionBundleInfo? versionInfo) && versionInfo != null)
{
    Console.WriteLine(versionInfo);
}

Console.WriteLine();
Console.WriteLine("=== Unified Known Router ===");
if (QlProtocolKnownRouter.TryParse(deviceTimeFrame, out QlKnownParseResult? routed1) && routed1 != null)
{
    Console.WriteLine(routed1);
}
if (QlProtocolKnownRouter.TryParse(runStatusFrame, out QlKnownParseResult? routed2) && routed2 != null)
{
    Console.WriteLine(routed2.Name + " => " + routed2.Value);
}

Console.WriteLine();
Console.WriteLine("=== TCP Stream Decoder ===");
QlProtocolStreamDecoder decoder = new QlProtocolStreamDecoder();
byte[] stickyBytes = Combine(deviceTimeResponse, runStatusResponse, versionResponse);
var frames = decoder.Append(stickyBytes);
Console.WriteLine($"Stream decoder output count: {frames.Count}");

static byte[] BuildReadResponse(string mnText, ushort address, byte[] payload)
{
    byte[] body = Combine(
        ToMnBytes(mnText),
        new byte[] { 0x03 },
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

static byte[] FixedUtf8(int byteLength, string text)
{
    return QlPayloadCodec.EncodeUtf8(text, fixedByteLength: byteLength, padToEven: false);
}

static byte[] Combine(params byte[][] arrays)
{
    int totalLength = arrays.Sum(item => item.Length);
    byte[] result = new byte[totalLength];
    int offset = 0;
    foreach (byte[] item in arrays)
    {
        Array.Copy(item, 0, result, offset, item.Length);
        offset += item.Length;
    }

    return result;
}
