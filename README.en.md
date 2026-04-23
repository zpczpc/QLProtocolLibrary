# QLProtocolLibrary

[![CI](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml)
[![NuGet](https://img.shields.io/nuget/v/QLProtocolLibrary?logo=nuget)](https://www.nuget.org/packages/QLProtocolLibrary)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

[中文](README.md) | English

`QLProtocolLibrary` is a C# library for the QL device application-layer protocol.

The main bare packet structure handled by the library is:

```text
DeviceAddress(4) + FunctionCode(1) + FunctionData(N) + CRC16(2)
```

It also supports the optional document envelope:

```text
C6 F4 C2 CC + Length(2) + BarePacket + 0D 0A
```

The goal is simple:

1. build protocol packets directly
2. parse protocol packets directly

## Good fit for

- upper-computer applications that send and receive protocol packets directly
- device debugging and protocol testing tools
- reusable protocol wrappers shared across projects
- external projects consuming QL protocol support via NuGet

## Installation

```bash
dotnet add package QLProtocolLibrary
```

## Smallest example

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(requestBytes));
// 10 00 00 01 03 00 00 00 01 43 21
```

## Common function codes

- `0x03`: read registers
- `0x06`: write registers
- `0x08`: operation command
- `0x23`: read logs
- `0x26`: write logs
- `0x30`: TF directory/file read
- `0x32`: command forwarding
- `0x33`: database read

## Quick examples

### 1. Read a float register

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

### 2. Write a float register

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

### 3. `0x32` command forwarding

The current `0x32` shape is:

```text
DeviceAddress(4) + 0x32(1) + DataLength(2) + PortId(1) + ForwardedContent(N) + CRC16(2)
```

Where:

- `DataLength` is the total byte count of `PortId + ForwardedContent`
- `CRC16` is still transmitted low byte first
- `ForwardedContent` is usually another complete normal QL command

Starting from `0.5.0`, the library provides dedicated APIs:

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

## Recommended mental model

The library has two layers.

### Layer 1: high-level known-register APIs

For business code that already maps to known register definitions.

Examples:

- `QlProtocolKnownCommands.BuildReadDeviceTime(deviceAddress)`
- `QlProtocolKnownCommands.BuildReadRunStatus(deviceAddress)`
- `QlKnownOperations.DeviceTime.BuildRead(deviceAddress)`
- `QlProtocolKnownParsers.TryParseRunStatus(frame, out var status)`

### Layer 2: generic protocol APIs

For protocol debugging, custom packet work, and raw frame handling.

Examples:

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolCommandBuilder.BuildForward(...)`
- `QlProtocolParser.Parse(...)`
- `frame.ReadSingle()`
- `frame.ReadForwardPortId()`

## Public APIs

### `QlProtocolCommandBuilder`

Common methods:

- `BuildPacket(uint deviceAddress, byte rawFunctionCode, byte[] functionData, bool includeEnvelope = false)`
- `BuildRead(uint deviceAddress, ushort address, ushort registerCount, bool includeEnvelope = false)`
- `BuildWrite(uint deviceAddress, ushort address, ushort registerCount, byte[] payload, bool includeEnvelope = false)`
- `BuildWriteRegisters(uint deviceAddress, ushort address, params ushort[] registers)`
- `BuildWriteFloat(uint deviceAddress, ushort address, params float[] values)`
- `BuildWriteUtf8(uint deviceAddress, ushort address, string value, int fixedByteLength = 0)`
- `BuildSetTime(uint deviceAddress, DateTime value)`
- `BuildForward(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)`

### `QlProtocolParser`

Common methods:

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

### `QlProtocolFrameExtensions`

Common methods:

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

### High-level business APIs

- `QlProtocolKnownCommands`
- `QlProtocolKnownParsers`
- `QlKnownOperations`
- `QlProtocolKnownRouter`

## Byte-order notes

This protocol does not use one universal byte-order rule for every field.

Recommended interpretation:

- protocol control fields
  such as register address, register count, and length fields
  use high-byte-first order
- payload value fields
  such as `WORD`, `FLOAT`, log content, and operation parameters
  must follow the corresponding function-code section and data-type definition

Current helper split:

- `EncodeUInt16 / DecodeUInt16`
  for protocol control fields
- `EncodeValueUInt16 / DecodeValueUInt16`
  for 16-bit payload values
- `EncodeUInt32 / DecodeUInt32`
  for 32-bit payload values
- `EncodeSingle / DecodeSingle`
  for floating-point values

## Current boundary

- `0x03 / 0x06` have dedicated request and response helpers
- `0x32` now has dedicated build and parse helpers
- `0x08 / 0x23 / 0x26 / 0x30 / 0x33` are still mainly handled as generic structural frames
- deeper business-field interpretation can still be added on top according to the protocol document

## Repository layout

- `src/QLProtocolLibrary`: library source
- `examples/QLProtocolLibrary.Demo`: source-based demo
- `examples/QLProtocolLibrary.NuGetDemo`: NuGet usage sample
- `tests/QLProtocolLibrary.Tests`: unit tests
- `docs`: API and publishing docs

## Documentation index

- package README: [src/QLProtocolLibrary/README.md](src/QLProtocolLibrary/README.md)
- Chinese API reference: [docs/API.zh-CN.md](docs/API.zh-CN.md)
- English API reference: [docs/API.en.md](docs/API.en.md)
- changelog: [CHANGELOG.md](CHANGELOG.md)

## Project links

- GitHub repository: https://github.com/zpczpc/QLProtocolLibrary
- Issue tracker: https://github.com/zpczpc/QLProtocolLibrary/issues
- NuGet package: https://www.nuget.org/packages/QLProtocolLibrary
- License: MIT, see [LICENSE](LICENSE)
