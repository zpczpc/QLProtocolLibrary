# QLProtocolLibrary

[![CI](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml)
[![NuGet](https://img.shields.io/nuget/v/QLProtocolLibrary?logo=nuget)](https://www.nuget.org/packages/QLProtocolLibrary)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

中文 | [English](README.en.md)

`QLProtocolLibrary` 是一个面向 QL 设备主应用层协议的 C# 类库。

当前实现对齐的主报文结构为：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

同时支持文档中的可选包络：

```text
C6 F4 C2 CC + 长度(2) + 裸报文 + 0D 0A
```

它的目标很直接：

1. 让调用方直接组协议报文
2. 让调用方直接解协议报文

## 适合什么场景

- 上位机直接通过串口、TCP、RS485 发送和接收报文
- 设备调试工具、协议测试工具
- 需要把协议能力封装成公共库的项目
- 外部项目通过 NuGet 复用协议构包和解包能力

## 安装

```bash
dotnet add package QLProtocolLibrary
```

## 最短接入方式

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 01 03 00 00 00 01 43 21
```

## 常见功能码

- `0x03`：读寄存器
- `0x06`：写寄存器
- `0x08`：操作指令
- `0x23`：读日志
- `0x26`：写日志
- `0x30`：TF 目录/文件读取
- `0x32`：指令转发
- `0x33`：数据库读取

## 快速示例

### 1. 读取一个 float 寄存器

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 01 03 00 00 00 01 43 21

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
float value = frame.ReadSingle();

Console.WriteLine(value); // 9.9385
```

### 2. 写入一个 float 寄存器

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000005;

byte[] requestBytes = QlProtocolCommandBuilder.BuildWriteFloat(deviceAddress, 0x164E, 0.0596f);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4

byte[] responseBytes =
{
    0x10, 0x00, 0x00, 0x05,
    0x06,
    0x16, 0x4E,
    0x01,
    0x60,
    0x2A, 0x82
};

QlProtocolFrame frame = QlProtocolParser.Parse(responseBytes);

Console.WriteLine(frame.Kind); // WriteResponse
Console.WriteLine($"0x{frame.ResponseCode.GetValueOrDefault():X2}"); // 0x60
```

### 3. `0x32` 指令转发

`0x32` 的当前约定为：

```text
设备地址(4) + 0x32(1) + 数据长度(2) + 端口ID(1) + 转发内容(N) + CRC16(2)
```

其中：

- 数据长度 = `端口ID + 转发内容` 的总字节数
- `CRC16` 仍然按低字节在前发送
- 转发内容本身通常就是另一条完整的正常协议命令

从 `0.5.0` 开始，可以直接用正式 API：

```csharp
using QLProtocolLibrary;

byte[] forwardedCommand = QlHexConverter.FromHexString(
    "10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9");

byte[] forwardFrame = QlProtocolCommandBuilder.BuildForward(
    0x1000000F,
    0x01,
    forwardedCommand);

Console.WriteLine(QlHexConverter.ToHexString(forwardFrame));
// 10 00 00 0F 32 00 11 01 10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9 CE D2

QlProtocolFrame parsed = QlProtocolParser.Parse(forwardFrame);

Console.WriteLine(parsed.ReadForwardPortId()); // 1
Console.WriteLine(QlHexConverter.ToHexString(parsed.ReadForwardContent()));
// 10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9
```

## 推荐理解方式

这个库分两层：

### 第一层：高层已知寄存器 API

适合已经有明确寄存器定义的业务调用。

例如：

- `QlProtocolKnownCommands.BuildReadDeviceTime(deviceAddress)`
- `QlProtocolKnownCommands.BuildReadRunStatus(deviceAddress)`
- `QlKnownOperations.DeviceTime.BuildRead(deviceAddress)`
- `QlProtocolKnownParsers.TryParseRunStatus(frame, out var status)`

### 第二层：通用协议 API

适合调试协议、扩展自定义地址、处理原始报文。

例如：

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolCommandBuilder.BuildForward(...)`
- `QlProtocolParser.Parse(...)`
- `frame.ReadSingle()`
- `frame.ReadForwardPortId()`

## 公开 API

### `QlProtocolCommandBuilder`

常用方法：

- `BuildPacket(uint deviceAddress, byte rawFunctionCode, byte[] functionData, bool includeEnvelope = false)`
- `BuildRead(uint deviceAddress, ushort address, ushort registerCount, bool includeEnvelope = false)`
- `BuildWrite(uint deviceAddress, ushort address, ushort registerCount, byte[] payload, bool includeEnvelope = false)`
- `BuildWriteRegisters(uint deviceAddress, ushort address, params ushort[] registers)`
- `BuildWriteFloat(uint deviceAddress, ushort address, params float[] values)`
- `BuildWriteUtf8(uint deviceAddress, ushort address, string value, int fixedByteLength = 0)`
- `BuildSetTime(uint deviceAddress, DateTime value)`
- `BuildForward(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)`

### `QlProtocolParser`

常用方法：

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

### `QlProtocolFrameExtensions`

常用方法：

- `ReadUInt16()`
- `ReadUInt32()`
- `ReadSingle()`
- `ReadSingles()`
- `ReadUtf8()`
- `ReadAscii()`
- `ReadBcdDateTime()`
- `ReadBcdDateTimeText()`
- `ReadUInt16Array()`
- `ReadForwardPortId()`
- `ReadForwardContent()`

### 高层已知业务 API

- `QlProtocolKnownCommands`
- `QlProtocolKnownParsers`
- `QlKnownOperations`
- `QlProtocolKnownRouter`

## 字节序注意事项

这份协议不能简单理解成“所有字段都同一种字节序”。

建议这样理解：

- 协议控制字段
  例如寄存器地址、寄存器数量、长度字段
  按文档约定使用高字节在前
- 数据值字段
  例如 `WORD`、`FLOAT`、日志内容、操作参数
  必须按具体功能码章节和数据类型定义处理

当前库中的辅助方法拆分为：

- `EncodeUInt16 / DecodeUInt16`
  用于协议控制字段
- `EncodeValueUInt16 / DecodeValueUInt16`
  用于 16 位数据值
- `EncodeUInt32 / DecodeUInt32`
  用于 32 位数据值
- `EncodeSingle / DecodeSingle`
  用于浮点值

## 当前能力边界

- `0x03 / 0x06` 已经具备明确的构包和解包支持
- `0x32` 已经具备正式的构包 helper 和解析 helper
- `0x08 / 0x23 / 0x26 / 0x30 / 0x33` 当前仍以通用结构解析为主
- 更细的业务字段解释，仍建议按项目实际文档章节继续扩展

## 仓库结构

- `src/QLProtocolLibrary`：类库源码
- `examples/QLProtocolLibrary.Demo`：源码引用示例
- `examples/QLProtocolLibrary.NuGetDemo`：NuGet 使用示例
- `tests/QLProtocolLibrary.Tests`：单元测试
- `docs`：API 文档与发布文档

## 文档导航

- NuGet 包 README：[src/QLProtocolLibrary/README.md](src/QLProtocolLibrary/README.md)
- API 参考（中文）：[docs/API.zh-CN.md](docs/API.zh-CN.md)
- API Reference (English)：[docs/API.en.md](docs/API.en.md)
- 发布清单（中文）：[docs/PUBLISHING.zh-CN.md](docs/PUBLISHING.zh-CN.md)
- Publishing checklist (English)：[docs/PUBLISHING.en.md](docs/PUBLISHING.en.md)
- 版本变更：[CHANGELOG.md](CHANGELOG.md)

## 项目链接

- GitHub 仓库：https://github.com/zpczpc/QLProtocolLibrary
- Issue 反馈：https://github.com/zpczpc/QLProtocolLibrary/issues
- NuGet 包：https://www.nuget.org/packages/QLProtocolLibrary
- 许可证：MIT，见 [LICENSE](LICENSE)
