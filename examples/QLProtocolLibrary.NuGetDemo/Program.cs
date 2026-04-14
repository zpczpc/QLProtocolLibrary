using QLProtocolLibrary;

// 这里把自己当成“外部用户”来使用 NuGet 包。
// 场景就是最常见的那种：
// 1. 先组一个发送报文
// 2. 把 byte[] 发给设备
// 3. 收到设备返回的 byte[] 后直接解析

uint deviceAddress = 0x10000001;
ushort registerAddress = 0x0000;
ushort registerCount = 0x0001;

Console.WriteLine("=== 1. NuGet 安装命令 ===");
Console.WriteLine("dotnet add package QLProtocolLibrary --version 0.4.0");

Console.WriteLine();
Console.WriteLine("=== 2. 组发送报文：读取 00 00 开始的 1 个寄存器 ===");

// 按协议文档：
// 设备地址(4) + 功能码03(1) + 寄存器起始地址(2) + 寄存器数量(2) + CRC(2)
// 其中 00 01 表示读取 1 个寄存器，而这份协议里 1 个寄存器 = 4 字节数据。
byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, registerAddress, registerCount);

Console.WriteLine("发送报文：");
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
Console.WriteLine("文档期望：");
Console.WriteLine("10 00 00 01 03 00 00 00 01 43 21");

Console.WriteLine();
Console.WriteLine("=== 3. 模拟接收设备应答报文 ===");

// 实际项目里，这里的 responseBytes 应该直接来自串口 / TCP / 485 收到的原始字节。
// Demo 里用 byte[] 常量来模拟设备返回，而不是把“十六进制字符串”当成正式输入。
byte[] responseBytes =
{
    0x10, 0x00, 0x00, 0x01,
    0x03,
    0x00, 0x00,
    0x04,
    0x1C, 0x04, 0x1F, 0x41,
    0x97, 0xE9
};

Console.WriteLine("应答报文：");
Console.WriteLine(QlHexConverter.ToHexString(responseBytes));

Console.WriteLine();
Console.WriteLine("=== 4. 解析应答报文 ===");

QlProtocolFrame frame = QlProtocolParser.Parse(responseBytes);

Console.WriteLine($"设备地址: {frame.DeviceAddressHex}");
Console.WriteLine($"功能码: 0x{frame.RawFunctionCode:X2}");
Console.WriteLine($"报文类型: {frame.Kind}");
Console.WriteLine($"寄存器地址: 0x{frame.Address:X4}");
Console.WriteLine($"数据长度: {frame.ByteCount}");
Console.WriteLine($"CRC 是否有效: {frame.IsCrcValid}");

Console.WriteLine();
Console.WriteLine("=== 5. 读取 payload 中的数据值 ===");

// 这条应答对应文档里的 00 00 寄存器。
// 文档说明 1C 04 1F 41 对应的 float 值为 9.9385。
// 由于这个地址不是库内置的“已知业务寄存器”，这里直接走通用 API 最合适。
float value = frame.ReadSingle();
Console.WriteLine($"读取值(float): {value:F4}");

Console.WriteLine();
Console.WriteLine("=== 6. 实际项目中的最小用法 ===");
Console.WriteLine("1) 用 QlProtocolCommandBuilder.BuildRead(...) 组出 requestBytes");
Console.WriteLine("2) 把 requestBytes 发给设备");
Console.WriteLine("3) 收到 responseBytes 后调用 QlProtocolParser.Parse(responseBytes)");
Console.WriteLine("4) 再按你的寄存器数据类型调用 ReadSingle / ReadUInt16 / ReadUInt32 等方法");
