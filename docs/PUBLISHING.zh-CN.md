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

建议补充但当前代码里未强制写死的字段：

- `PackageProjectUrl`
- `RepositoryUrl`
- `RepositoryBranch`
- `PackageLicenseExpression` 或 `PackageLicenseFile`
- `PackageIcon`
- `PackageReleaseNotes`

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

## 4. 示例检查

建议确认示例可以直接运行：

- `examples/QLProtocolLibrary.Demo`

## 5. 构建检查

发布前建议至少执行：

```bash
dotnet build .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release
dotnet pack .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release -o .\artifacts
```

## 6. NuGet 页面体验检查

建议确认：

- 包说明第一页是否足够清楚
- 安装命令是否出现在 README 中
- 高层 API 是否说明清楚
- 调用者是否知道“无地址用法”和“通用类型用法”两种模式
- 是否有示例代码可以直接复制

## 7. 发布后自检

发布完成后建议立刻验证：

1. `dotnet add package QLProtocolLibrary`
2. 新建一个空项目引用包
3. 调用 `QlKnownOperations.DeviceTime.BuildRead("1001")`
4. 调用 `QlProtocolParser.Parse(...)`
5. 验证 XML 注释是否能在 IDE 正常显示
