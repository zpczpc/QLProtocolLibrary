# QLProtocolLibrary

中文 | [English](README.en.md)

`QLProtocolLibrary` 是面向主应用层协议报文的 C# 类库。

这个包默认处理的是主协议裸报文：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

同时支持文档中的可选包络：

```text
C6 F4 C2 CC + 长度(2) + 裸报文 + 0D 0A
```

## 适合什么场景

- 上位机直接发送/接收协议报文
- 设备调试工具
- 协议解析库封装
- 外部项目通过 NuGet 复用协议能力

## 安装

```bash
dotnet add package QLProtocolLibrary
```

## 最短示例

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

byte[] command = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(command));
// 10 00 00 01 03 00 00 00 01 43 21
```

## 解析示例

```csharp
using QLProtocolLibrary;

byte[] responseBytes =
{
    0x10, 0x00, 0x00, 0x01,
    0x03,
    0x00, 0x00,
    0x04,
    0x1C, 0x04, 0x1F, 0x41,
    0x97, 0xE9
};

QlProtocolFrame frame = QlProtocolParser.Parse(responseBytes);

Console.WriteLine(frame.DeviceAddressHex);
Console.WriteLine(frame.ReadSingle()); // 9.9385
```

真实串口/485/TCP 接收场景里，建议直接把收到的原始 `byte[]` 传给 `QlProtocolParser.Parse(...)`。
`ParseHex(...)` 更适合调试和测试。

## 关键 API

### 组包

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWrite(...)`
- `QlProtocolCommandBuilder.BuildWriteRegisters(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolCommandBuilder.BuildPacket(...)`

### 解包

- `QlProtocolParser.Parse(...)`
- `QlProtocolParser.ParseHex(...)`
- `QlProtocolParser.TryParse(...)`

### 类型读取

- `frame.ReadUInt16()`
- `frame.ReadUInt32()`
- `frame.ReadSingle()`
- `frame.ReadUtf8()`
- `frame.ReadBcdDateTime()`

### 高层已知寄存器 API

- `QlProtocolKnownCommands`
- `QlProtocolKnownParsers`
- `QlKnownOperations`
- `QlProtocolKnownRouter`

## 使用时要注意

- 设备地址固定 4 字节
- CRC16 发送顺序为低字节在前
- 1 个寄存器按文档为 4 字节
- 协议字段和数据值的字节序不能混为一谈
- 不同功能码、不同数据类型的数据区定义不同，必须以文档对应章节为准

## 相关文档

- 仓库首页 README：`README.md`
- 中文 API：`docs/API.zh-CN.md`
- English API: `docs/API.en.md`
- 示例：`examples/QLProtocolLibrary.Demo`
