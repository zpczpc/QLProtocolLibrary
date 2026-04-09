# QLProtocolLibrary

[中文](README.md) | English

`QLProtocolLibrary` is designed to hide QL device protocol details from callers and let them focus on two things only:

1. build protocol frames
2. parse protocol frames

It supports both styles at the same time:

- high-level APIs that hide register addresses
- generic typed APIs for lower-level protocol work

## Problems this package solves

- no manual `AA 55 ... BB 55` frame assembly
- no manual CRC16 calculation
- no custom TCP sticky-packet handling
- no need to memorize common register addresses and lengths
- no repeated byte slicing from raw `byte[]`

## Mental model

This package is split into two layers.

### Layer 1: high-level business APIs

For callers who do not want to care about register addresses or payload structure.

Examples:

- `QlProtocolKnownCommands.BuildReadDeviceTime("1001")`
- `QlProtocolKnownCommands.BuildReadRunStatus("1001")`
- `QlProtocolKnownParsers.TryParseDeviceTime(frame, out var time)`
- `QlProtocolKnownParsers.TryParseRunStatus(frame, out var status)`

### Layer 2: generic protocol APIs

For callers who need to extend registers, debug frames, or build lower-level abstractions.

Examples:

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolParser.Parse(...)`
- `frame.ReadUInt16()`
- `frame.ReadSingle()`
- `frame.Decode(QlKnownRegisters.DeviceNo)`

## Frame format

Each frame is wrapped by:

- header: `AA 55`
- footer: `BB 55`

Common body layout:

- `MN`: 8 bytes
- `FunctionCode`: 1 byte
- `Address`: 2 bytes
- `Payload / RegisterCount / ByteCount`: function dependent
- `CRC16`: 2 bytes, low byte first

## Quick start

```csharp
using QLProtocolLibrary;

var command = QlProtocolKnownCommands.BuildReadDeviceTime("1001");
var hex = QlHexConverter.ToHexString(command);
Console.WriteLine(hex);

var frame = QlProtocolParser.ParseHex(
    "AA 55 00 00 00 00 00 00 03 E9 03 00 D0 06 26 04 09 08 30 45 36 87 BB 55");

if (QlProtocolKnownParsers.TryParseDeviceTime(frame, out var deviceTime))
{
    Console.WriteLine(deviceTime.ToString("yyyy-MM-dd HH:mm:ss"));
}
```

## Public APIs

### 1. `QlProtocolKnownCommands`

High-level command entry point. Callers do not need to know register addresses.

Common methods:

- `BuildReadDeviceTime(string mn)`
- `BuildReadRunStatus(string mn)`
- `BuildReadDeviceNo(string mn)`
- `BuildReadAnalyzerCode(string mn)`
- `BuildReadMeasureResult(string mn)`
- `BuildReadConcentration(string mn)`
- `BuildReadKbInfo(string mn)`
- `BuildReadMeterStrongLight(string mn)`
- `BuildReadVersionBundle(string mn)`
- `BuildSetDeviceTime(string mn, DateTime value)`
- `BuildWriteDeviceNo(string mn, string deviceNo, int fixedByteLength = 16)`
- `BuildWriteAnalyzerCode(string mn, string analyzerCode, int fixedByteLength = 16)`

Return values:

- binary methods return full protocol frame `byte[]`
- `Hex` methods return hex strings

### 2. `QlProtocolKnownParsers`

High-level result parsers. Callers do not need to manually interpret payload bytes.

Common methods:

- `TryParseDeviceTime(QlProtocolFrame frame, out DateTime value)`
- `TryParseDeviceNo(QlProtocolFrame frame, out string? value)`
- `TryParseAnalyzerCode(QlProtocolFrame frame, out string? value)`
- `TryParseConcentration(QlProtocolFrame frame, out float value)`
- `TryParseRunStatus(QlProtocolFrame frame, out QlRunStatusInfo? value)`
- `TryParseMeasureResult(QlProtocolFrame frame, out QlMeasureResultInfo? value)`
- `TryParseKbInfo(QlProtocolFrame frame, out QlKbInfo? value)`
- `TryParseMeterStrongLight(QlProtocolFrame frame, out QlMeterStrongLightInfo? value)`
- `TryParseVersionBundle(QlProtocolFrame frame, out QlVersionBundleInfo? value)`

### 3. `QlProtocolCommandBuilder`

Generic command builder for callers who want to specify addresses directly.

### 4. `QlProtocolParser`

Parses complete frames into `QlProtocolFrame`.

### 5. `QlProtocolFrame`

Represents parsed frame metadata and payload.

### 6. `QlProtocolFrameExtensions`

Generic typed payload readers.

Common methods:

- `ReadUInt16()`
- `ReadUInt32()`
- `ReadSingle()`
- `ReadSingles()`
- `ReadUtf8()`
- `ReadAscii()`
- `ReadBcdDateTime()`
- `ReadUInt16Array()`
- `Decode(QlRegisterDefinition register)`

### 7. `QlPayloadCodec`

Low-level encoding/decoding helpers.

## Typed result models

High-level parsers return these models when needed:

- `QlRunStatusInfo`
- `QlMeasureResultInfo`
- `QlKbInfo`
- `QlMeterStrongLightInfo`
- `QlVersionBundleInfo`
- plus primitive types like `DateTime`, `float`, and `string`

## Built-in register catalog

Provided in `QlKnownRegisters`:

- `DeviceNo`
- `MeasureResult`
- `RunStatus`
- `SubStatus`
- `RunMode`
- `MeasureMode`
- `WarnCode`
- `FaultCode`
- `DeviceTime`
- `Concentration`
- `WorkStateFlag`
- `KbInfo`
- `MeterStrongLight`
- `AnalyzerCode`
- `VersionBundle`

## Two supported styles

### Style 1: no register address concerns

```csharp
var cmd = QlProtocolKnownCommands.BuildReadDeviceTime("1001");
var frame = QlProtocolParser.Parse(bytes);
if (QlProtocolKnownParsers.TryParseDeviceTime(frame, out var time))
{
    Console.WriteLine(time);
}
```

### Style 2: generic typed parsing

```csharp
var frame = QlProtocolParser.Parse(bytes);
var time = frame.ReadBcdDateTime();
var value = frame.ReadSingle();
var text = frame.ReadUtf8();
var typed = frame.Decode(QlKnownRegisters.DeviceNo);
```

## Recommended usage style

For business applications, prefer this flow:

1. `QlProtocolKnownCommands`
2. `QlProtocolStreamDecoder`
3. `QlProtocolParser`
4. `QlProtocolKnownParsers`

Drop down to generic APIs only when you need protocol extension or debugging.

## Demo project

Runnable sample:

- `examples/QLProtocolLibrary.Demo`

Repository:

- `https://github.com/zpczpc/QLProtocolLibrary`


## More SDK-style usage

Two additional entry points are now available.

### `QlKnownOperations`

This is a known-operation catalog. Each operation contains:

- register metadata
- read-command builder
- typed response parser

Example:

```csharp
var cmd = QlKnownOperations.DeviceTime.BuildRead("1001");
var hex = QlKnownOperations.RunStatus.BuildReadHex("1001");

if (QlKnownOperations.DeviceTime.TryParse(frame, out DateTime time))
{
    Console.WriteLine(time);
}
```

### `QlProtocolKnownRouter`

This is a unified known-response router.

```csharp
if (QlProtocolKnownRouter.TryParse(frame, out var result) && result != null)
{
    Console.WriteLine(result.Name);
    Console.WriteLine(result.Value);
}
```
