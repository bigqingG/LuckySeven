# 中文与扑克牌界面

默认语言为简体中文。右上角语言按钮切换中文/英文，使用 Unity Localization 的 SelectedLocale 作为唯一语言状态，并通过 PlayerPrefs 保存选择。切换期间短暂锁定输入，完成异步文字和字体加载后恢复；战斗数据和已选卡牌保持不变。

- 文案源：`Assets/_LuckySeven/Localization/GameText.json`，63 项双语条目。
- Unity String Tables：GameText（zh-Hans/en）；Asset Tables：GameAssets，UIFont 按语言表加载；两种语言共用支持中英文的 Noto 字库，消除切换瞬间的缺字。
- 静态文本通过 LocalizeStringEvent 绑定；带生命、回合、伤害等参数的文本通过 L10n 格式化。
- 中文字体：Google Fonts 的 Noto Sans SC，生成 450 字重静态 TTF，再生成动态、多图集 TMP 字体；随 Web 包提供，不依赖玩家电脑字体。原始授权文件保存在字体目录与 StreamingAssets/ThirdPartyNotices。
- 字体上游：[Noto Sans SC](https://github.com/google/fonts/tree/main/ofl/notosanssc)；[OFL 授权](https://raw.githubusercontent.com/google/fonts/main/ofl/notosanssc/OFL.txt)。
- Web 加载页面、浏览器标题和无障碍名称跟随语言选择；加载失败提示也有中英文版本。

卡牌由 uGUI 矢量图形绘制：米白纸面、圆角边框、红黑四花色、按数字排列的花色点数、正反角标，以及底部强攻/守护/回响说明。花色仅用于外观，不改变战斗数值。

鼠标悬停时牌面上移 35 个参考像素、放大到 1.14 倍，并显示到其他手牌前方。底部点击条保持原位置，避免放大的牌面阻挡相邻手牌；抬起的上半部分也可点击和拖拽。移开鼠标后恢复位置和层级。点击/拖拽出牌、点击/拖回手牌的规则继续保留。

本次备份：`Backups/BeforeLocalization`。验证与视频：`Docs/Validation/LocalizedCards`。新版 Web 输出仍为 `Builds/WebGLV2`；原测试包保留在先前 ZIP 文件中。

## 2026-11-07：复古像素冒险换肤

用户选定复古像素冒险并授权接入。当前牌面使用阶梯边框、像素中央数值、花色角标和图标/文字附效标签，替代前述圆角点数排布；本地化仍沿用同一机制，现有69项中英双语条目。底部手牌间距加宽，悬停上移35参考像素与1.14倍缩放保持不变。详情见 `Docs/PixelAdventureIntegration.md`，当前验证见 `Docs/Validation/PixelAdventure`，接入前备份为 `Backups/BeforePixelAdventure20261107`。

## 2026-11-07：资源与组合 V1

补牌调整为抽2或换2抽3，组合奖励与预计伤害采用同一套中英本地化。当前75项双语条目；玩法说明已同步，具体规则见 Docs/ResourceComboV1.md，验证见 Docs/Validation/ResourceComboV1。
