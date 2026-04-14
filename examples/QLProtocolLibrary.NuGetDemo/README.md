# QLProtocolLibrary.NuGetDemo

这个示例演示“外部项目如何通过 NuGet 包使用 `QLProtocolLibrary`”，并且示例代码已经按协议文档的主报文结构改写：

```text
设备地址(4) + 功能码(1) + 功能数据(N) + CRC16(2)
```

它不依赖当前仓库源码项目引用，而是通过 `PackageReference` 使用 NuGet 包。

## 运行

这个示例默认假设 `QLProtocolLibrary 0.4.0` 已经发布到 nuget.org。

发布后可以直接运行：

```bash
dotnet restore
dotnet run
```

## 这个示例包含

- 使用 `PackageReference` 引用 `QLProtocolLibrary 0.4.0`
- 文档风格组包：`QlProtocolCommandBuilder`
- 文档风格解包：`QlProtocolParser`
- 高层已知寄存器 API：`QlProtocolKnownCommands` / `QlKnownOperations`
- 已知响应路由：`QlProtocolKnownRouter`
- 可选包络流拆分：`QlProtocolStreamDecoder`

## 示例关注点

- 直接构造 `0x03` 读寄存器报文
- 直接解析文档里的 `0x03` 读响应示例
- 展示用户在真实项目里“发送一个 `byte[]`，接收一个 `byte[]`，然后解析”的最小流程

如果你的实际使用场景是“发一帧、收一帧”，通常只需要：

1. `QlProtocolCommandBuilder.Build...`
2. 把 `byte[]` 发给设备
3. 收到 `byte[]` 后调用 `QlProtocolParser.Parse(...)`
