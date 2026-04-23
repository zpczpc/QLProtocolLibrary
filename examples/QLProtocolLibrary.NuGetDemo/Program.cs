using QLProtocolLibrary;

uint deviceAddress = 0x10000001;
uint forwardGatewayAddress = 0x1000000F;
ushort readAddress = 0x0000;
ushort readRegisterCount = 0x0001;
ushort writeAddress = 0x14B4;
ushort writeUInt16Value = 1;
const byte WriteSuccessCode = 0x60;

Console.WriteLine("=== 1. NuGet 安装命令 ===");
Console.WriteLine("dotnet add package QLProtocolLibrary --version 0.4.0");

Console.WriteLine();
Console.WriteLine("=== 2. 组 0x03 读请求 ===");

byte[] readRequestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, readAddress, readRegisterCount);

Console.WriteLine("读请求报文：");
Console.WriteLine(QlHexConverter.ToHexString(readRequestBytes));
Console.WriteLine("预期结果：");
Console.WriteLine("10 00 00 01 03 00 00 00 01 43 21");

Console.WriteLine();
Console.WriteLine("=== 3. 解析模拟 0x03 响应 ===");

byte[] readResponseBytes =
{
    0x10, 0x00, 0x00, 0x01,
    0x03,
    0x00, 0x00,
    0x04,
    0x1C, 0x04, 0x1F, 0x41,
    0x97, 0xE9
};

QlProtocolFrame readFrame = QlProtocolParser.Parse(readResponseBytes);

Console.WriteLine($"设备地址：{readFrame.DeviceAddressHex}");
Console.WriteLine($"功能码：0x{readFrame.RawFunctionCode:X2}");
Console.WriteLine($"报文类型：{readFrame.Kind}");
Console.WriteLine($"寄存器地址：0x{readFrame.Address:X4}");
Console.WriteLine($"数据字节数：{readFrame.ByteCount}");
Console.WriteLine($"CRC 是否有效：{readFrame.IsCrcValid}");
Console.WriteLine($"解析出的 float 值：{readFrame.ReadSingle():F4}");

Console.WriteLine();
Console.WriteLine("=== 4. 组 0x06 写请求 ===");

byte[] writeRequestBytes = QlProtocolCommandBuilder.BuildWriteRegisters(
    deviceAddress,
    writeAddress,
    writeUInt16Value);

Console.WriteLine($"写入地址：0x{writeAddress:X4}");
Console.WriteLine($"写入 UInt16 值：{writeUInt16Value}");
Console.WriteLine("写请求报文：");
Console.WriteLine(QlHexConverter.ToHexString(writeRequestBytes));

Console.WriteLine();
Console.WriteLine("=== 5. 解析模拟 0x06 响应 ===");

byte[] writeResponseBytes = QlProtocolCommandBuilder.BuildPacket(
    deviceAddress,
    (byte)QlFunctionCode.Write,
    new[]
    {
        (byte)(writeAddress >> 8),
        (byte)(writeAddress & 0xFF),
        (byte)0x01,
        WriteSuccessCode
    });

QlProtocolFrame writeFrame = QlProtocolParser.Parse(writeResponseBytes);
bool writeSuccess =
    writeFrame.IsCrcValid
    && writeFrame.Kind == QlProtocolFrameKind.WriteResponse
    && writeFrame.DeviceAddress == deviceAddress
    && writeFrame.Address == writeAddress
    && writeFrame.ResponseCode == WriteSuccessCode;

Console.WriteLine("写响应报文：");
Console.WriteLine(QlHexConverter.ToHexString(writeResponseBytes));
Console.WriteLine($"报文类型：{writeFrame.Kind}");
Console.WriteLine($"响应码：0x{writeFrame.ResponseCode.GetValueOrDefault():X2}");
Console.WriteLine($"CRC 是否有效：{writeFrame.IsCrcValid}");
Console.WriteLine($"是否写入成功：{writeSuccess}");

Console.WriteLine();
Console.WriteLine("=== 6. 组 0x32 指令转发请求 ===");

// 0x32 格式：
// 设备地址(4) + 0x32(1) + 数据长度(2，高字节在前) + 端口ID(1) + 转发内容(N) + CRC(2，低字节在前)
// 其中“数据长度”表示从端口ID开始，到 CRC 之前最后一个字节为止的总字节数。
byte forwardPortId = 0x01;
byte[] forwardedCommand =
{
    0x10, 0x00, 0x00, 0x01,
    0x06,
    0x00, 0x16,
    0x00, 0x01,
    0x04,
    0x01, 0x00, 0x00, 0x00,
    0x2E, 0xF9
};

// 当前 NuGet 0.4.0 还没有内置 BuildForward / ReadForwardPortId / ReadForwardContent。
// 等你发布包含新 API 的版本后，这里可以直接换成库方法一行调用。
byte[] forwardRequestBytes = BuildForwardPacket(
    forwardGatewayAddress,
    forwardPortId,
    forwardedCommand);

Console.WriteLine($"转发端口 ID：{forwardPortId}");
Console.WriteLine("被转发的原始命令：");
Console.WriteLine(QlHexConverter.ToHexString(forwardedCommand));
Console.WriteLine("0x32 转发请求：");
Console.WriteLine(QlHexConverter.ToHexString(forwardRequestBytes));
Console.WriteLine("预期示例：");
Console.WriteLine("10 00 00 0F 32 00 11 01 10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9 CE D2");

QlProtocolFrame forwardRequestFrame = QlProtocolParser.Parse(forwardRequestBytes);
Console.WriteLine($"报文类型：{forwardRequestFrame.Kind}");
Console.WriteLine($"功能码：0x{forwardRequestFrame.RawFunctionCode:X2}");
Console.WriteLine($"数据长度：{forwardRequestFrame.DataLength}");
Console.WriteLine($"解析出的端口 ID：{GetForwardPortId(forwardRequestFrame)}");
Console.WriteLine("解析出的转发内容：");
Console.WriteLine(QlHexConverter.ToHexString(GetForwardContent(forwardRequestFrame)));

Console.WriteLine();
Console.WriteLine("=== 7. 解析模拟 0x32 转发响应 ===");

byte[] forwardedResponse =
{
    0x10, 0x00, 0x00, 0x01,
    0x06,
    0x00, 0x16,
    0x01,
    0x60,
    0x0A, 0x8F
};

byte[] forwardResponseBytes = BuildForwardPacket(
    forwardGatewayAddress,
    forwardPortId,
    forwardedResponse);

QlProtocolFrame forwardResponseFrame = QlProtocolParser.Parse(forwardResponseBytes);

Console.WriteLine("0x32 转发响应：");
Console.WriteLine(QlHexConverter.ToHexString(forwardResponseBytes));
Console.WriteLine($"报文类型：{forwardResponseFrame.Kind}");
Console.WriteLine($"功能码：{forwardResponseFrame.FunctionCode}");
Console.WriteLine($"CRC 是否有效：{forwardResponseFrame.IsCrcValid}");
Console.WriteLine($"数据长度：{forwardResponseFrame.DataLength}");
Console.WriteLine($"解析出的端口 ID：{GetForwardPortId(forwardResponseFrame)}");
Console.WriteLine("转发响应里的内容：");
Console.WriteLine(QlHexConverter.ToHexString(GetForwardContent(forwardResponseFrame)));

Console.WriteLine();
Console.WriteLine("=== 8. 0x32 用法总结 ===");
Console.WriteLine("1) 先把你要转发的正常命令按原来的方式组出来。");
Console.WriteLine("2) 在这条命令前面加 1 个字节的端口 ID。");
Console.WriteLine("3) 数据长度 = 1 字节端口 ID + 被转发命令长度。");
Console.WriteLine("4) Parse(...) 后，payload[0] 是端口 ID，payload[1..] 才是被转发内容。");

static byte[] BuildForwardPacket(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)
{
    byte[] functionData = Combine(
        QlPayloadCodec.EncodeUInt16((ushort)(forwardedContent.Length + 1)),
        new[] { portId },
        forwardedContent);

    return QlProtocolCommandBuilder.BuildPacket(
        deviceAddress,
        (byte)QlFunctionCode.Forward,
        functionData,
        includeEnvelope);
}

static byte GetForwardPortId(QlProtocolFrame frame)
{
    ValidateForwardFrame(frame);
    return frame.Payload[0];
}

static byte[] GetForwardContent(QlProtocolFrame frame)
{
    ValidateForwardFrame(frame);

    byte[] content = new byte[frame.Payload.Length - 1];
    Array.Copy(frame.Payload, 1, content, 0, content.Length);
    return content;
}

static void ValidateForwardFrame(QlProtocolFrame frame)
{
    if (frame.FunctionCode != QlFunctionCode.Forward)
    {
        throw new InvalidOperationException("当前报文不是 0x32 指令转发报文。");
    }

    if (frame.Payload.Length < 1)
    {
        throw new InvalidOperationException("0x32 的 payload 至少要包含 1 个字节的端口 ID。");
    }
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
