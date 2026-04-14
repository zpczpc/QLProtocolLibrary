# 发布清单

这个文件用于真正发布到公开 NuGet 之前的最后检查。

## 1. 包元数据

建议确认这些字段：

- `PackageId`
- `Version`
- `Authors`
- `Company`
- `Description`
- `PackageTags`
- `PackageReadmeFile`
- `Version` 是否已为本次发布递增

建议补充但当前代码里未强制写死的字段：

- `PackageProjectUrl`
- `RepositoryUrl`
- `RepositoryBranch`
- `PackageLicenseExpression` 或 `PackageLicenseFile`
- `PackageIcon`
- `PackageReleaseNotes`

当前公开发布建议值：

- `PackageProjectUrl`：`https://github.com/zpczpc/QLProtocolLibrary`
- `RepositoryUrl`：`https://github.com/zpczpc/QLProtocolLibrary`
- `PackageLicenseExpression`：`MIT`

## 2. 开源信息

发布到公开仓库前，建议明确：

- 许可证类型
- 是否接受外部贡献
- 是否公开 issue
- 是否公开 roadmap
- 是否有商业或内部限制说明

## 3. 文档检查

至少确认：

- 根目录 `README.md`
- 包 README：`src/QLProtocolLibrary/README.md`
- 英文说明：`README.en.md` 和 `src/QLProtocolLibrary/README.en.md`
- `docs/API.zh-CN.md`
- `docs/API.en.md`
- `CHANGELOG.md`
- `CONTRIBUTING.md`

检查重点：

- 文档是否明确主协议结构为“设备地址 + 功能码 + 功能数据 + CRC”
- 是否还残留 `MN`、`AA 55 ... BB 55` 这类旧实现描述
- 是否明确说明可选包络与主裸报文的区别
- 是否明确说明“不同功能码、不同数据类型的数据值字节序可能不同”

## 4. 示例检查

建议确认示例可以直接运行：

- `examples/QLProtocolLibrary.Demo`
- `examples/QLProtocolLibrary.NuGetDemo`

检查重点：

- 是否直接使用 `uint deviceAddress`
- 是否使用文档示例报文
- 是否不再依赖旧的 `mn` 风格接口

## 5. 构建检查

发布前建议至少执行：

```bash
dotnet restore .\QLProtocolLibrary.sln
dotnet build .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release
dotnet test .\tests\QLProtocolLibrary.Tests\QLProtocolLibrary.Tests.csproj -c Release
dotnet pack .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release -o .\artifacts
```

## 6. NuGet 页面体验检查

建议确认：

- 包说明第一页是否足够清楚
- 安装命令是否出现在 README 中
- 是否明确列出主协议结构
- 是否说明可选包络只是可选项
- 示例代码是否可以直接复制

## 7. 发布后自检

发布完成后建议立刻验证：

1. `dotnet add package QLProtocolLibrary`
2. 新建一个空项目引用包
3. 调用 `QlProtocolCommandBuilder.BuildRead(0x10000001, 0x0000, 0x0001)`
4. 调用 `QlProtocolParser.Parse(...)`
5. 验证 XML 注释是否能在 IDE 正常显示

## 8. 下一次发布前

如果仓库主分支已经有新改动，但上一次发布的 NuGet 版本仍然是当前 `Version`，发布前记得先递增版本号并更新 `CHANGELOG.md`。
