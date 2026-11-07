# 幸运七 / Lucky Seven

Unity Game Jam 卡牌战斗游戏。将任意数量的牌面数字相加，凑成7或本场第二幸运数来破防，再另选一张手牌攻击，击败三场敌人。采用复古像素冒险风格，默认中文，可切换英文。

- Unity：**6000.3.7f1**；URP + uGUI / TextMeshPro + Input System。
- 入口：`Assets/_LuckySeven/Scenes/Main.unity`，或菜单 `Lucky Seven > Open Main Scene`。
- 配置：`Assets/_LuckySeven/Config/GameConfig.asset`。
- Web：`Lucky Seven > Build Web Prototype`；构建后运行 `./Tools/Serve-Web.ps1`。
- 测试：`Lucky Seven > Run EditMode Tests` / `Run PlayMode Tests`。

操作、架构、构建说明见 [基础框架说明](Docs/Framework.md)。验证记录位于 `Docs/Validation`。
版本管理与新电脑恢复见 [Git 使用说明](Docs/GitWorkflow.md)，图片、字体和录屏使用 Git LFS。
当前规则以 [卡牌规则V2](Docs/CardRulesV2.md) 为准；视觉接入见 [复古像素冒险说明](Docs/PixelAdventureIntegration.md)。Web输出为 Builds/WebGLV2，本地验收不代表已发布到Game Jam平台。
