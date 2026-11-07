> Historical V1 implementation record. Current card rules: [CardRulesV2.md](CardRulesV2.md); validation: `Validation/V2`.

# 幸运七：基础框架使用说明

## 运行

Unity 6000.3.7f1 打开本项目，打开 `Assets/_LuckySeven/Scenes/Main.unity`，按 Play。
也可使用菜单 `Lucky Seven > Open Main Scene`。原始 SampleScene 保留，Main 已放到构建场景首位。

操作：数字 → 符号 → 数字 → 符号 → 数字，点击 CONFIRM ATTACK。UNDO 撤销一项，CLEAR 清空。
RE-ROLL 仅在算式为空时可用：点击进入选择模式，选一张数字卡，再点击 RE-ROLL SELECTED 执行；UNDO 取消。
每场一次，排除重掷前的旧值。关卡结束点击 NEXT ENCOUNTER；胜败后点击 TRY YOUR LUCK AGAIN。
主菜单 HOW TO PLAY 显示规则；右上角 SOUND 可静音。没有倒计时。

## 架构

| 路径（均在 Assets/_LuckySeven） | 职责 |
|---|---|
| Scripts/Core | 纯 C# 求值、算式输入、96 路径求解器、种子发牌、战斗状态与数值结算 |
| Scripts/Runtime/Config | GameConfig ScriptableObject，敌人、血量、治疗、发牌概率和种子 |
| Scripts/Runtime/Game | GameFlowController，按钮指令、状态门禁、单个结算协程 |
| Scripts/Runtime/UI | BattleHUDView，序列化 uGUI/TMP 层级与状态展示 |
| Scripts/Runtime/Presentation | 只接收结果，显示反馈、缩放和程序生成音效 |
| Editor | 创建初始场景、构建 Web、调用 Unity Test Runner |
| Tests | EditMode 规则测试与 PlayMode 场景/按钮流程测试 |

Core asmdef 禁止 Unity 引用。Runtime 不引用 UnityEditor。求解器和玩家攻击使用同一求值函数。
卡牌以手牌索引作为身份，重复数字不会相互删除。确认当帧进入 Resolving，敌人未死亡才会进入 EnemyAct。
重开停止旧协程并恢复生命、护盾、关卡、回合、种子、统计、重掷和临时选择。

## 配置与范围

在 `Assets/_LuckySeven/Config/GameConfig.asset` 调整敌人 HP/攻击、玩家 HP、战间治疗和可解抽样概率。
默认值来自设计文档：24 HP，治疗 6，敌人 12/16/20 HP 和 1/2/3 攻击。
教学前两手固定 2/3/2、3/3/2，之后 70% 分支强制可解、30% 自由随机；自由随机也可能有解。
固定种子开关仅控制新开局使用的随机序列。连续相同种子会产生相同手牌和重掷结果。

首版为基础可玩原型：英文文字、几何占位敌人、基础合成音效，无 BGM、正式美术或发布页面。字体使用 TMP 自带 Liberation Sans，许可文件随资源保留。
参考设计文档已经复制到 Docs，原始文档未修改。不实现账号、联网、装备、存档或卡组构筑。

## Web 构建

本机 `D:/6000.3.7f1/Editor/Data/PlaybackEngines/WebGLSupport` 与 Emscripten 已检查存在。
Unity 菜单 `Lucky Seven > Build Web Prototype` 输出开发构建到 `Builds/WebGL`，仅包含 Main。
发布设置使用 Gzip + Decompression Fallback，避免简单静态服务器缺少压缩响应头时不能加载。本轮 Development 构建实际输出未压缩文件，Unity 会跳过开发构建的发布压缩；ZIP 仅用于运输与上传。
自定义模板 `Assets/WebGLTemplates/LuckySeven` 提供 16:9 自适应画布、进度条和加载错误提示。
这是 Unity 的解压回退功能，参考 [Unity 6000.3 API](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/webgl/decompressionfallback)。

PowerShell 执行 `./Tools/Serve-Web.ps1`，浏览器打开 `http://localhost:8077/`；不要直接双击 index.html。
上传 itch.io 时将 WebGL 文件夹内容压成 ZIP，保证 index.html 在压缩包根目录。此任务不发布到外部网站。
最终提交前还需 Release 构建、itch.io 嵌入验证、Chrome/Edge 实机完整通关、正式 Credits/AI 使用说明。

## 重跑验证

Unity 菜单 `Lucky Seven > Run EditMode Tests` 和 `Run PlayMode Tests`。
也可运行 `./Tools/Invoke-UnityCommand.ps1 edit-tests`（或 play-tests / build）。此脚本向当前项目编辑器的 Assets > Refresh 发送菜单消息，在本项目 Temp 中写入有限命令；不安装 MCP，不监听网络。
结果：`Docs/Validation/EditMode.xml`、`PlayMode.xml`、`web-build.txt`。

项目设置有意改动：`EditorBuildSettings.asset` 添加 Main；`ProjectSettings.asset` 设置产品名、Web 1280×720、后台运行、Gzip 解压回退、自定义 Web 模板。没有升级包、切换渲染管线或变更输入系统。

Unity 还自动序列化了既有 InputSystem_Actions 的 preloadedAssets 引用及 Standalone batching 默认值，并将编辑器活动目标切换到 Web。基础模板已有的包均保留。

首次 Web 打包出现后处理模块未加载的情况（虽然原始报告错误地标为 Succeeded，但没有产物，因此按失败保留证据）。确认场景已保存后重新打开项目，以 Web 目标加载模块，随后实际打包成功。构建工具同时校验错误计数、index.html 和 WASM 文件，避免只依据状态字段报告成功。
