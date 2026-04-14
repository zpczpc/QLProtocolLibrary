# QLProtocolLibrary

[![CI](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml)
[![NuGet](https://img.shields.io/nuget/v/QLProtocolLibrary?logo=nuget)](https://www.nuget.org/packages/QLProtocolLibrary)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

中文 | [English](README.en.md)

`QLProtocolLibrary` 是一个面向 QL 设备通讯协议的 C# 类库，当前实现对齐的是主应用层报文结构。

它的目标很直接：

1. 让调用方直接组装协议报文
2. 让调用方直接解析协议报文

也就是说，这个库首先解决的是“发一个协议报文，收一个协议报文”的问题，而不是额外引入另一套自定义外层协议。

## 协议结构

文档中的主协议裸报文按下面的结构组织：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

常见功能码：

- `0x03`：读寄存器
- `0x06`：写寄存器
- `0x08`：操作指令
- `0x23`：读取日志
- `0x26`：写日志
- `0x30`：TF 目录/文件读取
- `0x32`：指令转发
- `0x33`：数据库读取

文档还定义了一层可选包络：

```text
C6 F4 C2 CC + 长度(2) + 裸报文 + 0D 0A
```

这个包络不是主协议的必选项。对于“发一帧，收一帧”的常规场景，直接处理裸报文即可。

## 重要说明

- 设备地址固定 4 字节，例如 `10 00 00 01`
- CRC 使用 Modbus CRC16，发送顺序为低字节在前
- 寄存器数量按文档是“1 个寄存器 = 4 字节”
- 协议控制字段如地址、寄存器数量、长度字段，按高字节在前
- 数据值区的字节序不能一概而论，必须看具体功能码和具体数据类型
- 当前库已经按文档示例处理常用寄存器读写里的 `WORD/FLOAT` 数据值

这点非常重要：这份协议不是“所有数据都同一种字节序”。不同功能码、不同数据类型、不同章节的约定都可能不同。

## 安装

```bash
dotnet add package QLProtocolLibrary
```

## 最短接入方式

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

// 读取从 00 00 开始的 1 个寄存器
byte[] command = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(command));
// 10 00 00 01 03 00 00 00 01 43 21
```

## 快速示例

### 1. 读取一个 float 寄存器

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

// 发送报文：读取从 00 00 开始的 1 个寄存器
byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 01 03 00 00 00 01 43 21

// 实际项目里，这个 byte[] 来自串口 / 485 / TCP 接收
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

// 发送报文：向 16 4E 写入 1 个 float 值 0.0596
byte[] requestBytes = QlProtocolCommandBuilder.BuildWriteFloat(deviceAddress, 0x164E, 0.0596f);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4

// 实际项目里，这个 byte[] 来自设备应答
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
Console.WriteLine(frame.Address.ToString("X4")); // 164E
Console.WriteLine(frame.ByteCount); // 1
Console.WriteLine($"0x{frame.ResponseCode.GetValueOrDefault():X2}"); // 0x60
```

## 推荐理解方式

这个库分两层：

### 第一层：高层已知寄存器 API

给已经整理出“寄存器地址 + 数据结构”的业务调用方使用。

例如：

- `QlProtocolKnownCommands.BuildReadDeviceTime(deviceAddress)`
- `QlProtocolKnownCommands.BuildReadRunStatus(deviceAddress)`
- `QlKnownOperations.DeviceTime.BuildRead(deviceAddress)`
- `QlProtocolKnownParsers.TryParseRunStatus(frame, out var status)`

### 第二层：通用协议 API

给调试协议、扩展寄存器、或者只想处理原始报文的人使用。

例如：

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolParser.Parse(...)`
- `frame.ReadSingle()`
- `frame.ReadUInt16()`
- `frame.Decode(QlKnownRegisters.Concentration)`

## 公开 API

### 1. `QlProtocolCommandBuilder`

这是底层组包入口。

常用方法：

- `BuildPacket(uint deviceAddress, byte rawFunctionCode, byte[] functionData, bool includeEnvelope = false)`
- `BuildRead(uint deviceAddress, ushort address, ushort registerCount, bool includeEnvelope = false)`
- `BuildRead(uint deviceAddress, QlRegisterDefinition register, bool includeEnvelope = false)`
- `BuildWrite(uint deviceAddress, ushort address, ushort registerCount, byte[] payload, bool includeEnvelope = false)`
- `BuildWrite(uint deviceAddress, QlRegisterDefinition register, byte[] payload, bool includeEnvelope = false)`
- `BuildWriteRegisters(uint deviceAddress, ushort address, params ushort[] registers)`
- `BuildWriteFloat(uint deviceAddress, ushort address, params float[] values)`
- `BuildWriteUtf8(uint deviceAddress, ushort address, string value, int fixedByteLength = 0)`
- `BuildSetTime(uint deviceAddress, DateTime value)`

### 2. `QlProtocolParser`

这是底层解包入口。

常用方法：

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

说明：

- 实际串口/网络通讯场景，优先用 `Parse(byte[])`
- `ParseHex(...)` 主要适合调试、单元测试、文档示例对拍

### 3. `QlProtocolFrame`

这是解析结果对象。

常用属性：

- `RawBytes`
- `HasEnvelope`
- `DeviceAddress`
- `DeviceAddressHex`
- `RawFunctionCode`
- `FunctionCode`
- `Kind`
- `Address`
- `RegisterCount`
- `Payload`
- `ByteCount`
- `DataLength`
- `ResponseCode`
- `Crc`
- `ComputedCrc`
- `IsCrcValid`

### 4. `QlProtocolFrameExtensions`

这是按类型读取 payload 的通用入口。

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
- `Decode(QlRegisterDefinition register)`
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`

### 5. `QlPayloadCodec`

这是低层数据编解码工具。

常用方法：

- `EncodeUInt16 / DecodeUInt16`
  说明：用于协议字段，按高字节在前
- `EncodeValueUInt16 / DecodeValueUInt16`
  说明：用于 16 位数据值，按低字节在前
- `EncodeUInt32 / DecodeUInt32`
  说明：用于 32 位数据值
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

### 6. `QlProtocolKnownCommands`

这是高层已知寄存器组包入口。

常用方法：

- `BuildReadDeviceTime(uint deviceAddress)`
- `BuildReadRunStatus(uint deviceAddress)`
- `BuildReadDeviceNo(uint deviceAddress)`
- `BuildReadAnalyzerCode(uint deviceAddress)`
- `BuildReadMeasureResult(uint deviceAddress)`
- `BuildReadConcentration(uint deviceAddress)`
- `BuildReadKbInfo(uint deviceAddress)`
- `BuildReadMeterStrongLight(uint deviceAddress)`
- `BuildReadVersionBundle(uint deviceAddress)`
- `BuildSetDeviceTime(uint deviceAddress, DateTime value)`

### 7. `QlProtocolKnownParsers`

这是高层已知寄存器解析入口。

常用方法：

- `TryParseDeviceTime`
- `TryParseDeviceNo`
- `TryParseAnalyzerCode`
- `TryParseConcentration`
- `TryParseRunStatus`
- `TryParseMeasureResult`
- `TryParseKbInfo`
- `TryParseMeterStrongLight`
- `TryParseVersionBundle`

### 8. `QlKnownOperations`

这是“寄存器定义 + 读命令 + 响应解析”的统一目录。

例如：

```csharp
uint deviceAddress = 0x10000001;

byte[] cmd = QlKnownOperations.DeviceTime.BuildRead(deviceAddress);

if (QlKnownOperations.DeviceTime.TryParse(frame, out DateTime time))
{
    Console.WriteLine(time);
}
```

### 9. `QlProtocolKnownRouter`

这是统一已知响应路由器，适合收到一帧后自动分派业务结果。

```csharp
if (QlProtocolKnownRouter.TryParse(frame, out var result) && result != null)
{
    Console.WriteLine(result.Name);
    Console.WriteLine(result.Value);
}
```

## 当前能力

- 按文档主结构组包
- 按文档主结构解析
- CRC16 校验
- 可选包络报文解析
- 可选包络流拆分
- 常用寄存器高层 API
- 常用寄存器高层解析 API
- 通用类型读取 API
- 已知业务操作目录与统一响应路由

## 当前边界

下面这点建议你在使用前心里有数：

- 这个库已经支持主应用层协议里的常用寄存器读写和常见功能码基础解析
- `0x08 / 0x23 / 0x26 / 0x30 / 0x32 / 0x33` 当前已支持通用结构解析
- 这些功能码的数据区业务语义更复杂，具体字段解释仍应以文档对应章节为准
- 如果你遇到新寄存器或新业务，可以优先走通用 API，再按项目需要补高层包装

## 仓库结构

- `src/QLProtocolLibrary`：类库源码
- `examples/QLProtocolLibrary.Demo`：源码引用示例
- `examples/QLProtocolLibrary.NuGetDemo`：NuGet 使用示例
- `tests/QLProtocolLibrary.Tests`：单元测试
- `docs`：API 文档和发布说明

## 文档导航

- NuGet 包说明：[src/QLProtocolLibrary/README.md](src/QLProtocolLibrary/README.md)
- API 参考（中文）：[docs/API.zh-CN.md](docs/API.zh-CN.md)
- API reference (English): [docs/API.en.md](docs/API.en.md)
- 发布清单（中文）：[docs/PUBLISHING.zh-CN.md](docs/PUBLISHING.zh-CN.md)
- Publishing checklist (English): [docs/PUBLISHING.en.md](docs/PUBLISHING.en.md)
- 版本变更：[CHANGELOG.md](CHANGELOG.md)
- 源码示例：[examples/QLProtocolLibrary.Demo/Program.cs](examples/QLProtocolLibrary.Demo/Program.cs)
- NuGet 使用示例：[examples/QLProtocolLibrary.NuGetDemo/Program.cs](examples/QLProtocolLibrary.NuGetDemo/Program.cs)

## 项目链接

- GitHub 仓库：https://github.com/zpczpc/QLProtocolLibrary
- Issue 反馈：https://github.com/zpczpc/QLProtocolLibrary/issues
- NuGet 包：https://www.nuget.org/packages/QLProtocolLibrary
- 许可证：MIT，见 `LICENSE`
