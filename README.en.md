# QLProtocolLibrary

[![CI](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml)
[![NuGet](https://img.shields.io/nuget/v/QLProtocolLibrary?logo=nuget)](https://www.nuget.org/packages/QLProtocolLibrary)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

[中文](README.md) | English

`QLProtocolLibrary` is a C# library for the QL device communication protocol. The current implementation aligns with the main application-layer packet structure used by this project.

The goal is simple:

1. build protocol packets
2. parse protocol packets

In other words, this library focuses on the practical workflow of sending one protocol packet and receiving one protocol packet, without introducing a separate custom outer framing model.

## Protocol structure

The main bare packet format in the document is:

```text
DeviceAddress(4) + FunctionCode(1) + FunctionData(N) + CRC16(2)
```

Common function codes:

- `0x03`: read registers
- `0x06`: write registers
- `0x08`: operation command
- `0x23`: read logs
- `0x26`: write logs
- `0x30`: TF directory/file read
- `0x32`: command forwarding
- `0x33`: database read

The document also defines an optional envelope:

```text
C6 F4 C2 CC + Length(2) + BarePacket + 0D 0A
```

That envelope is optional. For the normal “send one frame, receive one frame” flow, the bare packet is usually enough.

## Important notes

- device address is always 4 bytes, for example `10 00 00 01`
- CRC uses Modbus CRC16 and is transmitted low byte first
- one register is `4 bytes` in this protocol
- control fields such as addresses, counts, and length fields use high-byte-first order
- payload value byte order is not globally uniform and must follow the specific function-code section in the protocol document
- the current library already matches the document examples for common register `WORD` and `FLOAT` payloads

This matters a lot: this protocol does not use one universal byte-order rule for every payload.

## Installation

```bash
dotnet add package QLProtocolLibrary
```

## Shortest start

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

// Read 1 register starting from 0x0000
byte[] command = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(command));
// 10 00 00 01 03 00 00 00 01 43 21
```

## Quick example

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

## Recommended mental model

The library has two layers.

### Layer 1: high-level known-register APIs

For business code that already works with known register definitions.

Examples:

- `QlProtocolKnownCommands.BuildReadDeviceTime(deviceAddress)`
- `QlProtocolKnownCommands.BuildReadRunStatus(deviceAddress)`
- `QlKnownOperations.DeviceTime.BuildRead(deviceAddress)`
- `QlProtocolKnownParsers.TryParseRunStatus(frame, out var status)`

### Layer 2: generic protocol APIs

For protocol debugging, custom register work, or raw packet processing.

Examples:

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolParser.Parse(...)`
- `frame.ReadSingle()`
- `frame.ReadUInt16()`
- `frame.Decode(QlKnownRegisters.Concentration)`

## Public APIs

### 1. `QlProtocolCommandBuilder`

Low-level packet builder.

Common methods:

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

Low-level packet parser.

Common methods:

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

Notes:

- use `Parse(byte[])` for real serial or network input
- `ParseHex(...)` is mainly a debugging and testing helper

### 3. `QlProtocolFrame`

Parsed frame object.

Common properties:

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

Typed payload readers.

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
- `Decode(QlRegisterDefinition register)`
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`

### 5. `QlPayloadCodec`

Low-level encoding and decoding helpers.

Common methods:

- `EncodeUInt16 / DecodeUInt16`
  Note: for protocol control fields, high byte first
- `EncodeValueUInt16 / DecodeValueUInt16`
  Note: for 16-bit payload values, low byte first
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

### 6. `QlProtocolKnownCommands`

High-level known-register command builders.

Typical methods:

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

High-level known-register response parsers.

Typical methods:

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

Unified catalog that combines register metadata, read-command building, and typed parsing.

```csharp
uint deviceAddress = 0x10000001;

byte[] cmd = QlKnownOperations.DeviceTime.BuildRead(deviceAddress);

if (QlKnownOperations.DeviceTime.TryParse(frame, out DateTime time))
{
    Console.WriteLine(time);
}
```

### 9. `QlProtocolKnownRouter`

Unified known-response router.

```csharp
if (QlProtocolKnownRouter.TryParse(frame, out var result) && result != null)
{
    Console.WriteLine(result.Name);
    Console.WriteLine(result.Value);
}
```

## Current capabilities

- packet building for the documented main structure
- packet parsing for the documented main structure
- CRC16 validation
- optional envelope parsing
- optional envelope stream decoding
- high-level APIs for common known registers
- generic typed payload readers
- known-operation catalog and unified response router

## Current boundary

- the main application-layer protocol is covered
- `0x08 / 0x23 / 0x26 / 0x30 / 0x32 / 0x33` currently have generic structural parsing support
- the detailed business meaning of those payloads should still be interpreted using the corresponding document sections
- for new registers or new business fields, prefer generic APIs first and add high-level wrappers only when needed

## Repository layout

- `src/QLProtocolLibrary`: library source
- `examples/QLProtocolLibrary.Demo`: source-based demo
- `examples/QLProtocolLibrary.NuGetDemo`: NuGet usage sample
- `tests/QLProtocolLibrary.Tests`: unit tests
- `docs`: API docs and publishing docs

## Documentation index

- NuGet package README: [src/QLProtocolLibrary/README.md](src/QLProtocolLibrary/README.md)
- API reference (Chinese): [docs/API.zh-CN.md](docs/API.zh-CN.md)
- API reference (English): [docs/API.en.md](docs/API.en.md)
- Publishing checklist (Chinese): [docs/PUBLISHING.zh-CN.md](docs/PUBLISHING.zh-CN.md)
- Publishing checklist (English): [docs/PUBLISHING.en.md](docs/PUBLISHING.en.md)
- Changelog: [CHANGELOG.md](CHANGELOG.md)
- Source-based demo: [examples/QLProtocolLibrary.Demo/Program.cs](examples/QLProtocolLibrary.Demo/Program.cs)
- NuGet usage sample: [examples/QLProtocolLibrary.NuGetDemo/Program.cs](examples/QLProtocolLibrary.NuGetDemo/Program.cs)

## Project links

- GitHub repository: https://github.com/zpczpc/QLProtocolLibrary
- Issue tracker: https://github.com/zpczpc/QLProtocolLibrary/issues
- NuGet package: https://www.nuget.org/packages/QLProtocolLibrary
- License: MIT, see `LICENSE`
