using QLProtocolLibrary;

// 这里把自己当成“外部用户”来使用 NuGet 包。
// 最常见的实际流程就是：
// 1. 先组一个发送报文
// 2. 把原始 byte[] 发给设备
// 3. 收到设备返回的 byte[] 后直接解析

uint deviceAddress = 0x10000001;
ushort readAddress = 0x0000;
ushort readRegisterCount = 0x0001;
ushort writeAddress = 0x14B4;
ushort writeUInt16Value = 1;
const byte WriteSuccessCode = 0x60;

Console.WriteLine("=== 1. NuGet 安装命令 ===");
Console.WriteLine("dotnet add package QLProtocolLibrary --version 0.4.0");

Console.WriteLine();
Console.WriteLine("=== 2. 组发送报文：读取 0x0000 开始的 1 个寄存器 ===");

byte[] readRequestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, readAddress, readRegisterCount);

Console.WriteLine("发送报文：");
Console.WriteLine(QlHexConverter.ToHexString(readRequestBytes));
Console.WriteLine("文档期望：");
Console.WriteLine("10 00 00 01 03 00 00 00 01 43 21");

Console.WriteLine();
Console.WriteLine("=== 3. 模拟接收设备读响应报文 ===");

byte[] readResponseBytes =
{
    0x10, 0x00, 0x00, 0x01,
    0x03,
    0x00, 0x00,
    0x04,
    0x1C, 0x04, 0x1F, 0x41,
    0x97, 0xE9
};

Console.WriteLine("读响应报文：");
Console.WriteLine(QlHexConverter.ToHexString(readResponseBytes));

Console.WriteLine();
Console.WriteLine("=== 4. 解析读响应报文 ===");

QlProtocolFrame readFrame = QlProtocolParser.Parse(readResponseBytes);

Console.WriteLine($"设备地址: {readFrame.DeviceAddressHex}");
Console.WriteLine($"功能码: 0x{readFrame.RawFunctionCode:X2}");
Console.WriteLine($"报文类型: {readFrame.Kind}");
Console.WriteLine($"寄存器地址: 0x{readFrame.Address:X4}");
Console.WriteLine($"数据长度: {readFrame.ByteCount}");
Console.WriteLine($"CRC 是否有效: {readFrame.IsCrcValid}");

Console.WriteLine();
Console.WriteLine("=== 5. 从 payload 中读取 float 值 ===");

float floatValue = readFrame.ReadSingle();
Console.WriteLine($"解析出的 float 值: {floatValue:F4}");

Console.WriteLine();
Console.WriteLine("=== 6. 组写入报文：写入 UInt16 值 1 ===");

// 这个协议里 1 个寄存器 = 4 字节。
// BuildWriteRegisters 会把 UInt16 值编码成协议里寄存器写入需要的 payload 格式。
byte[] writeRequestBytes = QlProtocolCommandBuilder.BuildWriteRegisters(
    deviceAddress,
    writeAddress,
    writeUInt16Value);

Console.WriteLine($"写入目标地址: 0x{writeAddress:X4}");
Console.WriteLine($"写入 UInt16 值: {writeUInt16Value}");
Console.WriteLine("写入请求报文：");
Console.WriteLine(QlHexConverter.ToHexString(writeRequestBytes));

Console.WriteLine();
Console.WriteLine("=== 7. 模拟接收设备写响应报文，并判断是否写入成功 ===");

// 实际项目中，这个 byte[] 应该来自串口 / TCP / 485 收到的设备回复。
// 写响应格式由库解析为：
// 设备地址 + 0x06 + 寄存器地址 + 字节数 + 响应码 + CRC
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

Console.WriteLine("写响应报文：");
Console.WriteLine(QlHexConverter.ToHexString(writeResponseBytes));

QlProtocolFrame writeFrame = QlProtocolParser.Parse(writeResponseBytes);
bool writeSuccess =
    writeFrame.IsCrcValid
    && writeFrame.Kind == QlProtocolFrameKind.WriteResponse
    && writeFrame.DeviceAddress == deviceAddress
    && writeFrame.Address == writeAddress
    && writeFrame.ResponseCode == WriteSuccessCode;

Console.WriteLine($"报文类型: {writeFrame.Kind}");
Console.WriteLine($"寄存器地址: 0x{writeFrame.Address:X4}");
Console.WriteLine($"响应码: 0x{writeFrame.ResponseCode.GetValueOrDefault():X2}");
Console.WriteLine($"CRC 是否有效: {writeFrame.IsCrcValid}");
Console.WriteLine($"是否写入成功: {writeSuccess}");

Console.WriteLine();
Console.WriteLine("=== 8. 最小使用方式总结 ===");
Console.WriteLine("1) 用 QlProtocolCommandBuilder 组出 requestBytes");
Console.WriteLine("2) 把 requestBytes 发给设备");
Console.WriteLine("3) 从串口 / TCP / 485 收到 responseBytes");
Console.WriteLine("4) 用 QlProtocolParser.Parse(responseBytes) 解析");
Console.WriteLine("5) 读操作看 payload，写操作看 WriteResponse + ResponseCode");
