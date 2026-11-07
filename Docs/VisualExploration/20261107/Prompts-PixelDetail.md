# 幸运七：复古像素冒险深化提示词

2026-11-07。使用内置 image_gen，以 07-retro-pixel-rpg.png 为风格/布局参考。09与11为设计展示板，10为悬停状态效果图。均为视觉概念，未实现。

## 像素卡牌与附效

输出：09-pixel-card-system.png

```text
Use case: ui-mockup. Using the attached selected 幸运七 pixel-adventure screenshot as the STYLE REFERENCE, create a polished 16:9 horizontal CARD DESIGN PRESENTATION BOARD, not a full battle screen. Preserve its navy/slate background, ivory cards, amber pixel borders, brick-red accents, 16-bit sprites and clear Chinese. Clean large typography, consistent deliberately stepped pixel edges; no blur, neon, photoreal materials or added mechanics.
Title at top: 幸运七 · 像素卡牌. Small subtitle: 数字优先，附效清晰.
Across the center show FOUR separate large vertical cards, with roomy equal gaps and NO overlap:
Card1 2♠, LARGE central 2, dark suit, a blue/slate shield badge labeled 守护, exact small text 格挡+3.
Card2 5♥, LARGE central 5, red suit, brick-red sword badge labeled 强攻, exact small text 伤害+2.
Card3 9♦, LARGE central 9, red diamond, muted teal/blue circular-arrow badge labeled 回响, exact small text 下次抽牌+1.
Card4 is the shared CARD BACK: dark navy, amber stepped geometric border and one large stylized gold 7 emblem, no number/suit corner indices or effect. Label above this fourth card 通用牌背. Label above first three 攻击附效. Fronts share ONE identical ivory frame. Distinguish effects by BOTH icon and text, not color alone. Poker suit and numeral take priority, never invent character art on cards or a mana/energy cost. Top-left number and suit must be very clear.
Bottom band presents THREE SMALL horizontal example UI chips, text 常态 / 悬停 / 待确认, showing a neutral ivory border, bright amber focused border with tiny upward arrow, and amber corner brackets respectively. These are design state samples, not new cards. Include explicit concise footer: 附效仅在攻击时触发；凑数、换牌、弃牌不触发.
This is a practical reusable card skin sheet with minimal pixel ornaments. Do not add tiny explanatory prose. Treat image as a design reference only; actual implementation uses dynamic text.
```

## 像素手牌悬停

输出：10-pixel-hover-state.png

```text
Use case: ui-mockup. EDIT the attached selected pixel-adventure battle screenshot into its HOVER STATE. Keep its full 16:9 layout, enemy and player art, all combat data, font readability, colors, card sequence and overall art direction. Preserve title 幸运七, 第1/3场 · 第1回合, 中文 / EN, 音效：开; player 命运掌牌人 生命30/30 格挡0; enemy 残缺之骰 生命18/18 护甲3 反击2; lucky numbers7或2. Keep the EMPTY central play area, no selected cards, no shield break, no attack, no changed counters.
Bottom hand consists of exactly EIGHT cards left to right 2♠守护,5♥强攻,9♦回响,1♥回响,3♠回响,5♠回响,4♣守护,7♠强攻.
The ONLY hovered card is THIRD card 9♦回响: lift it moderately above the hand (about35 pixels at1280x720 reference scale), scale about1.14x relative to other cards, render it on top of adjacent card edges, highlight its stepped pixel border in amber. It should remain near the third slot and not fly into central play zone. No duplicate9card at its original slot. All seven other cards remain at their original hand baseline; their top-left numbers must remain readable. Add a small pixel mouse pointer over the raised9card; keep numeral9 and label回响 visible. Show on the enlarged card clearly 回响 and 下次抽牌+1, with a very small loop arrow icon. No large tooltip covering battle actions.
Keep left counter 手牌8/9,牌库19·弃牌0,disabled 抽3张 and 换2抽4,本回合已补牌. Keep action row disabled 收回/返回,普通攻击,确认破防, since hover does not select anything. Right legend unchanged: 强攻伤害+2,守护格挡+3,回响下次抽牌+1,仅攻击时生效.
Maintain sufficient vertical gap so raised card does not cover the action buttons. This is an actual-looking full game UI preview, with no external design annotations, no exploded views. Retain crisp blocky pixel art and legible Chinese text.
```

## 像素破防与攻击分镜

输出：11-pixel-break-attack-sequence.png

```text
Use case: ui-mockup / visual storyboard. Use attached selected pixel-adventure 幸运七 screenshot ONLY as consistent ART STYLE and character reference. Create ONE 16:9 horizontal polished THREE-PANEL storyboard, ordered left to right, showing EXACT battle mechanics in large readable Chinese. Navy stone background, amber stepped pixel frames, ivory poker cards, brick red damage and clean Chinese typography. Same cracked dice enemy recognizable in all 3 panels. Top title 幸运七 · 破防与攻击. Under title a clear rule subtitle 先凑数破防，再另选一张手牌攻击. Three equally sized wide panels with large numerals, arrows between panels. This is a detail storyboard, NOT three complete miniature HUD screens; don't squeeze in full hands or unrelated menus.
LEFT PANEL heading ① 凑数待确认. At top a small dice enemy and healthy bar labeled 生命18/18; intact shield emblem labeled 护甲3. Middle show two ivory poker cards 2♠守护 and5♥强攻 with a plus sign between; below giant equation2+5=7 and a gold7target token. A clearly enabled amber button 确认破防. Bottom caption 凑数牌不触发附效. No damage numbers, no guardian block buff, no attack bonus here.
MIDDLE PANEL heading ② 破防成功. Enemy still has FULL health bar and text 生命18/18. Display a separate SHIELD EMBLEM cracking into 5 or6 chunky pixel fragments with a short amber-white burst; the ENEMY BODY remains intact. Label 已破防. Text 选择一张攻击牌. Show ONLY ONE ivory poker card7♠强攻 as the newly selected attack card, with a small sword icon. Enabled button 攻击. Below caption 破防不扣血 · 本次攻击无视护甲. Do not depict the previous2and5 cards as still available. Do not claim a stun or skip retaliation.
RIGHT PANEL heading ③ 攻击命中. Same dice enemy with short white silhouette flash and one diagonal chunky pixel slash, moderately large floating red -9. A HALF-full health bar with exact text 生命9/18. Clear calculation7+2=9 below enemy, and short label 强攻伤害+2. Small single7♠card icon flying toward enemy may be included but no additional damage or cards. Caption 敌人存活，随后反击2点. This is the instant BEFORE retaliation; don't claim enemy defeated.
Bottom whole-board footer 下一回合护甲恢复. Restrained small sprite-like effects that could be implemented by a few frames and UI transforms; no fullscreen bloom, particle storm, new status conditions, or 3D effects. Make all card values, equations and Chinese labels accurate and legible.
```

## 悬停稿最终修订

以首次生成的悬停图为编辑目标，最终交付采用本次修订结果。

```text
Edit the attached full pixel-art 幸运七 battle UI screenshot with one targeted hover/readability refinement. Preserve the whole 16:9 scene, layout, all Chinese labels, all combat values, all card identities and counts, enemy, player, and art style exactly.
Only adjust the hand's hover treatment and effect badge consistency:
The third card 9♦回响 must visibly float higher: position its top about 50 image pixels ABOVE the top of the other seven cards, with a clear lifted bottom edge as well. Its width should be only about 1.14 times the width of a normal card. Keep it frontmost, near third slot, with crisp amber pixel border. Move the small cursor with it. Keep its text 9, 回响, 下次抽牌+1 and small loop-arrow icon fully readable. Do not overlap the three action buttons. The instruction 保留一张手牌用于攻击 may be shifted slightly upward into the free gap to avoid the raised card, but preserve all other layout.
All FOUR 回响 badges (cards9♦,1♥,3♠,5♠) should use muted teal/slate-blue fill, visually matching a loop-arrow effect identity; labels remain ivory. Guard badges on2♠and4♣ remain dark slate; strike badges on5♥and7♠ remain brick-red. No other card is lifted or resized. All eight top-left values remain visible:2,5,9,1,3,5,4,7. No duplicated card under hovered9, no cards in central play area. Preserve EXACT battle stats and disabled actions since hovering doesn't select cards. No new design annotations or new mechanics.
```

