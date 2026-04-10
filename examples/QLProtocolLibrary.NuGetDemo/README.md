# QLProtocolLibrary.NuGetDemo

这个示例演示“外部项目如何通过 NuGet 包使用 `QLProtocolLibrary`”，不依赖当前仓库源码项目引用。

## 运行

```bash
dotnet restore
dotnet run
```

## 这个示例包含

- 使用 `PackageReference` 引用 `QLProtocolLibrary 0.3.0`
- 组包：`QlProtocolKnownCommands` / `QlKnownOperations`
- 解析：`QlProtocolParser` / `QlProtocolKnownParsers`
- 已知响应路由：`QlProtocolKnownRouter`
- TCP 粘包拆包：`QlProtocolStreamDecoder`
