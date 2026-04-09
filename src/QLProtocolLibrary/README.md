# QLProtocolLibrary

中文 | [English](README.en.md)

`QLProtocolLibrary` 的目标是把 QL 设备协议细节藏起来，让调用方直接做两件事：

1. 打包协议
2. 解析协议

也就是说，既支持“完全不用关心地址”的高层调用，也支持“按数据类型读取”的通用调用。

## 这个库解决什么问题

- 不需要手写 `AA 55 ... BB 55`
- 不需要手工计算 CRC16
- 不需要自己处理 TCP 粘包拆包
- 不需要记常用寄存器地址和寄存器长度
- 不需要每次从 `byte[]` 里手动切片和转类型

## 推荐理解方式

这个库分两层：

### 第一层：高层业务 API

给不想关心地址和 payload 结构的人使用。

例如：

- `QlProtocolKnownCommands.BuildReadDeviceTime("1001")`
- `QlProtocolKnownCommands.BuildReadRunStatus("1001")`
- `QlProtocolKnownParsers.TryParseDeviceTime(frame, out var time)`
- `QlProtocolKnownParsers.TryParseRunStatus(frame, out var status)`

### 第二层：通用协议 API

给要扩展寄存器、调试协议或做更底层封装的人使用。

例如：

- `QlProtocolCommandBuilder.BuildRead(...)`
- `QlProtocolParser.Parse(...)`
- `frame.ReadUInt16()`
- `frame.ReadSingle()`
- `frame.Decode(QlKnownRegisters.DeviceNo)`

## 报文格式

每帧以以下格式封装：

- 帧头：`AA 55`
- 帧尾：`BB 55`
- 中间主体：类 Modbus 布局

主体常见结构：

- `MN`：8 字节
- `FunctionCode`：1 字节
- `Address`：2 字节
- `Payload / RegisterCount / ByteCount`：按功能码决定
- `CRC16`：2 字节，低字节在前

## 快速开始

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

## 公开 API

### 1. `QlProtocolKnownCommands`

这是高层命令入口，调用方不需要关心地址。

常用方法：

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

返回值：

- 上面这些方法都返回完整协议帧 `byte[]`
- 对应的 `Hex` 版本返回十六进制字符串

示例：

```csharp
var cmd1 = QlProtocolKnownCommands.BuildReadDeviceTime("1001");
var cmd2 = QlProtocolKnownCommands.BuildReadRunStatus("1001");
var cmd3 = QlProtocolKnownCommands.BuildSetDeviceTime("1001", DateTime.Now);
```

### 2. `QlProtocolKnownParsers`

这是高层结果解析入口，调用方不需要自己拆 payload。

常用方法：

- `TryParseDeviceTime(QlProtocolFrame frame, out DateTime value)`
- `TryParseDeviceNo(QlProtocolFrame frame, out string? value)`
- `TryParseAnalyzerCode(QlProtocolFrame frame, out string? value)`
- `TryParseConcentration(QlProtocolFrame frame, out float value)`
- `TryParseRunStatus(QlProtocolFrame frame, out QlRunStatusInfo? value)`
- `TryParseMeasureResult(QlProtocolFrame frame, out QlMeasureResultInfo? value)`
- `TryParseKbInfo(QlProtocolFrame frame, out QlKbInfo? value)`
- `TryParseMeterStrongLight(QlProtocolFrame frame, out QlMeterStrongLightInfo? value)`
- `TryParseVersionBundle(QlProtocolFrame frame, out QlVersionBundleInfo? value)`

返回值：

- 成功时返回 `true`
- 对应 `out` 参数里给出强类型结果

示例：

```csharp
if (QlProtocolKnownParsers.TryParseDeviceTime(frame, out var time))
{
    Console.WriteLine(time);
}

if (QlProtocolKnownParsers.TryParseRunStatus(frame, out var status) && status != null)
{
    Console.WriteLine(status.Status);
    Console.WriteLine(status.WarnCode);
}
```

### 3. `QlProtocolCommandBuilder`

这是通用命令构造入口，适合需要自己指定地址的人。

常用方法：

- `BuildRead(string mn, ushort address, ushort registerCount)`
- `BuildRead(string mn, QlRegisterDefinition register)`
- `BuildWrite(string mn, ushort address, ushort registerCount, byte[] payload)`
- `BuildWriteFloat(string mn, ushort address, params float[] values)`
- `BuildWriteUtf8(string mn, ushort address, string value, int fixedByteLength = 0)`
- `BuildSetTime(string mn, DateTime value)`
- `BuildReadHex(...)` / `BuildWriteHex(...)` / `BuildSetTimeHex(...)`

返回值：

- 二进制版本返回完整协议帧 `byte[]`
- `Hex` 版本返回十六进制字符串

### 4. `QlProtocolParser`

把完整帧解析成结构化对象。

常用方法：

- `Parse(byte[] frameBytes)`
- `ParseHex(string hex)`
- `TryParse(byte[] frameBytes, out QlProtocolFrame? frame)`
- `TryParseHex(string hex, out QlProtocolFrame? frame)`

返回值：

- `Parse` / `ParseHex` 返回 `QlProtocolFrame`
- `TryParse` / `TryParseHex` 返回 `bool`

### 5. `QlProtocolFrame`

承载解析结果。

常用属性：

- `RawBytes`
- `Mn` / `MnText`
- `RawFunctionCode`
- `FunctionCode`
- `Kind`
- `Address`
- `RegisterCount`
- `Payload`
- `ByteCount`
- `ErrorCode`
- `Crc`
- `ComputedCrc`
- `IsCrcValid`

### 6. `QlProtocolFrameExtensions`

这是按数据类型读取的通用入口。

常用方法：

- `ReadUInt16()`：返回 `ushort`
- `ReadUInt32()`：返回 `uint`
- `ReadSingle()`：返回 `float`
- `ReadSingles()`：返回 `IReadOnlyList<float>`
- `ReadUtf8()`：返回 `string`
- `ReadAscii()`：返回 `string`
- `ReadBcdDateTime()`：返回 `DateTime`
- `ReadBcdDateTimeText()`：返回字符串
- `ReadUInt16Array()`：返回 `IReadOnlyList<ushort>`
- `Decode(QlRegisterDefinition register)`：返回 `QlDecodedRegisterValue`
- `TryDecodeKnownRegister(out QlDecodedRegisterValue? decoded)`：按内置寄存器定义尝试解码

示例：

```csharp
var frame = QlProtocolParser.Parse(bytes);
var time = frame.ReadBcdDateTime();
var value = frame.ReadSingle();
var text = frame.ReadUtf8();
var typed = frame.Decode(QlKnownRegisters.DeviceNo);
```

### 7. `QlPayloadCodec`

低层编解码工具，一般给扩展协议或特殊寄存器用。

常用方法：

- `EncodeUInt16 / DecodeUInt16`
- `EncodeUInt32 / DecodeUInt32`
- `EncodeSingle / DecodeSingle / DecodeSingles`
- `EncodeUtf8 / DecodeUtf8`
- `EncodeBcdDateTime / DecodeBcdDateTime / DecodeBcdDateTimeText`

## 强类型结果模型

高层解析器会返回这些模型：

- `QlRunStatusInfo`
- `QlMeasureResultInfo`
- `QlKbInfo`
- `QlMeterStrongLightInfo`
- `QlVersionBundleInfo`
- 以及基础类型：`DateTime`、`float`、`string`

## 内置寄存器目录

当前内置了常用寄存器定义，放在 `QlKnownRegisters`：

- `DeviceNo`：76，设备编号
- `MeasureResult`：94，测量结果复合报文
- `RunStatus`：200，运行状态复合报文
- `SubStatus`：201，子状态
- `RunMode`：202，运行模式
- `MeasureMode`：203，测量模式
- `WarnCode`：204，告警信息
- `FaultCode`：205，故障信息
- `DeviceTime`：208，设备时间
- `Concentration`：238，单浮点参数
- `WorkStateFlag`：248，状态位
- `KbInfo`：312，K/B/F 参数
- `MeterStrongLight`：460，吸收光强
- `AnalyzerCode`：464，仪表编号
- `VersionBundle`：709，版本信息复合报文

如果遇到未收录地址，可以：

```csharp
var register = QlKnownRegisters.GetOrCreateRaw(500, 2, "CustomRegister");
```

## 两种使用方式都支持

### 方式一：完全不关心地址

适合业务同事、上位机页面逻辑、流程控制代码：

```csharp
var cmd = QlProtocolKnownCommands.BuildReadDeviceTime("1001");
var frame = QlProtocolParser.Parse(bytes);
if (QlProtocolKnownParsers.TryParseDeviceTime(frame, out var time))
{
    Console.WriteLine(time);
}
```

### 方式二：按数据类型通用解析

适合扩展寄存器、调试协议、做底层封装：

```csharp
var frame = QlProtocolParser.Parse(bytes);
var time = frame.ReadBcdDateTime();
var value = frame.ReadSingle();
var text = frame.ReadUtf8();
var typed = frame.Decode(QlKnownRegisters.DeviceNo);
```

## 推荐使用方式

对于业务项目，建议优先走：

1. `QlProtocolKnownCommands` 负责组包
2. `QlProtocolStreamDecoder` 负责拆 TCP 流
3. `QlProtocolParser` 负责转成结构化帧
4. `QlProtocolKnownParsers` 负责得到业务结果

如果遇到新寄存器或协议扩展，再下沉到：

- `QlProtocolCommandBuilder`
- `QlProtocolFrameExtensions`
- `QlPayloadCodec`

## 示例工程

可运行示例在：

- `examples/QLProtocolLibrary.Demo`

仓库地址：

- `https://github.com/zpczpc/QLProtocolLibrary`


## 更进一步的 SDK 用法

现在除了高层命令方法和高层解析方法之外，还提供了两层统一入口：

### `QlKnownOperations`

这是“已知业务操作目录”，每个操作同时包含：

- 寄存器定义
- 读命令构造
- 响应解析

例如：

```csharp
var cmd = QlKnownOperations.DeviceTime.BuildRead("1001");
var hex = QlKnownOperations.RunStatus.BuildReadHex("1001");

if (QlKnownOperations.DeviceTime.TryParse(frame, out DateTime time))
{
    Console.WriteLine(time);
}
```

### `QlProtocolKnownRouter`

这是“统一已知响应路由器”，适合收到一帧后自动判断它属于哪种业务结果。

```csharp
if (QlProtocolKnownRouter.TryParse(frame, out var result) && result != null)
{
    Console.WriteLine(result.Name);
    Console.WriteLine(result.Value);
}
```

如果你想把这套库继续往 SDK 化推进，推荐优先使用顺序是：

1. `QlKnownOperations`
2. `QlProtocolKnownRouter`
3. `QlProtocolKnownCommands` / `QlProtocolKnownParsers`
4. `QlProtocolCommandBuilder` / `QlProtocolFrameExtensions`
