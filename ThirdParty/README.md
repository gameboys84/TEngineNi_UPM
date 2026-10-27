# 固定第三方依赖

| 依赖 | 版本 | 来源 | 许可证 |
| --- | --- | --- | --- |
| UniTask | 2.5.11 | https://github.com/Cysharp/UniTask | MIT，见 UniTask/LICENSE |
| YooAsset | 2.3.17 | https://github.com/tuyoogame/YooAsset | 见 YooAsset/LICENSE.md |
| HybridCLR Unity 包 | 8.8.0 | https://github.com/focus-creative-games/hybridclr_unity | 见 HybridCLR/LICENSE |

这些快照来自已有工程使用的包内容，保留元数据与第三方归属。UniTask 源工程的 UPM 子目录没有随附根仓库 LICENSE，本次补入其 2.5.11 标签对应的原始许可证。未升级第三方版本。为支持没有 Test Framework 的新工程，移除了 YooAsset/Editor/AssetArtScanner/AssetArtScannerSetting.cs 中未使用的 using NUnit.Framework.Constraints；未改变运行逻辑。

通过本仓库 Git 子目录引用可以让新项目一次配置完整依赖，不依赖开发机器的本地路径。项目已有同名嵌入式包时，应保留一套并验证兼容性。
