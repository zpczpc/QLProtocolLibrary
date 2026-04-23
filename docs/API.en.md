# API Reference

## Overview

This library implements the main application-layer packet model used by the project.

Default bare packet format:

```text
DeviceAddress(4) + FunctionCode(1) + FunctionData(N) + CRC16(2)
```

Optional envelope format:

```text
C6 F4 C2 CC + Length(2) + BarePacket + 0D 0A
```

If your workflow is simply “send one frame, receive one frame”, the bare packet APIs are the primary path.

## Usage layers

Recommended order from highest level to lowest level:

1. `QlKnownOperations`
2. `QlProtocolKnownRouter`
3. `QlProtocolKnownCommands` / `QlProtocolKnownParsers`
4. `QlProtocolCommandBuilder` / `QlProtocolParser` / `QlProtocolFrameExtensions`
5. `QlPayloadCodec`

## Byte-order notes

This protocol must not be treated as “one global byte-order rule for everything”.

Recommended interpretation:

- protocol control fields
  such as register addresses, register counts, and length fields
  use high-byte-first order
- payload value fields
  such as `WORD`, `FLOAT`, log content, and operation parameters
  must follow the specific function-code section and type definition

Current helper split:

- `EncodeUInt16 / DecodeUInt16`
  for protocol control fields
- `EncodeValueUInt16 / DecodeValueUInt16`
  for 16-bit payload values
- `EncodeUInt32 / DecodeUInt32`
  for 32-bit payload values
- `EncodeSingle / DecodeSingle`
  for floating-point payload values

## High-level entry points

### `QlKnownOperations`

Purpose: combine register metadata, read-command building, and typed response parsing.

Common members:

- `QlKnownOperations.DeviceTime`
- `QlKnownOperations.RunStatus`
- `QlKnownOperations.DeviceNo`
- `QlKnownOperations.MeasureResult`
- `QlKnownOperations.VersionBundle`

Common methods:

- `BuildRead(uint deviceAddress)`
- `BuildReadHex(uint deviceAddress)`
- `TryParse(QlProtocolFrame frame, out T value)`
- `Parse(QlProtocolFrame frame)`

### `QlProtocolKnownRouter`

Purpose: route a known business frame to the correct typed result.

Common method:

- `TryParse(QlProtocolFrame frame, out QlKnownParseResult? result)`

### `QlProtocolKnownCommands`

Purpose: build commands by business intent without exposing register addresses.

Typical methods:

- `BuildReadDeviceTime(uint deviceAddress)`
- `BuildReadRunStatus(uint deviceAddress)`
- `BuildReadDeviceNo(uint deviceAddress)`
- `BuildReadMeasureResult(uint deviceAddress)`
- `BuildReadVersionBundle(uint deviceAddress)`
- `BuildSetDeviceTime(uint deviceAddress, DateTime value)`
- `BuildWriteDeviceNo(uint deviceAddress, string deviceNo, int fixedByteLength = 16)`
- `BuildWriteAnalyzerCode(uint deviceAddress, string analyzerCode, int fixedByteLength = 16)`

### `QlProtocolKnownParsers`

Purpose: parse responses for known business registers.

Typical methods:

- `TryParseDeviceTime`
- `TryParseRunStatus`
- `TryParseDeviceNo`
- `TryParseAnalyzerCode`
- `TryParseConcentration`
- `TryParseMeasureResult`
- `TryParseKbInfo`
- `TryParseMeterStrongLight`
- `TryParseVersionBundle`

## Generic entry points

### `QlProtocolCommandBuilder`

Purpose: build frames directly from device address, function code, and function data.

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
- `BuildForward(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)`
- `BuildForwardHex(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)`

Example:

```csharp
uint deviceAddress = 0x10000001;
byte[] cmd = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 01 03 00 00 00 01 43 21
```

`0x32` forwarding example:

```csharp
byte[] forwardedContent = QlHexConverter.FromHexString(
    "10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9");

byte[] cmd = QlProtocolCommandBuilder.BuildForward(
    0x1000000F,
    0x01,
    forwardedContent);

Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 0F 32 00 11 01 10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9 CE D2
```

### `QlProtocolParser`

Purpose: parse a full frame into `QlProtocolFrame`.

Common methods:

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

Notes:

- supports bare frames
- supports optionally wrapped frames

### `QlProtocolFrame`

Purpose: carry parsed frame results.

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
- `ErrorCode`
- `Crc`
- `ComputedCrc`
- `IsCrcValid`

### `QlProtocolFrameExtensions`

Purpose: read typed values from parsed payload bytes.

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
- `Decode(QlRegisterDefinition register)`
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`

`0x32` parse example:

```csharp
QlProtocolFrame frame = QlProtocolParser.ParseHex(
    "10 00 00 0F 32 00 11 01 10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9 CE D2");

byte portId = frame.ReadForwardPortId();
byte[] content = frame.ReadForwardContent();

Console.WriteLine(portId); // 1
Console.WriteLine(QlHexConverter.ToHexString(content));
```

### `QlPayloadCodec`

Purpose: low-level field and payload-value encoding/decoding helpers.

Common methods:

- `EncodeUInt16 / DecodeUInt16`
- `EncodeValueUInt16 / DecodeValueUInt16`
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

## Function-code coverage

### `0x03`

Current support:

- request building
- request parsing
- response parsing
- high-level known-register read support

### `0x06`

Current support:

- request building
- request parsing
- simple response parsing

### `0x32`

Current support:

- dedicated request helper: `BuildForward(...)`
- dedicated hex helper: `BuildForwardHex(...)`
- generic structural parsing
- dedicated parse helpers: `ReadForwardPortId()` / `ReadForwardContent()`

Structure:

```text
DeviceAddress(4) + 0x32(1) + DataLength(2) + PortId(1) + ForwardedContent(N) + CRC16(2)
```

Notes:

- `DataLength = PortId + ForwardedContent`
- `ForwardedContent` is usually another complete normal QL command

### `0x08 / 0x23 / 0x26 / 0x30 / 0x33`

Current support:

- generic structural parsing

Notes:

- these function codes have richer business payload definitions
- the library currently focuses on structural support first
- deeper business interpretation can be added on top as needed

## Common result models

- `QlRunStatusInfo`
- `QlMeasureResultInfo`
- `QlKbInfo`
- `QlMeterStrongLightInfo`
- `QlVersionBundleInfo`
- `QlDecodedRegisterValue`
- `QlKnownParseResult`

## Built-in register catalog

Known register definitions are provided in `QlKnownRegisters`:

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

## Selection advice

For business callers:

- prefer `QlKnownOperations`
- or `QlProtocolKnownCommands + QlProtocolKnownParsers`

For protocol extension work:

- prefer `QlProtocolCommandBuilder`
- then combine it with `QlProtocolParser + QlProtocolFrameExtensions`

For simple “send one frame, receive one frame” flows:

- `Build...` + `Parse...` is usually enough

## `QlProtocolStreamDecoder`

Purpose: decode only the optional wrapped stream format.

Important:

- it handles `C6 F4 C2 CC + Length + BarePacket + 0D 0A`
- bare packets themselves do not carry their own stream boundary marker
- for bare packets, the transport layer must preserve packet boundaries or you must manage framing externally
