# API 参考

## 总体说明

这个库实现的是主应用层协议报文模型。

默认处理的主裸报文格式：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

可选包络格式：

```text
C6 F4 C2 CC + 长度(2) + 裸报文 + 0D 0A
```

如果你的使用场景是“发送一个报文，接收一个报文”，优先直接使用裸报文 API 即可。

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
  例如：寄存器地址、寄存器数量、长度字段
  按文档约定使用高字节在前
- 数据值字段
  例如：WORD、FLOAT、日志内容、操作参数
  必须按具体功能码章节和数据类型约定处理

当前库中：

- `EncodeUInt16 / DecodeUInt16`
  用于协议字段
- `EncodeValueUInt16 / DecodeValueUInt16`
  用于 16 位数据值
- `EncodeUInt32 / DecodeUInt32`
  用于 32 位数据值
- `EncodeSingle / DecodeSingle`
  用于浮点数据值

## 高层入口

### `QlKnownOperations`

作用：把“寄存器定义 + 读命令构造 + 响应解析”封装成统一对象。

典型成员：

- `QlKnownOperations.DeviceTime`
- `QlKnownOperations.RunStatus`
- `QlKnownOperations.DeviceNo`
- `QlKnownOperations.MeasureResult`
- `QlKnownOperations.VersionBundle`

常用方法：

- `BuildRead(uint deviceAddress)`
  返回：完整协议报文 `byte[]`

- `BuildReadHex(uint deviceAddress)`
  返回：十六进制字符串

- `TryParse(QlProtocolFrame frame, out T value)`
  返回：`bool`

- `Parse(QlProtocolFrame frame)`
  返回：强类型结果；解析失败时抛异常

示例：

```csharp
uint deviceAddress = 0x10000001;

byte[] cmd = QlKnownOperations.DeviceTime.BuildRead(deviceAddress);

if (QlKnownOperations.DeviceTime.TryParse(frame, out DateTime time))
{
    Console.WriteLine(time);
}
```

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

作用：按业务名称构造命令，不需要自己填写寄存器地址。

典型方法：

- `BuildReadDeviceTime(uint deviceAddress)`
- `BuildReadRunStatus(uint deviceAddress)`
- `BuildReadDeviceNo(uint deviceAddress)`
- `BuildReadMeasureResult(uint deviceAddress)`
- `BuildReadVersionBundle(uint deviceAddress)`
- `BuildSetDeviceTime(uint deviceAddress, DateTime value)`
- `BuildWriteDeviceNo(uint deviceAddress, string deviceNo, int fixedByteLength = 16)`
- `BuildWriteAnalyzerCode(uint deviceAddress, string analyzerCode, int fixedByteLength = 16)`

### `QlProtocolKnownParsers`

作用：按业务名称解析已知寄存器响应。

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
- `BuildReadHex(...)`
- `BuildWriteHex(...)`
- `BuildSetTimeHex(...)`

文档示例：

```csharp
uint deviceAddress = 0x10000001;
byte[] cmd = QlProtocolCommandBuilder.BuildRead(deviceAddress, 0x0000, 0x0001);
Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 01 03 00 00 00 01 43 21
```

写 float 示例：

```csharp
uint deviceAddress = 0x10000005;
byte[] cmd = QlProtocolCommandBuilder.BuildWriteFloat(deviceAddress, 0x164E, 0.0596f);
Console.WriteLine(QlHexConverter.ToHexString(cmd));
// 10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4
```

### `QlProtocolParser`

作用：把完整协议报文解析成 `QlProtocolFrame`。

常用方法：

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

说明：

- 既支持裸报文
- 也支持带可选包络的报文

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

作用：从已解析的 payload 里按数据类型读取值。

常用方法：

- `ReadUInt16()`：读取 16 位数据值
- `ReadUInt32()`：读取 32 位数据值
- `ReadSingle()`：读取 `float`
- `ReadSingles()`：读取 `float` 数组
- `ReadUtf8()`：读取 UTF-8 字符串
- `ReadAscii()`：读取 ASCII 字符串
- `ReadBcdDateTime()`：读取 BCD 时间
- `ReadBcdDateTimeText()`：读取 BCD 时间文本
- `ReadUInt16Array()`：读取 `ushort` 数组
- `Decode(QlRegisterDefinition register)`：按寄存器定义解码
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`：按内置寄存器目录解码

示例：

```csharp
// 读取 float 示例
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

写 float 应答示例：

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

补充：

- 真实通讯时，收到的通常就是原始 `byte[]`
- 所以业务代码优先应使用 `Parse(byte[])`
- `ParseHex(...)` 更适合调试、单元测试、文档验证

### `QlPayloadCodec`

作用：低层字段与数据值编解码。

常用方法：

- `EncodeUInt16 / DecodeUInt16`
- `EncodeValueUInt16 / DecodeValueUInt16`
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

## 功能码结构支持情况

### `0x03` 读寄存器

当前支持：

- 请求解析
- 响应解析
- 高层已知寄存器组包与解析

典型场景：

- 发送：`10 00 00 01 03 00 00 00 01 43 21`
- 应答：`10 00 00 01 03 00 00 04 1C 04 1F 41 97 E9`
- 解析结果：`9.9385`

### `0x06` 写寄存器

当前支持：

- 请求组包
- 请求解析
- 简单响应解析

典型场景：

- 发送：`10 00 00 05 06 16 4E 00 01 04 21 1F 74 3D 05 E4`
- 应答：`10 00 00 05 06 16 4E 01 60 2A 82`
- 应答码：`0x60`

### `0x08`

当前支持：

- 通用结构解析
- 响应码读取

### `0x23 / 0x26 / 0x30 / 0x32 / 0x33`

当前支持：

- 按文档的“长度字段 + payload”结构做通用解析

说明：

- 这些功能码的业务字段定义更复杂
- 当前库优先提供结构化基础解析
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

当前内置了常用寄存器定义，放在 `QlKnownRegisters`：

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

如果遇到未收录地址，可以：

```csharp
var register = QlKnownRegisters.GetOrCreateRaw(500, 1, "CustomRegister");
```

## 典型选择建议

如果你是业务调用方：

- 优先用 `QlKnownOperations`
- 或者 `QlProtocolKnownCommands + QlProtocolKnownParsers`

如果你是协议扩展方：

- 优先用 `QlProtocolCommandBuilder`
- 再配合 `QlProtocolParser + QlProtocolFrameExtensions`

如果你需要处理可选包络流：

- 使用 `QlProtocolStreamDecoder`

如果你只是“发一帧，收一帧”：

- 直接 `Build...` + `Parse...` 即可

## `QlProtocolStreamDecoder`

作用：只针对可选包络流做拆包。

注意：

- 它处理的是 `C6 F4 C2 CC + 长度 + 裸报文 + 0D 0A`
- 对于裸报文本身，没有额外边界字段，不能凭空做流切分
- 所以裸报文场景下，通常应由你的通讯层保证“一次收一帧”或自行管理边界
