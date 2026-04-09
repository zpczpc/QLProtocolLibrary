# QLProtocolLibrary

中文 | [English](README.en.md)

`QLProtocolLibrary` 是一个面向公司内部设备协议的 C# 类库，目标是把协议细节封装起来，让调用方尽量不需要先熟悉整份协议文档就能开始使用。

它适合：

- WinForms 上位机
- 设备 TCP 通讯服务
- 需要快速接入协议的桌面程序或工具程序
- 想把协议能力封装成 NuGet 直接复用的团队项目

## 核心价值

这个库解决两件事：

1. 协议组包
2. 协议解析

并且同时支持两种使用方式：

- 高层业务方式：不关心地址，直接用 `BuildReadDeviceTime` / `TryParseDeviceTime`
- 通用协议方式：按地址和数据类型操作，比如 `BuildRead` / `ReadSingle` / `ReadUtf8`

## 安装

```bash
dotnet add package QLProtocolLibrary
```

## 快速示例

```csharp
using QLProtocolLibrary;

var command = QlProtocolKnownCommands.BuildReadDeviceTime("1001");

// 下发 command 后收到响应帧
QlProtocolFrame frame = QlProtocolParser.Parse(responseBytes);

if (QlProtocolKnownParsers.TryParseDeviceTime(frame, out var deviceTime))
{
    Console.WriteLine(deviceTime);
}
```

## 文档导航

- NuGet 包说明：[src/QLProtocolLibrary/README.md](src/QLProtocolLibrary/README.md)
- API 参考（中文）：[docs/API.zh-CN.md](docs/API.zh-CN.md)
- API reference (English): [docs/API.en.md](docs/API.en.md)
- 发布清单（中文）：[docs/PUBLISHING.zh-CN.md](docs/PUBLISHING.zh-CN.md)
- Publishing checklist (English): [docs/PUBLISHING.en.md](docs/PUBLISHING.en.md)
- 贡献说明：[CONTRIBUTING.md](CONTRIBUTING.md)
- 版本变更：[CHANGELOG.md](CHANGELOG.md)
- 示例工程：[examples/QLProtocolLibrary.Demo/Program.cs](examples/QLProtocolLibrary.Demo/Program.cs)

## 当前能力

- 完整协议帧组包
- 完整协议帧解析
- CRC16 校验
- TCP 粘包拆包
- 高层已知命令 API
- 高层已知业务解析 API
- 通用类型读取 API
- 已知业务操作目录与统一响应路由

## 仓库结构

- `src/QLProtocolLibrary`：类库源码
- `examples/QLProtocolLibrary.Demo`：示例项目
- `docs`：开源文档和发布说明

## 开源发布前建议

在真正发布到公开 NuGet 前，建议至少确认这些信息：

- 包许可证类型
- 项目主页 URL
- 仓库 URL
- issue / discussion 入口
- 包图标
- README 最终文案
- 版本号策略

详细清单见：[docs/PUBLISHING.zh-CN.md](docs/PUBLISHING.zh-CN.md)
