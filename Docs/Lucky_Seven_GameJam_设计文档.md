# 《幸运七 / LUCKY SEVEN》游戏设计文档

> **项目类型：** 2026 Official Unity Game Jam · 单人短周期作品  
> **赛题：** Lucky Number（幸运数字）  
> **文档类型：** GDD（玩法）+ TDD（技术）+ ADD（美术与音频）  
> **版本：** v1.0 · 2026-11-06  
> **开发引擎：** Unity 6（建议使用已安装且稳定的版本）  
> **目标平台：** WebGL / itch.io，桌面浏览器优先  
> **目标时长：** 单局 5–10 分钟，首次理解操作不超过 60 秒  
> **核心原则：** **7 是规则，不是装饰；先做可玩闭环，再做美术；用最少系统实现清晰的惊喜。**

---

## 0. 一页概要

### 0.1 游戏概念

玩家坐在一台神秘的「命运校准机」前，对抗由坏运气形成的数字怪物。每回合机器发出 **3 张数字票**（1–9），玩家调整数字顺序，并选用 **2 次四则运算**，将结果尽量凑成 **7**。结果越接近 7，命运校准越成功，能够对敌人造成越高伤害；**恰好等于 7**，会触发「幸运七」金色爆发，并获得一点护盾。

这不是传统数学考试：每回合有有限的改运机会，运气不佳时也能打出保底伤害。玩家的思考是：「我能否在这三个看似随机的数字里找到属于我的 7？」

**一句话宣传：** *Three numbers. Two operations. One lucky number.*  
**中文标语：** *三张数字，两次运算，改写你的幸运。*

### 0.2 最小可交付体验（MVP）

- 单屏 2D 桌面游戏；一个开始界面、一个战斗界面、一个胜败界面。
- 玩家依次选择 3 张数字卡与 2 个运算符，构造左结合表达式：`((A op1 B) op2 C)`。
- 判定结果与 7 的距离，造成伤害；敌人存活则反击。
- 3 场连续战斗（前两场常规怪 + 最终怪），战间恢复少量生命。
- 每场战斗可重掷一张尚未使用的数字卡 **1 次**。
- 有教程、胜败反馈、基础音效、可重开流程。
- WebGL 构建可在 itch.io 页面直接运行。

### 0.3 明确不做

不做卡组构筑、背包、商店、永久成长、联网、账号、剧情分支、复杂随机事件、多语言系统、3D 实时场景、长教程、数十张卡牌。这些都不能优先于核心算式体验。

---

# 第一部分：玩法设计（GDD）

## 1. 游戏目标与体验支柱

| 支柱 | 玩家感受 | 对应实现 |
|---|---|---|
| 幸运可以被理解 | “随机数字里有解！” | 以 7 为目标、有限但清晰的表达式规则 |
| 幸运可以被改写 | “再给我一次机会。” | 每战一次数字重掷 |
| 幸运值得庆祝 | “我凑出了 7！” | 金色特效、独特音效、伤害与护盾奖励 |
| 每一步都有意义 | “没凑到也可以继续玩。” | 非完美结果保底伤害，不会卡关 |

**推荐定位：** 轻策略数学解谜 × 回合制数字战斗；桌面鼠标优先，键盘快捷键作为加分项。

## 2. 核心玩法流程

```text
开始游戏
  ↓
20–40 秒交互式教程（首局）
  ↓
进入第 1 场战斗
  ↓
每回合随机获得 3 张数字卡
  ↓
[可选] 在开始组算式前，重掷一张卡（每场仅 1 次）
  ↓
按顺序选择：数字 → 运算符 → 数字 → 运算符 → 数字
  ↓
系统实时预览左结合表达式和计算结果
  ↓
确认攻击 → 根据与 7 的偏差结算伤害与护盾
  ↓
敌人若未死亡：扣除护盾后反击 → 进入下一回合
  ↓
敌人死亡：战间治疗 → 下一场
  ↓
第 3 场胜利：通关；玩家 HP ≤ 0：失败；均可重新开始
```

### 2.1 一回合严格规则

1. 生成 3 张数字卡，取值均为整数 `1–9`，**允许数字重复**，每张卡在本回合只能使用一次。
2. 玩家可以任意排列三张卡的使用顺序，例如原始手牌 `3, 2, 8` 可选择 `8, 3, 2`。
3. 玩家必须使用 **且只使用** 3 张卡、2 个运算符。运算符可重复，例如 `+,+` 或 `×,×`。
4. 支持 `+、−、×、÷`；所有运算严格**从左到右**执行，实际表达式为 `((A op1 B) op2 C)`。UI 必须显示括号，避免与普通数学优先级产生歧义。
5. **除法只接受整除**：`8÷2=4` 合法，`5÷2` 不合法；**除以 0** 不合法。可以产生 0 或负数，均为合法整数结果。
6. 表达式未完成、任一步不合法时，「确认攻击」按钮禁用，并显示原因。
7. 确认后统一结算；不能在敌人回合修改表达式；下一回合全部卡与算式重置。
8. 不设置实时倒计时，让玩家可以从容思考；后期如确有必要，再试验限时玩法（不纳入 MVP）。

**正确示例：** `((3 × 3) − 2) = 7`、`((8 − 3) + 2) = 7`、`((6 ÷ 2) + 4) = 7`。  
**顺序示例：** `((2 + 3) × 2) = 10`，不是 `2 + (3 × 2) = 8`。  
**非法示例：** `((5 ÷ 2) + 3)`，第一步非整除。

### 2.2 操作方式（尽量不用拖拽）

- 鼠标单击某数字卡：自动填入**下一个数字槽**；该卡被标记为已使用。
- 单击运算符：自动填入**下一个运算符槽**；运算符按钮可重复使用。
- 必须遵循 `数字 → 运算符 → 数字 → 运算符 → 数字` 的选择顺序；不符合当前步骤的按钮置灰。
- `撤销`：退回上一项，恢复对应数字卡；`清空`：重置整条算式；`确认`：计算并攻击。
- `重掷`：**仅当算式为空**且本场次数未使用时，先选中一张手牌，再点击重掷；重掷后取消选择。
- 键盘可选：数字卡 `1/2/3`、运算符 `Q/W/E/R`、撤销 `Backspace`、确认 `Enter`；第一版只保证鼠标操作即可。

> **交互重点：** 不要把“卡片放到错误位置”的操作当作有效输入；状态驱动可点击区域，杜绝无声失败。

## 3. 结算数值

### 3.1 攻击伤害

设合法结果为 `R`、偏差 `D = |R - 7|`：

| 条件 | 伤害 | 额外效果 | 表现 |
|---|---:|---|---|
| `D = 0` | 7 | 获得 1 点护盾 | 金色「LUCKY 7」冲击 |
| `D = 1` | 4 | 无 | 蓝绿高亮、数字靠近 7 |
| `D = 2` | 2 | 无 | 普通蓝色反馈 |
| `D ≥ 3` | 1 | 无 | 灰白低强度反馈 |

- 伤害不随正负方向变化；例如结果 6 和 8 都造成 4 点。
- 结果绝不因偏差太大而变成 0 伤害，以免连续“无解手牌”导致停滞。
- 护盾可以累积，上限 **3 点**；受击时先消耗护盾，再扣生命；每场战斗结束时护盾清零。
- 击杀敌人的回合，敌人**不反击**。

### 3.2 数值初稿（必须试玩验证）

| 对象 | 最大 HP | 每回合攻击 | 描述 |
|---|---:|---:|---|
| 玩家 | 24 | 不适用 | 战间恢复 6 HP，最多恢复到 24 |
| 关卡 1：破运骰 | 12 | 1 | 教学关，强调简单算式 |
| 关卡 2：偏差之影 | 16 | 2 | 需要更频繁找到 7 |
| 关卡 3：失控七号 | 20 | 3 | 首领，造型和特效更强，但**不增加新规则** |

**首轮测试目标：** 新玩家 5–10 分钟能通关；第一次看到「幸运七」的平均时间低于 90 秒；试玩失败更多由策略选择造成，而不是看不懂操作。

### 3.3 改运：一次重掷

- 每场战斗开始拥有 **1 次重掷**，使用后本场清零，下场恢复。
- 先在三张手牌中选中一张，再点击 `RE-ROLL`，该卡替换为 `1–9` 的随机新数字；允许与原数相同，但为减少挫败感，**建议**排除与旧数相同（配置可开关）。
- 只有算式为空时可使用。重掷后，不能撤销重掷。
- 重掷不可重置其他卡，也不能重掷敌人。

> **备选增强（仅在 MVP 打磨完毕后）：** 触发「幸运七」返还一次重掷，但每战最多返还 1 次；默认不要加入，避免数值和 UI 复杂化。

## 4. 随机数字设计：可控的运气

纯随机 `1–9` 三张卡未必能得到 7。如果完全无解，游戏虽仍能造成保底伤害，但玩家可能误认为“自己没找出答案”。因此建议把可解性当作**发牌质量约束**，而不在 UI 上公布答案。

### 4.1 建议生成策略

- **新手教学第 1 回合：** 固定 `2、3、2`，通过 `2+3+2=7` 引导操作。
- **新手教学第 2 回合：** 使用可解牌组，如 `3、3、2`，允许玩家发现 `3×3−2=7`。
- **后续回合：** 目标 **70% 的牌组可得到 7，30% 使用自由随机牌组**；不能把“70% 的牌组必然有唯一解”理解为 70% 的玩家必然能算出。
- 通过候选生成 + 求解器检验实现：随机生成三数；判断是否至少有一种排列和两次运算得到 7；对“应可解”的牌组不符合则重试，最多 100 次；重试耗尽则从预制可解牌组池回退。
- 对“自由随机”牌组不强制无解——如果碰巧可解，保留。这样实际可解比例通常略高于 70%。

### 4.2 求解器枚举算法

对三张卡所有至多 `3! = 6` 种排列与 `4 × 4 = 16` 个运算组合进行检测，每组最多 96 条路径，规模极小。

```text
HasSevenSolution(cards):
    foreach ordered (a,b,c) in uniquePermutations(cards):
        foreach op1 in [+,-,*,/]:
            foreach op2 in [+,-,*,/]:
                if TryEvaluate(a,op1,b,op2,c,out result) and result == 7:
                    return true
    return false
```

所有运行用同一套 `TryEvaluate` 规则，避免“发牌求解器认为有解，但 UI 不认”这种致命错误。

**建议保留调试开关：** 在开发版 Console 中打印 `Seed / 手牌 / 可解表达式之一`，正式版关闭；不要在游戏本体提供“自动解题”按钮。

## 5. 三场战斗的体验层次

### 第一场｜破运骰（60–120 秒）
- 视觉上是一颗开裂的黑色骰子形怪物；主要负责让玩家理解操作。
- 首回合固定可解牌；屏幕提示：“让结果变成 7。”
- 在玩家输入数字后，逐步只强调下一个可操作按钮。

### 第二场｜偏差之影（90–180 秒）
- 由不完整数字和错位光带构成；不引入新操作，仅提高 HP 与攻击。
- 玩家自然意识到：并非每一手都能完美，接近 7 也有价值。

### 第三场｜失控七号（120–240 秒）
- 一个被噪声侵蚀的巨大“7”型核心装置。阶段仅通过血量和视觉强度升级。
- 最后一击如果恰好为 7，播放强化的金色终结演出；如果不是 7，也必须能正常通关。

**通关条件：** 三场敌人的 HP 全部降到 0。  
**失败条件：** 玩家 HP 归零。  
**重玩条件：** 一键重新开始，回到满血、初始第一场。

## 6. UI 状态与引导

- `MainMenu`：标题、Play、How to Play、音量开关。
- `Tutorial`：使用真实战斗界面承载提示，不单独制作教程场景。
- `Battle / Input`：玩家选卡、改运、选符号、撤销、确认。
- `Battle / Resolve`：停止输入 → 算式飞向中央 7 → 伤害数字 → 怪物受击 → 可能反击。
- `BetweenBattles`：显示 “FATE RECALIBRATED” 与恢复生命；点击下一关。
- `Victory`：恭喜、击中 7 次数、本局回合数、Replay。
- `Defeat`：失败原因提示、Replay。

**核心可访问性：** 不靠颜色单独传达信息；每种偏差都显示明确数字/文本；提供音效静音开关；UI 支持 1280×720 到 1920×1080 常用桌面窗口。

---

# 第二部分：技术设计（TDD）

## 7. 技术选型与约束

| 模块 | 方案 | 理由 |
|---|---|---|
| 引擎 | Unity 6 稳定版 | 熟悉的 C# 工作流，易于 WebGL 构建 |
| 渲染 | 2D URP（或项目已确定的稳定 2D 管线） | 以少量 Sprite、UI、粒子完成视觉表现 |
| UI | UGUI + TextMeshPro | 快速搭建并动画化单屏游戏 |
| 输入 | `Button.onClick` 为主 | 只需鼠标指针，无复杂设备依赖 |
| 逻辑 | 纯 C# 算式计算与数值结算 | 易测试、与 UI 解耦 |
| 配置 | 少量 ScriptableObject | 敌人参数、色彩与音效引用可调 |
| 动画 | Animator / Coroutine / Tween 三选一 | 不引入复杂动画框架；以项目现有依赖为准 |
| 随机 | `System.Random` + 可记录 Seed | 可复现调试、便于定位随机发牌 Bug |
| 保存 | 不需要 | 单局 5–10 分钟，避免存档风险 |
| 运行目标 | 桌面浏览器 WebGL | 对应 Game Jam 提交和评奖要求 |

### 7.1 场景组织

**只使用 `Main.unity` 一个场景**，把标题、教学叠层、战斗、过场、胜败做成 Canvas 面板，不要为三个敌人创建三个独立 Scene。

```text
Main Scene
├─ Bootstrap / GameFlowController
├─ WorldBackground
├─ EnemyVisualRoot
├─ Canvas (Screen Space - Overlay)
│  ├─ MainMenuPanel
│  ├─ TutorialOverlay
│  ├─ BattleHUD
│  │  ├─ PlayerHP / Shield / Stage
│  │  ├─ EnemyHP / EnemyName
│  │  ├─ TargetSevenVisual
│  │  ├─ ExpressionSlots (5 slots)
│  │  ├─ NumberHand (3 cards)
│  │  ├─ OperatorButtons (+ − × ÷)
│  │  └─ Undo / Clear / Reroll / Confirm
│  ├─ BetweenBattlesPanel
│  └─ ResultPanel
├─ SfxAudioSource
└─ MusicAudioSource
```

## 8. 代码模块与职责边界

```text
Assets/_LuckySeven/
├─ Scenes/Main.unity
├─ Scripts/
│  ├─ Core/
│  │  ├─ OperatorType.cs
│  │  ├─ ExpressionData.cs
│  │  ├─ ExpressionEvaluator.cs
│  │  ├─ SevenSolver.cs
│  │  ├─ NumberDealer.cs
│  │  └─ BattleRules.cs
│  ├─ Game/
│  │  ├─ GameFlowController.cs
│  │  ├─ BattleController.cs
│  │  ├─ ExpressionBuilder.cs
│  │  └─ BattleSession.cs
│  ├─ UI/
│  │  ├─ BattleHUDView.cs
│  │  ├─ NumberCardView.cs
│  │  ├─ ExpressionSlotView.cs
│  │  ├─ TutorialOverlayView.cs
│  │  └─ ResultPanelView.cs
│  ├─ Presentation/
│  │  ├─ BattleFxPlayer.cs
│  │  └─ AudioController.cs
│  └─ Config/EnemyConfig.cs
├─ Prefabs/NumberCard.prefab / EnemyView.prefab / FX/
├─ Art/Backgrounds/ / Cards/ / Icons/ / Enemies/ / FX/
├─ Audio/SFX/ / BGM/
└─ Tests/EditMode/ExpressionEvaluatorTests.cs
```

**依赖方向（必须遵守）：** `UI → BattleController → Core`；`Core` 不引用 `MonoBehaviour`、`UnityEngine.UI`、`TMP`，也不访问场景物体。Presentation 只能播放结果，不重新计算伤害。

### 8.1 核心数据结构

```csharp
public enum OperatorType { Add, Subtract, Multiply, Divide }

// 完整表达式的值对象；不包含 GameObject 引用。
public struct ExpressionData
{
    public int A;
    public int B;
    public int C;
    public OperatorType First;
    public OperatorType Second;
}

public struct AttackResult
{
    public int Value;
    public int DistanceToSeven;
    public int Damage;
    public int ShieldGain;
    public bool IsLuckySeven;
}
```

`BattleSession` 负责当前玩家 HP/护盾、敌人 HP、关卡编号、回合数、重掷剩余次数、随机种子；这些是纯数值状态。`BattleController` 决定流程，View 只展示。

### 8.2 四则运算纯函数实现

> 下面的代码片段可以直接作为 `ExpressionEvaluator.cs` 的基础。三个数字取值最多 9，不存在此玩法下的整数溢出风险；但如未来加入大数字，需额外检查范围。

```csharp
public static class ExpressionEvaluator
{
    public static bool TryApply(int a, OperatorType op, int b, out int result)
    {
        result = 0;
        switch (op)
        {
            case OperatorType.Add:
                result = a + b;
                return true;
            case OperatorType.Subtract:
                result = a - b;
                return true;
            case OperatorType.Multiply:
                result = a * b;
                return true;
            case OperatorType.Divide:
                if (b == 0 || a % b != 0) return false;
                result = a / b;
                return true;
            default:
                return false;
        }
    }

    // 只支持 ((A op1 B) op2 C)，不支持通用表达式解析。
    public static bool TryEvaluate(ExpressionData expression, out int result)
    {
        result = 0;
        if (!TryApply(expression.A, expression.First, expression.B, out int first))
            return false;
        return TryApply(first, expression.Second, expression.C, out result);
    }
}
```

**不要使用** `DataTable.Compute`、`eval`、动态编译或字符串解析器。固定结构的表达式用纯函数最可靠。

### 8.3 攻击判定

```text
ResolveAttack(expression):
    assert expression is complete
    if not TryEvaluate(expression): reject and keep input phase
    distance = abs(value - 7)
    damage = distance == 0 ? 7 : distance == 1 ? 4 : distance == 2 ? 2 : 1
    shieldGain = (distance == 0) ? 1 : 0
    enemyHp = max(0, enemyHp - damage)
    playerShield = min(3, playerShield + shieldGain)
    play attack animation, update UI
    if enemyHp == 0: end battle (no counterattack)
    else: enemy damage consumes shield first, then player HP
    if playerHp <= 0: defeat
    else: deal next hand and return to input phase
```

### 8.4 游戏状态与防重复点击

```text
GameState: MainMenu → Tutorial → Battle → BetweenBattles → Victory/Defeat
BattlePhase: Deal → PlayerInput → Resolving → EnemyAct → Deal / BattleEnded
```

- `BattlePhase != PlayerInput` 时禁用所有战斗按钮。
- 点击 `Confirm` 的第一帧就把 Phase 切到 `Resolving`，防止连点多次伤害。
- 通过一个受控 Coroutine 串行执行：伤害、反馈、敌人反击、发牌；不得多个协程同时修改 HP。
- `Restart` 必须重置所有临时变量（种子、重掷次数、护盾、HP、关卡、教程状态、音效播放状态）。

### 8.5 最小事件策略

不要为一个小项目搭建通用 EventBus 或消息框架。可使用明确的方法调用与少量 C# 事件，例如：

- `OnHandChanged`：通知手牌刷新。
- `OnExpressionChanged`：通知 UI 刷新槽位与预览。
- `OnAttackResolved`：通知特效与音效。
- `OnBattleStateChanged`：通知 UI 启用/禁用。

绑定时订阅、销毁时取消订阅，避免重复触发。

## 9. 发牌与可解性实现细节

`NumberDealer` 持有一个 `System.Random` 对象，**只在新开局时初始化**，不要每回合重新 `new Random()`。每回合按 `ShouldEnsureSolvable`（建议 70%）决定是否筛选。`SevenSolver` 完全调用 `ExpressionEvaluator`，并只返回“有/无解”，不参与数值结算。

### 必须覆盖的边界情况

- 手牌有重复数：`2,2,3`，按卡牌唯一 `CardId` 标记使用，**不能按数值删除**。
- 先运算得到 `0` 后执行除法：必须禁止除以 0。
- `5 / 2` 不可用浮点近似作为 2.5；直接判定非法。
- 先出现负数再通过加法达到 7：合法。
- 重掷与算式不可同时进行；连点重掷不可超过次数。
- 最后一击击杀敌人时不出现反击动画。
- 发牌生成失败或候选超限：退回预定义可解牌组，不能死循环。

## 10. 测试计划

### 10.1 EditMode 单元测试

| 测试用例 | 预期 |
|---|---|
| `3 × 3 − 2` | 7，合法 |
| `8 − 3 + 2` | 7，合法 |
| `6 ÷ 2 + 4` | 7，合法 |
| `5 ÷ 2 + 3` | 非法（非整除） |
| `1 − 9 + 2` | -6，合法 |
| `2 + 3 × 2` | 10（左结合），合法 |
| 同数值不同卡 ID | 仍可分别使用 |
| 结果为 7 | 7 伤害、+1 护盾 |
| 结果 6 或 8 | 4 伤害 |
| 结果 5 或 9 | 2 伤害 |
| 结果偏差 ≥3 | 1 伤害 |
| 护盾达到 3 后再获 1 | 仍保持 3 |
| 敌人被击杀 | 不反击 |
| 生成器要求可解时 | 测试多种种子，全部可被 Solver 证明有解 |

### 10.2 PlayMode / 人工测试

- [ ] 新玩家不读文档，能在 60 秒内理解“凑 7 然后攻击”。
- [ ] 数字 / 运算符选择、撤销、清空、确认按钮无错位与误触。
- [ ] 3 场关卡可连续通关，死亡可重启；重复重启不产生重复事件。
- [ ] 强制浏览器窗口缩小到 1280×720，关键按钮没有遮挡。
- [ ] 开局没有背景音乐自动播放失败导致游戏异常（浏览器音频需用户手势触发）。
- [ ] 浏览器刷新后无需远程服务即可重新进入游戏。
- [ ] Chrome / Edge 各完成至少一次完整通关。
- [ ] 关闭音量后所有 SFX、BGM 都静音，游戏逻辑照常。

## 11. WebGL 性能与发布

- 不启用实时光照、复杂 Post Processing、大分辨率粒子贴图、重计算 Shader。
- SpriteAtlas 集中 UI 与敌人小图；人物怪物尽量采用小规模分层 Sprite。
- 字体需预烘焙必要字符或配置合适的 Fallback；避免 WebGL 显示中文方块。
- `CanvasScaler` 使用 `Scale With Screen Size`，参考分辨率 `1920 × 1080`，匹配 `0.5` 起步，再实机调节。
- 运行目标：普通桌面浏览器流畅 60 FPS（不以 60 FPS 为必须满足的资格门槛）。
- 构建验证：先用开发构建本地验证错误，再用发布构建压缩上传。
- itch.io 设置：新建项目 → `HTML` → 上传 WebGL 构建 ZIP → 勾选 `This file will be played in the browser` → 选择合适嵌入尺寸 → 在真实 itch 页面打开、通关。
- 发布页面必须写清：玩法、控制、团队与素材来源、AI 使用情况；建议提供主图与 2–3 张截图。
- **严禁仅在 Editor 中测试完成就提交**。正式的 WebGL 构建与 itch 嵌入行为必须单独验证。

---

# 第三部分：艺术与音频设计（ADD）

## 12. 整体艺术定位

### 12.1 一句话视觉风格

**「现代幸运售票机 × 平面编辑设计 × 柔和霓虹数字仪式」**。

游戏看起来像一台制作精良的现代街头交互装置：干净的深色底板、象牙白票据、圆角数字卡、细线网格和黄色幸运 7。画面具有仪式感，但不做厚重奇幻、写实赌场或过度赛博朋克。

**关键词：** `minimal vector / graphic design / tactile ticket / geometric / playful / soft glow / warm editorial / readable numbers`。

**不要：** 高密度纹样、金属盔甲、复杂二次元人物立绘、赌场筹码与现金赌注、密集小字、满屏炫光。

### 12.2 叙事包装

背景设定：玩家面对一台出故障的“命运校准机”，要用算式把紊乱的数字频率校准回 7。每击败一个「厄运模块」，机器恢复一个发光部件。第三关终结巨大七号核心。

叙事只通过简短 UI 文字、环境装置与声音传达，不制作长剧情对话。

## 13. 配色系统与用途

| Token | Hex | 使用位置 |
|---|---|---|
| Ink / 深墨蓝 | `#171D30` | 背景、主要文字深色区域 |
| Paper / 暖纸白 | `#F5F2E9` | 数字票、标题、信息面板 |
| Lucky Gold / 幸运黄 | `#F4C84B` | 数字 7、完美攻击、确认按钮 |
| Calm Teal / 湖蓝绿 | `#55B6B1` | 近 7 提示、选中态、辅助操作 |
| Error Coral / 珊瑚红 | `#ED736B` | 敌人攻击、非法表达式、低 HP |
| Muted Lavender / 灰紫 | `#8C8CAA` | 敌人阴影、次要分割装饰 |

**比例建议：** 深墨蓝 55% / 暖纸白 25% / 湖蓝绿 10% / 幸运黄 8% / 珊瑚红 2%。不要让金色成为满屏常态：金色只为“靠近 7”和“幸运七”发光，才能制造奖励感。

**色彩语义：** 黄色=7/达成；湖绿=可操作/接近；珊瑚=风险；灰白=普通。无障碍信息同时使用数字、形状和文本。

## 14. 字体与数字形态

- 数字与英文标题：优先使用有清晰几何形态的开放授权无衬线字体，例如 **Space Grotesk**（实际嵌入前核实字体授权与字符集）。
- 中文：**Noto Sans SC** 或项目已获得授权的清晰无衬线字体。
- **数字 7 造型**：可专门用矢量路径设计一个几何大“7”，上横线略长、斜笔画大胆，作为品牌核心标识；不直接把受版权保护的标志当作游戏 Logo。
- 关键字号以 1920×1080 为基准：主 7 建议 160–240 px；卡数字 88–120 px；常规按钮字 32–40 px；小提示尽量不低于 26 px。
- 设计原则：**画面首先读到 7，其次读到当前算式，然后才是敌人和装饰。**

## 15. 主战斗界面布局

### 15.1 16:9 草图（概念排布）

```text
┌──────────────────────────────────────────────────────────────┐
│ LUCKY SEVEN       STAGE 02/03          [SFX] [HELP]         │
│                                                              │
│ PLAYER HP ███████████      TARGET                            │
│ SHIELD ◆◆◇                ┌────────┐     [偏差之影 / 敌人]    │
│                           │   7    │     HP ██████          │
│                           └────────┘      几何敌人动画       │
│                                                              │
│                ( [ 8 ]  −  [ 3 ] )  +  [ 2 ]                  │
│                       RESULT: 7  ·  LUCKY!                  │
│                                                              │
│                    [ + ] [ − ] [ × ] [ ÷ ]                    │
│                                                              │
│        ┌──────┐     ┌──────┐     ┌──────┐                     │
│        │  8   │     │  3   │     │  2   │                     │
│        └──────┘     └──────┘     └──────┘                     │
│           [撤销]   [清空]   [重掷 1]       [确认攻击 →]      │
└──────────────────────────────────────────────────────────────┘
```

> 图中为信息结构草图，不是像素精确布局。已经使用的卡仍可在手牌区显示，但需半透明与禁用，避免玩家误以为“原卡消失”。

### 15.2 交互与视觉反馈

- **默认：** 手里 3 张象牙白数字票，7 的目标环微弱呼吸。
- **悬停：** 数字卡抬升约 6–10 px，露出浅金色描边。
- **点击入槽：** 卡片中的数字以 150–220 ms 滑入表达式槽位；手牌变浅灰。
- **预览有效：** 结果数字出现，旁边标注对应伤害，例如 `RESULT 6 → 4 DMG`。
- **预览为 7：** 结果与目标 7 的光圈对齐，出现金色小粒子；尚未攻击时不放完整特效。
- **确认完美：** 中央 7 放大至 115%、快速回弹，金色短促冲击波向怪物传递。
- **非法表达式：** 槽位轻微横向抖动并给出文字（例如 `Division must be exact`），不使用遮住数字的巨型红弹窗。

## 16. 卡牌 / UI / 敌人素材规范

尺寸均为**建议原始工作尺寸**，可根据 Unity Canvas 实际缩放调整。UI 核心尽量直接用 UGUI Image + TMP 生成，避免为每个数字制作单独图片。

| 素材 | 建议尺寸 / 格式 | 数量 | 优先级 |
|---|---|---:|---|
| 标题 Logo（Lucky Seven 字标+几何 7） | 1400×500，透明 PNG / SVG 源 | 1 | P1 |
| 主界面背景（装置与数字几何） | 1920×1080，PNG/JPG | 1 | P0（可先纯色） |
| 数字票底框（空白，不带数字） | 320×460，透明 PNG / 9-slice | 1 | P0 |
| 目标 7 环形底座 | 512×512，透明 PNG | 1 | P1 |
| 运算符按钮底框 | 256×256，透明 PNG / 9-slice | 1 | P0（可用纯色） |
| 功能按钮底框（默认/悬停/禁用） | 420×140，透明 PNG / 9-slice | 3 | P0 |
| 生命/护盾图标 | 128×128，透明 PNG / SVG | 各 1 | P1 |
| 破运骰（常规敌人） | 600×600，透明 PNG | 1 | P1 |
| 偏差之影（常规敌人） | 600×600，透明 PNG | 1 | P1 |
| 失控七号（最终敌人） | 800×800，透明 PNG | 1 | P1 |
| 金色数字粒子、冲击光圈 | 512×512，透明 PNG | 2–3 | P1 |
| 首页及结算装饰线、网格 | 可程序化或 SVG | 1 套 | P2 |
| itch.io 封面图 | 630×500（按页面展示再核对） | 1 | P1 |
| 页面截图 | 16:9 PNG | 2–3 | P1 |

**切图要求：** UI 元素边缘干净；透明 PNG 使用直观可控的留白；敌人图片统一光源（左上）、统一软阴影、统一线宽。不要让 AI 直接画卡牌上的数值和按钮文案——这些全部由 TextMeshPro 输出，避免错字并便于动态显示。

### 16.1 三个敌人的造型语法

1. **破运骰**：骰子主体 + 断裂面的 1–6 圆点 + 暗紫短阴影；轮廓以切角矩形、圆点为主，像坏掉的桌面玩具。
2. **偏差之影**：上下错位的数字片段 + 负片残影 + 断续的青色扫描线；尽量不做复杂人形。
3. **失控七号**：巨大的扭曲 7 型几何核心 + 旋转缺口圆盘 + 少量珊瑚红故障信号；金色恢复时转为完整 7。

三者同样使用矢量感、单一主色、少量高光，只靠轮廓与运动区分。这样一个人也能保证美术统一。

## 17. 动效规范

| 事件 | 建议时长 | 动作 |
|---|---:|---|
| 数字卡 Hover | 0.10 s | 上浮、细描边变亮 |
| 点击填槽 | 0.18 s | 缩放 0.96→1.0，快速进入槽位 |
| 撤销 | 0.12 s | 最近输入项退回，槽位缩小消失 |
| 普通攻击 | 0.40 s | 小型脉冲、伤害数字上飘 |
| 幸运 7 攻击 | 0.80 s | 7 聚光、震屏 0.08 s、金色光圈击中敌人 |
| 敌人反击 | 0.35 s | 敌人前冲 12 px、HP/护盾受击闪烁 |
| 敌人死亡 | 0.65 s | 几何块拆散、渐隐、关卡过渡 |

**限制：** 每帧 UI 动画不过度创建临时对象；屏幕震动幅度小，提供可关闭的开关（P2）；严禁视觉特效遮住数学表达式。

## 18. 音频设计

**音频定位：** 轻巧、机械、节奏明确的“数字售票装置”。倾向电子敲击、玻璃马林巴、轻柔模拟合成器，不用沉重史诗管弦乐。

| 音效 | 声音意象 | 作用 |
|---|---|---|
| `sfx_card_select` | 细纸票轻弹、短促 click | 选数字 |
| `sfx_operator` | 清晰的双音电子滴声 | 选运算符 |
| `sfx_undo` | 轻微反向滑动 | 撤销 |
| `sfx_reroll` | 三个快速拨盘刻度 | 重掷 |
| `sfx_attack_normal` | 短电子冲击 | 非 7 攻击 |
| `sfx_lucky_seven` | 明亮上行音型 + 甜美钟音 | 完美命中 |
| `sfx_damage` | 柔和低频拨动 | 受击 |
| `sfx_victory` | 2–3 秒轻快尾奏 | 通关 |
| `sfx_defeat` | 缓缓下落的三音 | 失败 |
| `bgm_loop` | 80–100 BPM 的轻律动循环 | 战斗环境 |

- `Lucky 7` 音效必须和普通命中一听就不同，且不能刺耳；BGM 音量低于主要反馈。
- 控制 SFX 音量，避免连续按卡时音频堆叠爆音。
- 确保用于作品的音源有明确的游戏发布授权，并在 itch.io 提交页面署名。

## 19. AI 美术生成提示词（可直接改写使用）

所有生成稿应统一套用相同视觉约束：`flat modern vector illustration, warm editorial graphic design, clean geometric silhouettes, bold simple forms, limited color palette, subtle soft shading, consistent top-left lighting, high readability, no typography, no watermark`。

### 19.1 首页背景

```text
A modern mysterious lucky-number calibration machine in a cozy dark navy arcade-like room, giant circular dial centered around the number seven motif (no printed digits), geometric control panels, warm ivory panels, tiny golden light trails, muted teal indicators, playful minimal 2D vector game illustration, editorial graphic design, clean flat shapes with subtle shading, symmetrical yet lively composition, clear empty center-left space reserved for game UI, 16:9, no text, no letters, no watermark, no casino chips, no money
```

> 主体背景尽量不画真正的文字或数字，所有核心的“7”由 UI 矢量组件显示，保证清晰可控。

### 19.2 敌人：破运骰

```text
A cute but slightly menacing broken lucky dice monster, dark indigo cube body with beveled corners, a few circular pips and fractured edges, muted violet shadow, one small coral red eye, geometric minimal vector game character, front-facing, centered, full body, clean silhouette, warm editorial illustration, soft top-left light, limited navy teal coral palette, isolated on fully transparent background, no words, no watermark, not photorealistic, not pixel art
```

### 19.3 敌人：偏差之影

```text
An abstract glitch shadow creature built from fragmented number-like geometric pieces, asymmetric silhouette, offset transparent layers, muted lavender and dark navy body, thin teal signal fragments, playful and mysterious rather than scary, modern minimalist vector game illustration, consistent stroke weight and top-left lighting, character isolated and centered on transparent background, no readable text or digits, no watermark, no pixel art
```

### 19.4 首领：失控七号

```text
An imposing corrupted seven-shaped mechanical core boss, a bold angular diagonal core inside a broken circular ring, dark navy and smoky violet flat shapes, restrained coral red malfunction light, tiny gold cracks hinting at restoration, modern geometric vector 2D game enemy, readable silhouette, symmetrical staging, clean edges, subtle depth, isolated on transparent background, no text, no watermark, no cyberpunk city, no realistic metal
```

### 19.5 UI 装饰套件

```text
Minimal vector game UI asset sheet for a lucky number puzzle battle game: blank ivory ticket card frame, rounded operation button, ring-shaped target medallion, thin geometric separators, tiny starburst and clover-like lucky motifs, simple shield and heart icons, restrained palette of deep navy #171D30, warm ivory #F5F2E9, gold #F4C84B, teal #55B6B1, clean flat 2D vector with subtle soft shading, isolated individual assets on transparent background, no text, no digits, consistent stroke weight
```

**美术验收：** 统一轮廓、留白、光照、透视和颜色；不依赖 AI 一次性生成整张“带 UI 的游戏截图”。游戏 UI 应以 Unity 实际组合为准。

---

# 第四部分：制作排期与交付

## 20. 7 天工作计划（2026-11-06 起）

| 时间 | 交付物 | 不完成就不进入下一阶段的验收点 |
|---|---|---|
| **Day 1 · 11/06** | Core 数学逻辑、表达式 UI 原型、一次完整攻击 | 用 3 张卡能合法凑 7，并造成伤害 |
| **Day 2 · 11/07** | 回合/敌人/HP/护盾/重掷、开始和失败 | 一场战斗可以胜、败、重开 |
| **Day 3 · 11/08** | 三关流程、教程、数据配置、随机可解生成 | 3 场连续战斗可通关，流程无阻断 |
| **Day 4 · 11/09** | 主界面视觉重构、卡片、敌人、统一配色 | 截图具有明确视觉风格，数字和结果可读 |
| **Day 5 · 11/10** | 动效、音效、BGM、胜利演出、平衡 | 10 分钟内完整玩完；幸运 7 很有“命中感” |
| **Day 6 · 11/11** | WebGL 上传初版、跨浏览器实测、Bug 修复 | itch.io 页面可直接打开并通关 |
| **Day 7 · 11/12** | 新手测试、打磨手感、冻结功能、正式构建 | 新人 60 秒内理解规则，关键 Bug 为 0 |
| **缓冲 · 11/13–14** | 提交页面、截图、Credits、AI 声明、最终验证 | 截止前成功提交、外部设备复测、链接可访问 |

> **硬截止：** 2026-11-14 **12:00 UTC / 北京时间 20:00 / 日本时间 21:00**。官方建议七天后尽早结束开发，把额外时间用于构建与提交，而不是继续加功能。若版本完成较早，应尽早提交，再在截止前按规则更新。

## 21. 优先级 / 砍功能原则

### P0：必须完成（缺一则不提交）

- [ ] 算式构造、精确整数运算、撤销与非法输入反馈
- [ ] 7 命中判定、伤害结算、敌人反击
- [ ] 三场或至少一场可完整战斗的体验闭环
- [ ] 开始、失败、胜利、重新开始
- [ ] 鼠标操作直观、页面正常缩放
- [ ] itch.io 浏览器 WebGL 可运行

### P1：强烈建议完成（评分感知最明显）

- [ ] 固定教学牌组 + 新手提示
- [ ] 三种敌人及统一风格背景
- [ ] 幸运 7 专属音效和特效
- [ ] 一次重掷、可解牌组生成
- [ ] 清楚的配色、数字、按钮视觉层级
- [ ] itch 页面封面 / 操作说明 / Credits

### P2：如果还有时间

- [ ] 键盘快捷键、可关闭震动
- [ ] 更丰富的敌人表情及动效
- [ ] 最短回合通关、完美命中次数统计
- [ ] 稍微丰富的结算文字

### 红线

如果 Day 3 仍不能完整通关：**立即停止增加美术素材和新玩法，只修主循环**。  
如果 Day 5 仍没有 itch WebGL 测试版本：**立即停止一切非必要功能，先做 WebGL**。  
如果 Day 6 出现重大打包问题：**删掉造成问题的依赖或效果，不更换整个项目架构**。

## 22. 提交清单（官方规则映射）

- [ ] 原创游戏，工程和玩法实现在 Game Jam 开始后制作。
- [ ] 使用 Unity 引擎，提交在官方 itch.io Jam 页面。
- [ ] 至少一个能在 itch.io **浏览器内运行的 WebGL 构建**（参与官方奖项的重要条件）。
- [ ] 项目页面写清楚：短描述、操作方式、团队成员、素材及工具 Credits。
- [ ] 如果使用生成式 AI：在提交页面**明确披露用途**，例如代码生成辅助、视觉素材草图、音效制作等；不要省略。
- [ ] 所有需要的游戏文件都已包含在提交构建中，不依赖外部下载。
- [ ] 游戏关键操作在 itch 页面能正常完成。
- [ ] 截止前提交，评审期不修改游戏文件；网页描述和截图以赛事允许范围为准。

**官方信息：** https://itch.io/jam/official-unity-game-jam-2026  
**官方公告（赛程与 WebGL）：** https://discussions.unity.com/t/the-2026-official-unity-game-jam-starts-november-5th/1740103

## 23. itch.io 页面说明文案草稿

> **LUCKY SEVEN** — Three numbers. Two operations. One lucky number.  
> Build a left-to-right equation from three number cards, using +, −, ×, and ÷. Hit **7** to unleash a lucky strike and gain protection. Miss? You still fight back. Face three glitches of fate and recalibrate your luck!  
> **Controls:** Click number cards and operations in sequence. Use Undo/Clear to revise your equation. Click Confirm to attack. You may reroll one card once per battle, before building an equation.  
> **Credits:** [developer name, game art, music and SFX sources to be filled in]  
> **Generative AI disclosure:** [describe actual AI-assisted coding, art or audio processes here; do not copy untrue statements]

## 24. 交给 AI 编程工具的首轮任务拆分

1. **Core 任务：** “在 `Scripts/Core` 实现纯 C# 算式结构、左结合求值、整除判定与 7 的伤害结算，不引用 UI；编写 EditMode 测试。”
2. **UI 任务：** “在一个 Unity 场景中创建三张数字卡、四个运算符按钮、五个表达式槽位和确认/撤销/清空功能，仅通过 Controller 修改 Core 状态；不要实现拖拽。”
3. **Combat 任务：** “实现 3 场战斗的有限状态机、HP/护盾、一次重掷、胜败流程；攻击判定只调用纯逻辑。”
4. **Dealer 任务：** “用固定随机种子与 96 组合求解器，实现首回合固定牌组与后续可解概率抽样；添加极端情形回退机制。”
5. **Presentation 任务：** “在现有流程中增加 7 命中特效与音效、敌人受击和场景过渡；只响应事件，不改变伤害判定。”
6. **WebGL 任务：** “检查构建平台、字体、浏览器音频、输入和界面缩放问题，确保无服务器依赖；输出可上传 itch.io 的 WebGL 构建及运行检查说明。”

**给 AI 的共同约束：** 不添加未经要求的系统；不引入不必要第三方依赖；代码易于单元测试；先实现已定义规则；不要改动其他脚本的公开行为；每次完成说明改动文件、测试方式和遗留问题。

---

## 25. 最终验收标准（Definition of Done）

| 类别 | 验收条件 |
|---|---|
| 玩法可读性 | 玩家能解释“3 数字 + 2 运算 → 接近 7”的目标 |
| 规则可靠性 | 左结合、整除、重复牌、护盾、击杀结算等测试通过 |
| 完整性 | 开始 → 教程 → 3 场战斗 → 胜利/失败 → 重开完整可用 |
| 主题关联 | 去掉数字 7，核心玩法即失去目标和意义 |
| 表现统一 | 卡牌、数字、敌人、粒子与声音使用同一套设计语言 |
| 易用性 | 算式的下一步点击目标明确；非法输入说明清楚 |
| 性能与兼容 | WebGL 在至少两个常见桌面浏览器通过完整试玩 |
| 可提交性 | itch 页面可玩、有简介、操作、署名、AI 使用声明 |
| 进度控制 | 功能冻结后不新增高风险模块，留足上传缓冲 |

### 开发决策记录（后续可以持续更新）

| 日期 | 决策 | 原因 |
|---|---|---|
| 2026-11-06 | 选择 `((A op1 B) op2 C)` 左结合表达式 | 消除运算优先级歧义，降低 UI 与解析成本 |
| 2026-11-06 | 一屏 2D、三场战斗、无装备系统 | 保证七天内能发布 |
| 2026-11-06 | 选择现代幸运装置平面视觉风格 | 数字辨识度优先，容易保持统一 |
| 2026-11-06 | 数字计算与展示分离 | 快速测试逻辑，方便迭代演出 |

---

**文档结语：** 不需要做“玩法最多”的 Game Jam 游戏；做一款玩家在第一次凑出 **7** 时会微笑、在最后一击时愿意再来一次的小游戏，就已经把赛题变成了游戏本身。
