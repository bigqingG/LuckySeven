# 资源与组合 V1 验收

日期：2026-11-07。Unity 6000.3.7f1，WebGL 非 Development 构建。

## 实现范围

抽 2；换 2 抽 3；1/2/3 张及以上凑数破防后，单次攻击 +0/+2/+4。回响仍额外抽 1。组合奖励和预计伤害双语显示，预览与结算共用规则。其他数值未变。

`source-check.json` 确认 GameConfig 和 Main 场景文件均与接入前备份相同；本轮新增内容通过原有规则及 HUD 组件接入。源文件变化见 `changed-files.json`。配置 SHA-256：`643f5c1f630df2ac9b20d166030df582107f37ab31916a58835cd780b02f4f21`。

## 测试结果

| 层级 | 结果 | 证据 |
|---|---|---|
| EditMode | 39 通过，0 失败 | EditMode.xml |
| PlayMode | 5 通过，0 失败 | PlayMode.xml |
| Web 构建 | 成功，0 错误，0 警告；35,963,775 bytes | web-build.txt |
| Edge 浏览器实操 | 78 步，三场胜利，重开成功；无 JS pageerror | Browser/summary.json、actions.json |
| ZIP | 18 个构建文件 + 试玩说明；CRC 与逐文件 SHA-256 比对通过 | package.json |

规则测试包括两个幸运目标下 1/2/3/4 张组合、奖励封顶、破防不伤血及不触发附效、换攻击牌/清空选择后保留奖励、预览无副作用且等于实伤、奖励只消耗一次、普通攻击不继承奖励、跨回合/战斗/重开无残留、回响与两种补牌、牌库守恒及完整流程。

运行测试覆盖语言切换前后组合/预览状态、文字区域尺寸、像素表现、攻击后护甲恢复、点击/拖拽、三场流程和重开。首次运行发现公式区域行高不足，已把该区域字号上限从 25 调整为 22；复测全部通过。初次失败结果保留为 PlayMode-before-layout-fix.xml。

浏览器脚本为 `Tools/Validate-ResourceCombo.py`，通过真实鼠标操作和只读可见 HUD 状态核验，没有注入游戏状态。实际覆盖 +0/+2/+4，普通及破防攻击预计伤害与扣血一致，抽2、换2抽3、语言切换、悬停与相邻牌点击、无效落点回弹、中央收回、1280×720 / 960×540。截图见 `Browser/preview-combo-4.png`、`preview-english.png`、`16-result.png`。

## 交付与边界

- 本机服务：`http://127.0.0.1:8078/`，服务指向 `Builds/WebGLV2`，验收时 HTTP 200。
- ZIP：`Builds/LuckySeven-ResourceComboV1-WebGL.zip`，34,632,696 bytes，校验信息见 package.json。
- Edge 使用 headless SwiftShader 验收。WebGL INVALID_ENUM、持久化同步弃用警告和 URP 内部着色器兼容性日志共 6 种诊断，与像素版本基线逐条一致；并非浏览器控制台零诊断。对比见 browser-diagnostics-comparison.json，JS pageerror 为 0。
- 未发布到公网；未把自动通关当作长期平衡性证明。
- 接入前备份：`Backups/BeforeResourceComboV1`。
