# QLProtocolLibrary

[中文](README.md) | English

`QLProtocolLibrary` is a C# package for the main application-layer packet format used by this project.

By default, this package works with the main bare packet format:

```text
DeviceAddress(4) + FunctionCode(1) + FunctionData(N) + CRC16(2)
```

It also supports the optional document envelope:

```text
C6 F4 C2 CC + Length(2) + BarePacket + 0D 0A
```

## Good fit for

- upper-computer applications that send and receive protocol packets directly
- device debugging tools
- reusable protocol wrappers
- external projects consuming the protocol via NuGet

## Installation

```bash
dotnet add package QLProtocolLibrary
```

## Smallest example

```csharp
using QLProtocolLibrary;

uint deviceAddress = 0x10000001;

byte[] command = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(command));
// 10 00 00 01 03 00 00 00 01 43 21
```

## Parse example

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

For real serial/485/TCP input, pass the received raw `byte[]` directly to `QlProtocolParser.Parse(...)`.
`ParseHex(...)` is mainly useful for debugging and tests.

## Key APIs

### Packet building

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolCommandBuilder.BuildWrite(...)`
- `QlProtocolCommandBuilder.BuildWriteRegisters(...)`
- `QlProtocolCommandBuilder.BuildWriteFloat(...)`
- `QlProtocolCommandBuilder.BuildPacket(...)`

### Packet parsing

- `QlProtocolParser.Parse(...)`
- `QlProtocolParser.ParseHex(...)`
- `QlProtocolParser.TryParse(...)`

### Typed payload access

- `frame.ReadUInt16()`
- `frame.ReadUInt32()`
- `frame.ReadSingle()`
- `frame.ReadUtf8()`
- `frame.ReadBcdDateTime()`

### High-level known-register APIs

- `QlProtocolKnownCommands`
- `QlProtocolKnownParsers`
- `QlKnownOperations`
- `QlProtocolKnownRouter`

## Important usage notes

- device address is always 4 bytes
- CRC16 is transmitted low byte first
- one register equals 4 bytes in this protocol
- protocol field byte order and payload value byte order are not the same concept
- payload layout depends on the function-code section and the data type defined in the protocol document

## Related docs

- repository README: `README.md`
- Chinese API: `docs/API.zh-CN.md`
- English API: `docs/API.en.md`
- demo: `examples/QLProtocolLibrary.Demo`
