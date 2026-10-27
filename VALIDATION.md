# 验证记录

2026-10-27，Windows / Unity 2022.3.62f2，com.ni.tengine 0.6.0、com.ni.gamecore 0.1.0。

- 五包静态检查通过：身份、版本、依赖闭包、许可证、脚本 .meta、重复 GUID 和文件大小。
- 全新最小 Unity 工程通过本地目录导入并完成编译，无需消费工程 Assets 或测试框架。
- 包身份/程序集、UniTask/YooAsset、1000 项优先队列排序及空队列保护、环形列表顺序与副本隔离、存档读写与路径穿越保护，共五组检查通过。
- 新工程导入修正：Ni TEngine 显式声明所用 Unity 内置模块；YooAsset 编辑器移除一条未使用的 NUnit using，未改变运行逻辑。
- 历史 2026-10-26 消费工程回归：EditMode 3/3、PlayMode 10/10、26 Prefab 无缺失脚本、完整 .NET 构建通过。独立发布阶段重新运行结果如下。
- 固定提交 `7a71e9eed0511767df01ac12deaf440d6843fa43` 的五项 Git 子目录依赖在第二个全新 Unity 工程成功解析，实际代码来自 Library/PackageCache；上述五组基础功能再次通过。
- 消费工程引用同一包源码重新执行 EditMode 3/3、PlayMode 10/10，失败/跳过均为 0，PlayMode 约 35.40 秒。
- 消费工程完整生成解决方案 .NET 构建成功。首次与 Unity 测试并行构建遇到 Temp 输出被清理，待 Editor 退出后独立重跑通过。

Git 安装预发布验证采用本机 Git transport，验证真实提交、子目录提取与不可写 PackageCache。远端 HTTPS 依赖将在包推送后由消费工程重新解析，并在消费工程文档记录结果。

Ni 源工程 CLI/Pipeline 缺失，尚未取得其独立 Editor/PlayMode 验收。设备、IL2CPP、AOT 元数据、泛型、裁剪、HybridCLR 设备热更新及真实远端资源下载不在此次 Editor 回归范围内。

原始日志和临时工程留在消费工程的 Design/reports 与 Design/local，不加入版本管理。批处理进程需有正常 Windows 环境变量；本次显式提供进程级 ALLUSERSPROFILE 后 UPM 正常运行，未更改系统环境设置。
