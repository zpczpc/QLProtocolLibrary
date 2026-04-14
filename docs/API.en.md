# API Reference

## Overview

This library implements the main application-layer packet model used by this project.

Default bare packet format:

```text
DeviceAddress(4) + FunctionCode(1) + FunctionData(N) + CRC16(2)
```

Optional envelope format:

```text
C6 F4 C2 CC + Length(2) + BarePacket + 0D 0A
```

If your workflow is simply “send one packet, receive one packet”, the bare packet APIs are the primary path.

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
  must follow the specific function-code section and type definition in the document

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

Typical members:

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

Example:

```csharp
uint deviceAddress = 0x10000001;

byte[] cmd = QlKnownOperations.DeviceTime.BuildRead(deviceAddress);

if (QlKnownOperations.DeviceTime.TryParse(frame, out DateTime time))
{
    Console.WriteLine(time);
}
```

### `QlProtocolKnownRouter`

Purpose: route an incoming frame to a known business result automatically.

Common method:

- `TryParse(QlProtocolFrame frame, out QlKnownParseResult? result)`

Result object:

- `Name`
- `Register`
- `Value`
- `GetValue<T>()`

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

Purpose: parse known business-register responses.

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

Purpose: build packets directly from device address, function code, and function data.

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
- `BuildReadHex(...)`
- `BuildWriteHex(...)`
- `BuildSetTimeHex(...)`

Document example:

```csharp
uint deviceAddress = 0x10000001;
byte[] cmd = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 01 03 00 00 00 01 43 21
```

Write-float example:

```csharp
uint deviceAddress = 0x10000005;
byte[] cmd = QlProtocolCommandBuilder.BuildWriteFloat(deviceAddress, 0x164E, 0.0596f);
Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4
```

### `QlProtocolParser`

Purpose: parse a full packet into `QlProtocolFrame`.

Common methods:

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

Notes:

- supports bare packets
- supports optionally wrapped packets

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
- `Decode(QlRegisterDefinition register)`
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`

Example:

```csharp
// Read-float example
byte[] requestBytes = QlProtocolCommandBuilder.BuildRead(0x10000001, 0x0000, 0x0001);
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

Write-float response example:

```csharp
byte[] writeRequestBytes = QlProtocolCommandBuilder.BuildWriteFloat(0x10000005, 0x164E, 0.0596f);
Console.WriteLine(QlHexConverter.ToHexString(writeRequestBytes));
// 10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4

byte[] writeResponseBytes =
{
    0x10, 0x00, 0x00, 0x05,
    0x06,
    0x16, 0x4E,
    0x01,
    0x60,
    0x2A, 0x82
};

QlProtocolFrame writeFrame = QlProtocolParser.Parse(writeResponseBytes);

Console.WriteLine(writeFrame.Kind); // WriteResponse
Console.WriteLine($"0x{writeFrame.ResponseCode.GetValueOrDefault():X2}"); // 0x60
```

Additional note:

- real communication code usually receives raw `byte[]`
- therefore `Parse(byte[])` should be the primary API
- `ParseHex(...)` is mainly a debugging, testing, and documentation helper

### `QlPayloadCodec`

Purpose: low-level field and payload-value encoding/decoding.

Common methods:

- `EncodeUInt16 / DecodeUInt16`
- `EncodeValueUInt16 / DecodeValueUInt16`
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

## Function-code coverage

### `0x03` read registers

Currently supports:

- request parsing
- response parsing
- high-level known-register command building and parsing

Typical scenario:

- request: `10 00 00 01 03 00 00 00 01 43 21`
- response: `10 00 00 01 03 00 00 04 1C 04 1F 41 97 E9`
- parsed value: `9.9385`

### `0x06` write registers

Currently supports:

- request building
- request parsing
- simple response parsing

Typical scenario:

- request: `10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4`
- response: `10 00 00 05 06 16 4E 01 60 2A 82`
- response code: `0x60`

### `0x08`

Currently supports:

- generic structural parsing
- response-code extraction

### `0x23 / 0x26 / 0x30 / 0x32 / 0x33`

Currently supports:

- generic “length field + payload” parsing according to the document structure

Notes:

- those function codes have richer business payload definitions
- the library currently focuses on structural parsing first
- higher-level business interpretation can be extended on top as needed

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

For registers not included yet:

```csharp
var register = QlKnownRegisters.GetOrCreateRaw(500, 1, "CustomRegister");
```

## Selection advice

For business callers:

- prefer `QlKnownOperations`
- or `QlProtocolKnownCommands + QlProtocolKnownParsers`

For protocol extension work:

- prefer `QlProtocolCommandBuilder`
- then combine it with `QlProtocolParser + QlProtocolFrameExtensions`

For optional wrapped streams:

- use `QlProtocolStreamDecoder`

For simple “send one packet, receive one packet” flows:

- `Build...` + `Parse...` is usually enough

## `QlProtocolStreamDecoder`

Purpose: decode only the optional wrapped stream format.

Important:

- it handles `C6 F4 C2 CC + Length + BarePacket + 0D 0A`
- bare packets themselves do not carry their own stream boundary marker
- for bare packets, the transport layer must already preserve one-packet boundaries or you must manage framing externally
