# Contributing

中文说明在下方，English summary follows after that.

## 中文

欢迎对 `QLProtocolLibrary` 提交 issue、建议和代码贡献。

推荐贡献方向：

- 新寄存器定义
- 新的高层业务解析器
- 更完整的业务模型
- README / API 文档完善
- 测试样例补充
- 包元数据和发布流程优化

### 提交建议

1. 先描述问题或目标
2. 尽量附上协议报文样例
3. 如果涉及寄存器，请说明地址、长度、数据类型、期望返回含义
4. 如果涉及行为变更，请同步更新 README 或 API 文档

### 文档要求

如果新增公开 API，建议至少同步更新以下任一位置：

- `src/QLProtocolLibrary/README.md`
- `docs/API.zh-CN.md`
- `docs/API.en.md`
- 示例项目

### 代码风格

- 优先保持 `netstandard2.0` 兼容
- 避免引入不必要的第三方依赖
- 公共 API 尽量补充 XML 注释
- 优先让高层 API 简洁、稳定、可发现

## English

Contributions are welcome.

Typical contribution areas:

- new register definitions
- new high-level business parsers
- richer typed business models
- README / API documentation improvements
- more test cases and frame samples
- package metadata and publishing workflow improvements

### Suggested workflow

1. describe the problem or goal first
2. include protocol frame samples when possible
3. if a register is involved, include address, length, payload type, and expected meaning
4. if public behavior changes, update README or API docs as well

### Documentation expectation

When you add or change a public API, also update at least one of these:

- `src/QLProtocolLibrary/README.md`
- `docs/API.zh-CN.md`
- `docs/API.en.md`
- demo project
