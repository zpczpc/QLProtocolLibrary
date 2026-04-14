using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

Console.WriteLine("=== 1. 文档示例：读取 00 00 寄存器 1 个 ===");
byte[] readCommand = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(readCommand));

Console.WriteLine();
Console.WriteLine("=== 2. 解析读取响应报文 ===");
byte[] responseBytes =
{
    0x10, 0x00, 0x00, 0x01,
    0x03,
    0x00, 0x00,
    0x04,
    0x1C, 0x04, 0x1F, 0x41,
    0x97, 0xE9
};
QlProtocolFrame readFrame = QlProtocolParser.Parse(responseBytes);
Console.WriteLine($"Device={readFrame.DeviceAddressHex}, Kind={readFrame.Kind}, Address=0x{readFrame.Address:X4}, CRC={readFrame.IsCrcValid}");
Console.WriteLine($"FloatValue={readFrame.ReadSingle():F4}");

Console.WriteLine();
Console.WriteLine("=== 3. 文档示例：写 FLOAT 寄存器 ===");
byte[] writeFloatCommand = QlProtocolCommandBuilder.BuildWriteFloat(0x10000005, 0x164E, 0.0596f);
Console.WriteLine(QlHexConverter.ToHexString(writeFloatCommand));

Console.WriteLine();
Console.WriteLine("=== 4. 文档示例：写 WORD 寄存器 ===");
byte[] writeWordCommand = QlProtocolCommandBuilder.BuildWriteRegisters(deviceAddress, 0x14B4, 115);
Console.WriteLine(QlHexConverter.ToHexString(writeWordCommand));

Console.WriteLine();
Console.WriteLine("=== 5. 高层已知寄存器组包 ===");
Console.WriteLine(QlProtocolKnownCommands.BuildReadConcentrationHex(deviceAddress));

Console.WriteLine();
Console.WriteLine("=== 6. 路由解析运行状态响应 ===");
byte[] runStatusPayload = Combine(
    QlPayloadCodec.EncodeValueUInt16(2),
    QlPayloadCodec.EncodeValueUInt16(7),
    QlPayloadCodec.EncodeValueUInt16(1),
    QlPayloadCodec.EncodeValueUInt16(3),
    QlPayloadCodec.EncodeUInt32(1001),
    QlPayloadCodec.EncodeUInt32(0));
QlProtocolFrame runStatusFrame = QlProtocolParser.Parse(BuildReadResponse(deviceAddress, QlKnownRegisters.RunStatus.Address, runStatusPayload));

if (QlProtocolKnownRouter.TryParse(runStatusFrame, out QlKnownParseResult? routed) && routed != null)
{
    Console.WriteLine(routed.Name + " => " + routed.Value);
}

static byte[] BuildReadResponse(uint deviceAddress, ushort address, byte[] payload)
{
    byte[] body = Combine(
        ToDeviceAddressBytes(deviceAddress),
        new[] { (byte)QlFunctionCode.Read },
        QlPayloadCodec.EncodeUInt16(address),
        new[] { (byte)payload.Length },
        payload);

    ushort crc = QlProtocolCrc16.Compute(body);
    return Combine(body, QlProtocolCrc16.GetBytesLowHigh(crc));
}

static byte[] ToDeviceAddressBytes(uint deviceAddress)
{
    return new[]
    {
        (byte)((deviceAddress >> 24) & 0xFF),
        (byte)((deviceAddress >> 16) & 0xFF),
        (byte)((deviceAddress >> 8) & 0xFF),
        (byte)(deviceAddress & 0xFF)
    };
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
