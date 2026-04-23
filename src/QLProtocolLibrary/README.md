# QLProtocolLibrary

中文 | [English](README.en.md)

`QLProtocolLibrary` 是一个面向 QL 设备主应用层协议报文的 C# NuGet 包。

默认处理的主报文格式为：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

同时支持文档中的可选包络：

```text
C6 F4 C2 CC + 长度(2) + 裸报文 + 0D 0A
```

## 安装

```bash
dotnet add package QLProtocolLibrary
```

## 最短示例

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 01 03 00 00 00 01 43 21
```

## `0x32` 指令转发

从 `0.5.0` 开始，库提供了专用的 `0x32` 构包与解析辅助方法。

报文结构：

```text
设备地址(4) + 0x32(1) + 数据长度(2) + 端口ID(1) + 转发内容(N) + CRC16(2)
```

示例：

```csharp
using QLProtocolLibrary;

byte[] forwardedCommand = QlHexConverter.FromHexString(
    "10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9");

byte[] forwardFrame = QlProtocolCommandBuilder.BuildForward(
    0x1000000F,
    0x01,
    forwardedCommand);

QlProtocolFrame frame = QlProtocolParser.Parse(forwardFrame);

Console.WriteLine(frame.ReadForwardPortId()); // 1
Console.WriteLine(QlHexConverter.ToHexString(frame.ReadForwardContent()));
```

## 常用 API

### 构包

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWrite(...)`
- `QlProtocolCommandBuilder.BuildWriteRegisters(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolCommandBuilder.BuildForward(...)`
- `QlProtocolCommandBuilder.BuildPacket(...)`

### 解包

- `QlProtocolParser.Parse(...)`
- `QlProtocolParser.ParseHex(...)`
- `QlProtocolParser.TryParse(...)`

### payload 读取

- `frame.ReadUInt16()`
- `frame.ReadUInt32()`
- `frame.ReadSingle()`
- `frame.ReadUtf8()`
- `frame.ReadBcdDateTime()`
- `frame.ReadForwardPortId()`
- `frame.ReadForwardContent()`

## 使用注意事项

- 设备地址固定 4 字节
- CRC16 按低字节在前发送
- 1 个寄存器按文档为 4 字节
- 协议控制字段与 payload 数据值字段的字节序不能混为一谈
- `0x32` 的数据长度 = `端口ID + 转发内容` 的总字节数

## 相关文档

- 仓库首页 README：`README.md`
- 中文 API：`docs/API.zh-CN.md`
- English API：`docs/API.en.md`
- 示例：`examples/QLProtocolLibrary.Demo`
