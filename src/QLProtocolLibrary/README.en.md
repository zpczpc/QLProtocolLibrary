# QLProtocolLibrary

[中文](README.md) | English

`QLProtocolLibrary` is a NuGet package for the QL device application-layer protocol.

The default bare packet format is:

```text
DeviceAddress(4) + FunctionCode(1) + FunctionData(N) + CRC16(2)
```

It also supports the optional document envelope:

```text
C6 F4 C2 CC + Length(2) + BarePacket + 0D 0A
```

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

## `0x32` command forwarding

Starting from `0.5.0`, the library provides dedicated helpers for `0x32` forwarding frames.

Packet shape:

```text
DeviceAddress(4) + 0x32(1) + DataLength(2) + PortId(1) + ForwardedContent(N) + CRC16(2)
```

Example:

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

## Common APIs

### Packet building

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWrite(...)`
- `QlProtocolCommandBuilder.BuildWriteRegisters(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolCommandBuilder.BuildForward(...)`
- `QlProtocolCommandBuilder.BuildPacket(...)`

### Packet parsing

- `QlProtocolParser.Parse(...)`
- `QlProtocolParser.ParseHex(...)`
- `QlProtocolParser.TryParse(...)`

### Payload readers

- `frame.ReadUInt16()`
- `frame.ReadUInt32()`
- `frame.ReadSingle()`
- `frame.ReadUtf8()`
- `frame.ReadBcdDateTime()`
- `frame.ReadForwardPortId()`
- `frame.ReadForwardContent()`

## Important notes

- device address is always 4 bytes
- CRC16 is transmitted low byte first
- one register equals 4 bytes in this protocol
- protocol control fields and payload value fields do not share one universal byte order
- for `0x32`, `DataLength = PortId + ForwardedContent`

## Related docs

- repository README: `README.md`
- Chinese API: `docs/API.zh-CN.md`
- English API: `docs/API.en.md`
- demo: `examples/QLProtocolLibrary.Demo`
