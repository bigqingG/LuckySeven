# 复古像素冒险接入验收

日期：2026-11-07（用户时区 Asia/Tokyo；Unity XML内部记录UTC）。

状态：Ready with limitations。已完成本地Unity与WebGL验收，可在本地试玩；未发布到外部平台，未做专项性能或跨浏览器/移动设备验收。

## 结果

| 验收项 | 结果 | 证据 |
| --- | --- | --- |
| Unity编译与WebGL构建 | 通过，0错误、0构建警告 | web-build.txt |
| EditMode规则测试 | 25通过，0失败，0跳过 | EditMode.xml |
| PlayMode流程与视觉交互 | 4通过，0失败，0跳过 | PlayMode.xml |
| 新像素图形确实生成可见网格 | 通过 | PixelPresentationTests + 实际截图 |
| 悬停抬起/缩放/置顶、邻牌可点击 | 通过 | LocalizationCardTests；Browser/06-hover.png |
| 点击抬起牌、拖入、拖回、无效落点 | 通过 | TableInteractionTests；Browser/actions.json |
| 破防不扣血、凑数不触发附效 | 通过 | PixelPresentationTests；Browser/11-break.png |
| 单独攻击、正确伤害、护甲下一回合恢复 | 通过 | PixelPresentationTests；Browser/14-hit.png |
| 攻击结算提示匹配当前状态 | 通过 | PlayMode断言与最终Browser/14-hit.png |
| 中英文切换保留手牌和选择状态 | 通过 | LocalizationCardTests；Browser/08-battle-english.png |
| 抽/换牌入口可见，非补牌阶段禁用 | 通过 | PlayMode断言与浏览器点击检查 |
| 1280×720与960×540窗口 | 通过，已检查截图 | Browser/05-hand.png、09-small-window.png |
| 三场战斗、胜利和重开 | 通过；最终浏览器79次鼠标操作 | Browser/summary.json、16-result.png、17-replay.png |
| 核心规则与战斗配置未改 | SHA256一致 | rules-unchanged.json、baseline-hashes.json |
| 主场景GUID保留、无原资源删除 | 通过 | changed-files.json |
| 新脚本/美术meta齐全 | 静态检查通过 | Unity导入与文件检查 |

最终PlayMode用例：
- FlowTests.CardUiCompleteRunAndReplay
- LocalizationCardTests.ChineseEnglishSwitchPreservesBattleAndHoverIsReadable
- TableInteractionTests.StageReturnAndDragRejectInvalidDrops
- PixelPresentationTests.PixelSkinShowsArtAndSeparatesBreakFromDamage

## 浏览器环境与诊断

使用本机Microsoft Edge的无头Chromium模式，Playwright驱动真实鼠标事件；只读取已有公开HUD镜像，不修改或注入战斗状态。浏览器通关使用正常随机开局，界面中的数字不要求与概念图固定示例相同。PlayMode规则回归使用确定性测试局。

最终浏览器没有JavaScript页面异常。浏览器诊断去重后6种，与旧LocalizedCards浏览器记录相同，没有新增诊断。原有项包含SwiftShader环境的WebGL格式查询、部分URP内部Shader不支持提示、Unity持久化接口弃用提示；完整比较见 browser-diagnostics-comparison.json。不能将“0构建警告”描述成“运行时日志完全无提示”。

早期可视检查发现PixelGraphic缺CanvasRenderer，已补齐并增加真实网格检查。最终截图已重新生成，所有验收结果对应修复后的版本。

## 构建

Unity 6000.3.7f1，目标WebGL，非Development。
- 输出：Builds/WebGLV2
- 最终构建大小：35,936,543字节，约34.27 MiB
- 构建耗时：2分33.83秒
- 18个产物的路径、大小及SHA256：web-artifacts.json
- 本地HTTP：[试玩入口](http://127.0.0.1:8078/)

启动脚本：Tools/Serve-Web.ps1。
复现浏览器验收：python Tools/Validate-PixelAdventure.py。
当前本地服务已运行；若浏览器仍显示旧版本，重新加载页面。

## 实际运行截图

![中文战斗界面](Browser/05-hand.png)

![悬停](Browser/06-hover.png)

![破防](Browser/11-break.png)

![攻击命中](Browser/14-hit.png)

![英文界面](Browser/08-battle-english.png)

## 交付边界

已接入界面、角色美术、卡牌、悬停与破防/命中反馈。文字与数值是实际UI，不是概念图整屏贴图。角色图集采用Point采样，但保留1.14倍悬停和任意窗口缩放，因此不宣称严格整数倍像素完美。

性能、触屏手感、Safari/Firefox、低端硬件、音频质量均未专项验收。现有Game Jam网页玩法与参数保持不变。素材、架构及备份说明见 Docs/PixelAdventureIntegration.md。

