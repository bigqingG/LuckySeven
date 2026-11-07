# 复古像素冒险接入

日期：2026-11-07。用户选择方案7并授权接入，基于 Docs/VisualExploration/20261107 的视觉稿。

## 实现范围

- 沿用 Main 场景、uGUI + TMP、Unity Localization、原有按钮与 GameFlowController。
- 石砖背景、阶梯金边、幸运数字徽章、状态面板、菜单/结算/帮助面板统一换肤。
- 角色使用单张透明图集：掌牌人、残缺之骰、偏差之影、失控之七。Unity 中切为4张精灵；Point采样、无Mipmap，纹理最大1024。
- 手牌保留真实文本角标，中央数值改为程序绘制像素数字。红黑花色不改变规则；盾/剑/循环箭头配三色附效标签。
- 普通8–9张手牌以较宽间距叠放；超限弃牌阶段按数量压缩。悬停仍上移35参考像素、放大1.14倍、置顶，并增加金边强调；邻牌点击区仍留在原位。
- 抽牌和换牌入口持续可见，非补牌阶段禁用；牌库、弃牌、回响加抽等状态仍来自规则层。
- 破防显示短暂护甲碎片；攻击显示短斩击、受击闪亮、独立伤害数字。破防不扣血；已破防的护甲在本次攻击和反击期间不提前显示恢复。
- 中文/英文仍由现有Localization管理；新增6个双语条目，沿用字体，Web加载页配色同步更新。

规则层 BattleSession.cs 与 GameConfig.asset 保持原样，源文件SHA256对比见 Docs/Validation/PixelAdventure/baseline-hashes.json。视觉变化未改变伤害、抽牌、敌人反击或流程等待时间。

## 实现文件

- Scripts/Runtime/UI/PixelGraphic.cs：复用像素形状、背景、图标、数字。
- Scripts/Runtime/UI/PokerCardFace.cs：像素牌面，保留稳定卡牌身份对应花色。
- Scripts/Runtime/UI/BattleHUDView.Pixel.cs：定向场景换肤与状态展示。
- Scripts/Runtime/UI/CardMotion.cs：保留原有交互，增加悬停金边。
- Scripts/Runtime/Presentation/PixelBattleFx.cs：纯视觉反馈；不负责结算。
- Editor/LuckySevenPixelTools.cs：图集导入、本地化更新、可重复执行的场景接入。
- Tools/Validate-PixelAdventure.py：真实WebGL鼠标验收；只读可见状态，不注入战斗数据。

以上Scripts/Editor路径均位于 Assets/_LuckySeven 下。

## 工具与备份

接入前备份：Backups/BeforePixelAdventure20261107，包含 _LuckySeven、Tools 与 WebGLTemplates。没有Git仓库，所以采用独立目录备份。

使用现有项目级Unity菜单自动化，没有安装MCP桥接或新包。Unity编辑器保持打开，目标WebGL。

重现：
1. Tools/Invoke-UnityCommand.ps1 -Command pixel-skin（重做换肤，需已保存场景）
2. -Command edit-tests / -Command play-tests
3. -Command build
4. Tools/Serve-Web.ps1 后运行 Tools/Validate-PixelAdventure.py

网页入口为 Builds/WebGLV2，通过HTTP访问，不直接双击index.html。

## 美术来源与边界

角色图集由内置 image_gen 根据已选方案生成，路径 Assets/_LuckySeven/Art/PixelAdventure/Characters.png；切片由Unity导入器完成。所有正式UI数字/文字是可变文本或像素几何，不使用整张概念图冒充界面。

像素风美术不是严格的全屏整数倍像素渲染。为保留现有1.14倍悬停及任意浏览器窗口缩放，文本使用TMP，角色使用Point采样。后续如需严格像素网格，应单独评审缩放行为，不能悄悄改变当前交互参数。

性能预算未作专项测量。视觉反馈采用固定6个碎片和复用的UI元素，无全屏Bloom或运行时贴图生成。

最终验证结果、截图及构建日志见 Docs/Validation/PixelAdventure。

