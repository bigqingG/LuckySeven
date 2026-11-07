# 基础框架验证记录 — 2026-11-06

结论：基础可玩原型已验证，可继续制作。不是最终 Game Jam 发布验收。

| 检查 | 结果与证据 |
|---|---|
| Web 模块 | 本机 Unity 6000.3.7f1 的 WebGLSupport、Emscripten 及平台程序集存在；实际 Web 构建验证可用 |
| Unity 编译 | Core / Runtime / Editor / EditModeTests / PlayModeTests 程序集编译成功 |
| 规则测试 | `EditMode.xml`：28 passed / 0 failed / 0 skipped。包含 40 个种子 × 30 手的强制可解抽样 |
| 场景流程 | `PlayMode.xml`：1 passed / 0 failed。通过真实 Button.onClick 监听器覆盖规则面板、开局、重掷、撤销、重复确认门禁、三战通关、重开、静音 |
| 实际 Web 构建 | `web-build.txt`：Succeeded，0 errors / 0 warnings，Development，单 Main 场景 |
| 浏览器加载与鼠标 | Edge 156.0.4314.50，无头模式，本地 HTTP；`Browser/summary.json`：0 pageErrors / 0 consoleErrors |
| 浏览器可玩行为 | 实际点击得到 ((2 + 3) + 2) = 7，预览 7 伤害，敌人 12→5 HP，护盾吸收反击，玩家保持 24 HP，进入第二回合 |
| 页面缩放 | 检视 1280×720、1920×1080、1024×768 截图。窄窗口留边，关键区域没有裁切 |
| 资源完整性 | _LuckySeven 目录无遗漏 .meta；字体随包，无远程下载依赖 |
| ZIP | `web-artifacts.json` 记录文件、大小、SHA-256；ZIP CRC 检查通过，index.html 位于根目录 |

## 产物

- Web 文件：`Builds/WebGL`，解压后约 116.3 MB。开发包未做发布压缩与裁剪。
- 上传/传输包：`Builds/LuckySeven-Web-Prototype-20261106.zip`，28,832,481 bytes。
- 本地预览：`http://localhost:8077/`（本次后台服务已启动；重启电脑后用 `Tools/Serve-Web.ps1` 再启动）。
- 8 张浏览器截图与原始浏览器日志：`Docs/Validation/Browser`。

## 已解决的问题

- UnityEngine 与 Core 同名 ExpressionEvaluator 的命名冲突：使用显式类型别名。
- TMP 首次异步导入尚未完成：等待导入后创建并保存场景。
- Web 后处理模块在旧编辑器进程中不可用：确认场景没有未保存更改，重启该项目并以 Web 目标重新加载。前两次原始报告虽然写 Succeeded，但有错误且缺少产物，按失败处理，保留在 web-build-attempt1/2.txt。
- 构建工具增加错误计数和 index.html / WASM 实体文件检查。
- PlayMode 重新加载域后结果回调丢失：InitializeOnLoad 重新注册回调并使用 SessionState 保存测试模式；重跑后结果文件自动保存成功。
- Web 设置固定 60 FPS 触发 emscripten 帧循环提示：Web Player 改为 -1，交给浏览器 requestAnimationFrame 调度。
- 缺省 favicon 404：Web 模板嵌入 SVG 图标。修正后重新打包、重新浏览器实测，控制台错误归零。

## 尚未验证 / 未包含

- 浏览器仅做主菜单、说明、首回合攻击/防护/换手与缩放验证；完整三战的自动流程验证在 Unity PlayMode 中完成。
- 未做 Chrome 与 Edge 两个浏览器各自的人工完整通关，未上传 itch.io，也没有验证真实嵌入页。
- 无头 Edge 的软件 WebGL 驱动有 getInternalformatParameter capability-probe 警告；无页面异常或渲染阻断，不作为真实 GPU 性能证据。
- 未测真实设备帧率、移动端触摸、无障碍及音频听感。
- 正式美术、BGM、中文字体、Release 体积优化、发布页面/Credits/AI 使用声明仍是后续工作。

## 设置改动范围

保留原 SampleScene、URP 管线和包版本。新增 Main 到构建场景首位；Web 构建脚本显式只构建 Main。ProjectSettings 设置 Web 分辨率、后台运行、Gzip 解压回退与自定义模板。Unity 自动记录了输入动作预加载与 batching 默认值。前置快照见 `*.before.txt`。
