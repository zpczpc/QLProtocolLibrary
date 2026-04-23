# QLProtocolLibrary.NuGetDemo

这个示例演示“外部项目如何通过 NuGet 包使用 `QLProtocolLibrary`”。

它关注的最小流程是：

1. 组一个请求报文 `byte[]`
2. 把这个 `byte[]` 发给设备
3. 收到设备返回的 `byte[]`
4. 用 `QlProtocolParser.Parse(...)` 解包

## 当前状态

这个示例工程目前仍固定引用 `QLProtocolLibrary 0.4.0`，目的是在仓库发布 `0.5.0` 之前保持 CI 与解决方案可正常恢复和编译。

等 `0.5.0` 正式发布到 nuget.org 后，你可以把 `PackageReference` 升级到 `0.5.0`，然后把示例中的 `0x32` 本地 helper 替换成正式 API：

- `QlProtocolCommandBuilder.BuildForward(...)`
- `frame.ReadForwardPortId()`
- `frame.ReadForwardContent()`

## 运行

```bash
dotnet restore
dotnet run
```

## 这个示例包含

- `0x03` 读请求与读响应解析
- `0x06` 写请求与写响应解析
- `0x32` 指令转发报文结构演示
- 最小的“发一帧、收一帧、直接解析”的调用方式

## 升级到 `0.5.0` 后的 `0x32` 用法

```csharp
byte[] forwardedCommand = QlHexConverter.FromHexString(
    "10 00 00 01 06 00 16 00 01 04 01 00 00 00 2E F9");

byte[] requestBytes = QlProtocolCommandBuilder.BuildForward(
    0x1000000F,
    0x01,
    forwardedCommand);

QlProtocolFrame frame = QlProtocolParser.Parse(requestBytes);

byte portId = frame.ReadForwardPortId();
byte[] forwardedContent = frame.ReadForwardContent();
```
