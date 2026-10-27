# TEngineNi UPM

Ni 共用 Unity 包的独立源码仓库。最低验证环境为 **Unity 2022.3.62f2**，使用 Git 子目录依赖导入；本仓库根目录本身不是一个 Unity 包。

| 包 | 版本 | Git 子目录 |
| --- | --- | --- |
| Ni TEngine | 0.6.0 | com.ni.tengine |
| Ni GameCore | 0.1.0 | com.ni.gamecore |
| UniTask | 2.5.11 | ThirdParty/UniTask |
| YooAsset | 2.3.17 | ThirdParty/YooAsset |
| HybridCLR | 8.8.0 | ThirdParty/HybridCLR |

第三方源码为固定版本快照，保留原作者、许可证和元数据。Ni 包内不包含项目专有入口、网络协议、配置表、业务场景或 SDK 签名。

## 新项目安装

先安装 Git，并在 Unity 2022.3 项目 `Packages/manifest.json` 的 `dependencies` 中加入下列条目，保留工程原有依赖：

```json
{
  "com.cysharp.unitask": "https://github.com/gameboys84/TEngineNi_UPM.git?path=/ThirdParty/UniTask#main",
  "com.tuyoogame.yooasset": "https://github.com/gameboys84/TEngineNi_UPM.git?path=/ThirdParty/YooAsset#main",
  "com.code-philosophy.hybridclr": "https://github.com/gameboys84/TEngineNi_UPM.git?path=/ThirdParty/HybridCLR#main",
  "com.ni.tengine": "https://github.com/gameboys84/TEngineNi_UPM.git?path=/com.ni.tengine#main",
  "com.ni.gamecore": "https://github.com/gameboys84/TEngineNi_UPM.git?path=/com.ni.gamecore#main"
}
```

这是开发分支示例。稳定项目将五处 `main` 换为同一个经过验证的完整 40 位 commit SHA，并提交 `manifest.json` 和 Unity 生成的 `packages-lock.json`。若仓库需要认证，预先配置 Git 凭据，或改用 `ssh://git@github.com/gameboys84/TEngineNi_UPM.git`；不要把口令或 token 写入 manifest。

也可克隆本仓库后运行：

```powershell
./Tools/Install-NiPackages.ps1 -ProjectPath E:/YourUnityProject -Revision main -WhatIf
./Tools/Install-NiPackages.ps1 -ProjectPath E:/YourUnityProject -Revision main
```

脚本仅合并上述五个依赖，保留其他 manifest 字段；发现同名嵌入式依赖时默认保留，避免导入同一个包的两份源码。然后打开 Unity 等待解析与编译。若已有同名包，先核对版本和程序集，不要同时保留 Assets 内旧框架与新包。

Unity 不支持在包的 package.json 中嵌套 Git URL 依赖。因此不要只向空工程添加 GameCore URL 并假定 Git 依赖会自动递归安装；使用完整模板或安装脚本。uGUI、TMP、Newtonsoft.Json 和 Scriptable Build Pipeline 等注册表依赖由 UPM 正常解析。

## 导入后使用

- `TEngine.Runtime` / `TEngine.Editor` 提供基础模块与工具；`Ni.GameCore` / `Ni.GameCore.Editor` 提供 UI、存档、单例、集合、资源引用、实例池和列表等共用能力。
- 命名空间继续使用 `TEngine`、`GameCore`。已有脚本与 asmdef 元数据随包维护；不要复制成第二套源码。
- Unity 工程内保存可修改的启动、资源采集、语言、输入及 UI 生成配置。包导入不会自动生成产品场景或提供可联网的 SLG 游戏。
- 普通资源需先完成 YooAsset 初始化，加载后配对释放；实例销毁和池化生命周期按接口约定使用。
- UI 生成器通过 `TEngine/Create ScriptGeneratorSetting` 建立项目设置，生成代码留在 Assets。HTML 烘焙器依赖 UI 生成器，因此归入 GameCore Editor。
- 可参考 [UnityGameEngine 演示工程](https://github.com/gameboys84/UnityGameEngine) 的 Launcher、窗口、资源与测试使用方式。
- HybridCLR 安装工具、IL2CPP/AOT 元数据、裁剪和设备代码热更新须由具体项目独立配置和验收；包可编译不代表已完成热更发布。

## 版本与源码管理

Ni TEngine 当前使用映射 `TEngine M.N.P → Ni 0.M.N`：基于上游包声明 6.0.0，当前为 0.6.0；完整上游补丁号和 Ni 改动另记来源，不声称纯净提交复刻。GameCore 使用独立版本 0.1.0。未来需要连续独立发版时再明确递增规则，不覆盖已发布版本。

本仓库维护包源码；TEngineNi 的 `tengineni_upm` 分支以 `FrameworkPackages` 子模块记录本仓库提交，继续保留 Ni UnityProject 和原 fork 历史。消费工程稳定使用固定 Git SHA，开发时可以临时切换本地检出目录。

上游同步与包升级分别管理：先审阅 TEngine 上游变更并合并到包，再验证 Ni 和消费工程；包提交完成后更新 Ni 子模块指针与消费工程的依赖版本。修改必须保留 .meta / GUID，不能用目录覆盖更新回旧 Assets 布局。

## 检查与许可证

`python Tools/validate-packages.py` 检查五个包的身份、版本、依赖闭包、元数据和许可证。实际 Unity 验证结果记录在 [VALIDATION.md](VALIDATION.md)，原始本机日志不入库。

根 LICENSE 适用于本仓库自有内容；各包内及 ThirdParty 下的许可证和原始版权声明继续适用于相应内容，不能被根许可证替代。第三方来源见 [ThirdParty/README.md](ThirdParty/README.md)。
