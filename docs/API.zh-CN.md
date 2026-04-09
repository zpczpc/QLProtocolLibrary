# API 参考

## 使用层级

推荐优先级从高到低：

1. `QlKnownOperations`
2. `QlProtocolKnownRouter`
3. `QlProtocolKnownCommands` / `QlProtocolKnownParsers`
4. `QlProtocolCommandBuilder` / `QlProtocolParser` / `QlProtocolFrameExtensions`
5. `QlPayloadCodec`

## 高层入口

### `QlKnownOperations`

作用：把“寄存器定义 + 命令构造 + 响应解析”封装成统一对象。

典型成员：

- `QlKnownOperations.DeviceTime`
- `QlKnownOperations.RunStatus`
- `QlKnownOperations.DeviceNo`
- `QlKnownOperations.MeasureResult`
- `QlKnownOperations.VersionBundle`

常用方法：

- `BuildRead(string mn)`
  返回：完整协议帧 `byte[]`

- `BuildReadHex(string mn)`
  返回：十六进制字符串

- `TryParse(QlProtocolFrame frame, out T value)`
  返回：`bool`

- `Parse(QlProtocolFrame frame)`
  返回：强类型结果；解析失败时抛异常

### `QlProtocolKnownRouter`

作用：收到任意一帧后，自动路由到已知业务结果。

常用方法：

- `TryParse(QlProtocolFrame frame, out QlKnownParseResult? result)`

返回对象：

- `Name`：业务名，如 `DeviceTime`
- `Register`：对应寄存器定义
- `Value`：解析结果对象
- `GetValue<T>()`：强制类型读取

### `QlProtocolKnownCommands`

作用：按业务名称构造命令，不关心寄存器地址。

典型方法：

- `BuildReadDeviceTime`
- `BuildReadRunStatus`
- `BuildReadDeviceNo`
- `BuildReadMeasureResult`
- `BuildReadVersionBundle`
- `BuildSetDeviceTime`
- `BuildWriteDeviceNo`
- `BuildWriteAnalyzerCode`

### `QlProtocolKnownParsers`

作用：按业务名称解析响应。

典型方法：

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

作用：按地址和寄存器数量直接构造协议帧。

常用方法：

- `BuildRead`
- `BuildWrite`
- `BuildWriteFloat`
- `BuildWriteUtf8`
- `BuildSetTime`
- `BuildReadHex`
- `BuildWriteHex`
- `BuildSetTimeHex`

### `QlProtocolParser`

作用：把完整帧解析成 `QlProtocolFrame`。

常用方法：

- `Parse`
- `ParseHex`
- `TryParse`
- `TryParseHex`

### `QlProtocolFrameExtensions`

作用：从已解析的帧里按数据类型取值。

常用方法：

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

作用：低层编码 / 解码工具。

常用方法：

- `EncodeUInt16 / DecodeUInt16`
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

## 常见结果模型

- `QlRunStatusInfo`
- `QlMeasureResultInfo`
- `QlKbInfo`
- `QlMeterStrongLightInfo`
- `QlVersionBundleInfo`
- `QlDecodedRegisterValue`
- `QlKnownParseResult`

## 典型选择建议

如果你是业务调用方：

- 优先用 `QlKnownOperations`
- 或者 `QlProtocolKnownCommands + QlProtocolKnownParsers`

如果你是协议扩展方：

- 优先用 `QlProtocolCommandBuilder`
- 再配合 `QlProtocolParser + QlProtocolFrameExtensions`

如果你是做统一接收分发：

- 优先用 `QlProtocolKnownRouter`
