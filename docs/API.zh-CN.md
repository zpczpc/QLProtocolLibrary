# API 参考

## 总体说明

这个库实现的是主应用层协议报文模型。

默认裸报文格式：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

可选包络格式：

```text
C6 F4 C2 CC + 长度(2) + 裸报文 + 0D 0A
```

如果你的使用场景只是“发一帧、收一帧”，优先直接使用裸报文 API。

## 使用层级

推荐优先级从高到低：

1. `QlKnownOperations`
2. `QlProtocolKnownRouter`
3. `QlProtocolKnownCommands` / `QlProtocolKnownParsers`
4. `QlProtocolCommandBuilder` / `QlProtocolParser` / `QlProtocolFrameExtensions`
5. `QlPayloadCodec`

## 字节序说明

这份协议不能简单理解成“所有字段都同一种字节序”。

建议这样理解：

- 协议控制字段
  例如寄存器地址、寄存器数量、长度字段
  使用高字节在前
- 数据值字段
  例如 `WORD`、`FLOAT`、日志内容、操作参数
  必须按具体功能码章节和数据类型定义处理

当前库中的拆分为：

- `EncodeUInt16 / DecodeUInt16`
  用于协议控制字段
- `EncodeValueUInt16 / DecodeValueUInt16`
  用于 16 位数据值
- `EncodeUInt32 / DecodeUInt32`
  用于 32 位数据值
- `EncodeSingle / DecodeSingle`
  用于浮点值

## 高层入口

### `QlKnownOperations`

作用：把“寄存器定义 + 读命令构造 + 响应解析”封装成统一对象。

常用成员：

- `QlKnownOperations.DeviceTime`
- `QlKnownOperations.RunStatus`
- `QlKnownOperations.DeviceNo`
- `QlKnownOperations.MeasureResult`
- `QlKnownOperations.VersionBundle`

常用方法：

- `BuildRead(uint deviceAddress)`
- `BuildReadHex(uint deviceAddress)`
- `TryParse(QlProtocolFrame frame, out T value)`
- `Parse(QlProtocolFrame frame)`

### `QlProtocolKnownRouter`

作用：把一帧已知业务响应自动路由到对应结果对象。

常用方法：

- `TryParse(QlProtocolFrame frame, out QlKnownParseResult? result)`

### `QlProtocolKnownCommands`

作用：按业务名构建命令，不暴露寄存器地址。

常用方法：

- `BuildReadDeviceTime(uint deviceAddress)`
- `BuildReadRunStatus(uint deviceAddress)`
- `BuildReadDeviceNo(uint deviceAddress)`
- `BuildReadMeasureResult(uint deviceAddress)`
- `BuildReadVersionBundle(uint deviceAddress)`
- `BuildSetDeviceTime(uint deviceAddress, DateTime value)`
- `BuildWriteDeviceNo(uint deviceAddress, string deviceNo, int fixedByteLength = 16)`
- `BuildWriteAnalyzerCode(uint deviceAddress, string analyzerCode, int fixedByteLength = 16)`

### `QlProtocolKnownParsers`

作用：按业务名解析已知寄存器响应。

常用方法：

- `TryParseDeviceTime`
- `TryParseRunStatus`
- `TryParseDeviceNo`
- `TryParseAnalyzerCode`
- `TryParseConcentration`
- `TryParseMeasureResult`
- `TryParseKbInfo`
- `TryParseMeterStrongLight`
- `TryParseVersionBundle`

## 通用入口

### `QlProtocolCommandBuilder`

作用：直接按“设备地址 + 功能码 + 功能数据”组包。

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
- `BuildForward(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)`
- `BuildForwardHex(uint deviceAddress, byte portId, byte[] forwardedContent, bool includeEnvelope = false)`

示例：

```csharp
uint deviceAddress = 0x10000001;
byte[] cmd = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 01 03 00 00 00 01 43 21
```

`0x32` 指令转发示例：

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

作用：把完整协议报文解析成 `QlProtocolFrame`。

常用方法：

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

说明：

- 支持裸报文
- 支持可选包络报文

### `QlProtocolFrame`

作用：承载解析结果。

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
- `ErrorCode`
- `Crc`
- `ComputedCrc`
- `IsCrcValid`

### `QlProtocolFrameExtensions`

作用：从已解析的 payload 中按类型读取数据。

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
- `Decode(QlRegisterDefinition register)`
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`

`0x32` 解析示例：

```csharp
QlProtocolFrame frame = QlProtocolParser.ParseHex(
    "10 00 00 0F 32 00 11 01 10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9 CE D2");

byte portId = frame.ReadForwardPortId();
byte[] content = frame.ReadForwardContent();

Console.WriteLine(portId); // 1
Console.WriteLine(QlHexConverter.ToHexString(content));
```

### `QlPayloadCodec`

作用：低层字段与数据值编解码工具。

常用方法：

- `EncodeUInt16 / DecodeUInt16`
- `EncodeValueUInt16 / DecodeValueUInt16`
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

## 功能码支持情况

### `0x03`

当前支持：

- 请求组包
- 请求解析
- 响应解析
- 高层已知寄存器读操作

### `0x06`

当前支持：

- 请求组包
- 请求解析
- 简单响应解析

### `0x32`

当前支持：

- 专用构包 helper：`BuildForward(...)`
- 专用 Hex helper：`BuildForwardHex(...)`
- 通用结构解析
- 专用解析 helper：`ReadForwardPortId()` / `ReadForwardContent()`

结构说明：

```text
设备地址(4) + 0x32(1) + 数据长度(2) + 端口ID(1) + 转发内容(N) + CRC16(2)
```

注意：

- 数据长度 = `端口ID + 转发内容` 的总字节数
- 转发内容通常就是另一条完整的正常协议命令

### `0x08 / 0x23 / 0x26 / 0x30 / 0x33`

当前支持：

- 通用结构解析

说明：

- 这些功能码的业务字段定义更复杂
- 当前优先提供结构化基础能力
- 更细的业务解释建议按项目需要继续扩展

## 常见结果模型

- `QlRunStatusInfo`
- `QlMeasureResultInfo`
- `QlKbInfo`
- `QlMeterStrongLightInfo`
- `QlVersionBundleInfo`
- `QlDecodedRegisterValue`
- `QlKnownParseResult`

## 内置寄存器目录

当前内置了常用寄存器定义，位于 `QlKnownRegisters`：

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

## 选择建议

如果你是业务调用方：

- 优先使用 `QlKnownOperations`
- 或者 `QlProtocolKnownCommands + QlProtocolKnownParsers`

如果你是协议扩展方：

- 优先使用 `QlProtocolCommandBuilder`
- 再配合 `QlProtocolParser + QlProtocolFrameExtensions`

如果你只是在做“发一帧、收一帧”：

- 直接 `Build...` + `Parse...` 即可

## `QlProtocolStreamDecoder`

作用：只针对可选包络流做拆包。

说明：

- 处理的是 `C6 F4 C2 CC + 长度 + 裸报文 + 0D 0A`
- 裸报文本身没有额外边界字段
- 所以裸报文场景通常要由你的传输层保证“一次收一帧”，或者你自己维护边界
