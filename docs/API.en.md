# API Reference

## Usage layers

Recommended order from highest level to lowest level:

1. `QlKnownOperations`
2. `QlProtocolKnownRouter`
3. `QlProtocolKnownCommands` / `QlProtocolKnownParsers`
4. `QlProtocolCommandBuilder` / `QlProtocolParser` / `QlProtocolFrameExtensions`
5. `QlPayloadCodec`

## High-level entry points

### `QlKnownOperations`

Purpose: package register metadata, command building, and response parsing into one operation object.

Typical members:

- `QlKnownOperations.DeviceTime`
- `QlKnownOperations.RunStatus`
- `QlKnownOperations.DeviceNo`
- `QlKnownOperations.MeasureResult`
- `QlKnownOperations.VersionBundle`

Common methods:

- `BuildRead(string mn)`
- `BuildReadHex(string mn)`
- `TryParse(QlProtocolFrame frame, out T value)`
- `Parse(QlProtocolFrame frame)`

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

Purpose: build frames by business intent without exposing register addresses.

Typical methods:

- `BuildReadDeviceTime`
- `BuildReadRunStatus`
- `BuildReadDeviceNo`
- `BuildReadMeasureResult`
- `BuildReadVersionBundle`
- `BuildSetDeviceTime`
- `BuildWriteDeviceNo`
- `BuildWriteAnalyzerCode`

### `QlProtocolKnownParsers`

Purpose: parse business responses by intent.

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

Purpose: build frames directly by address and register count.

### `QlProtocolParser`

Purpose: parse a full frame into `QlProtocolFrame`.

### `QlProtocolFrameExtensions`

Purpose: read strongly typed values from a parsed frame payload.

Common methods:

- `ReadUInt16`
- `ReadUInt32`
- `ReadSingle`
- `ReadSingles`
- `ReadUtf8`
- `ReadAscii`
- `ReadBcdDateTime`
- `ReadBcdDateTimeText`
- `ReadUInt16Array`
- `Decode`
- `TryDecodeKnownRegister`

### `QlPayloadCodec`

Purpose: low-level encoding and decoding helpers.

## Common result models

- `QlRunStatusInfo`
- `QlMeasureResultInfo`
- `QlKbInfo`
- `QlMeterStrongLightInfo`
- `QlVersionBundleInfo`
- `QlDecodedRegisterValue`
- `QlKnownParseResult`

## Selection advice

For business callers:

- prefer `QlKnownOperations`
- or `QlProtocolKnownCommands + QlProtocolKnownParsers`

For protocol extension work:

- prefer `QlProtocolCommandBuilder`
- with `QlProtocolParser + QlProtocolFrameExtensions`

For unified incoming-frame routing:

- prefer `QlProtocolKnownRouter`
