# 幸运七：四套战斗界面生成提示词

日期：2026-11-07。使用内置 image_gen 工具，各方案独立生成。仅为视觉探索，不是 Unity 实机截图或已切分的可用 UI 素材。

参考阅读：Docs/CardRulesV2.md、Docs/LocalizationAndPokerCards.md，以及 Docs/Validation/LocalizedCards/Browser/04-poker-hand.png 和 05-hover-2.png。截图用于理解结构，生成时通过以下统一文字规格固定内容。

## 共用提示词

```text
Use case: ui-mockup.
Create ONE complete polished Unity Game Jam web card battle interface concept for the Chinese game 幸运七, true landscape 16:9, ideally 2048x1152. Full screen artwork, no browser chrome, no external caption, no multi-panel collage. This is a practical 2D uGUI mockup.
All Chinese text must be correct, crisp, high contrast and easily legible, use a clean Chinese sans-serif font; decorative font only for title. Generous type sizes, uncluttered visual hierarchy.
LOCKED SHARED LAYOUT for a set of 4 visual comparisons:
Top slim header at y3-9%: title 幸运七 left; center 第1/3场 · 第1回合; right a language button 中文 / EN and a sound button 音效：开.
Upper combat band y12-36%: left player panel x4-30% title 命运掌牌人, text 生命 30/30 and 格挡 0 and a full health bar. Center x37-60% title 幸运数字 above two large distinct circular medallions 7 and 2 separated by 或; below small text 牌面数值相加，凑成任一目标. Right x67-96% enemy panel with an illustrated dice-creature enemy on its LEFT and readable stats on its RIGHT: name 残缺之骰; 生命 18/18; 护甲 3; 反击 2; full health bar. Enemy is a single cracked six-sided dice creature, no additional enemy.
Middle y40-62%: centered heading 请选择要出的牌, wide EMPTY outlined card playing area x17-83%, text 出牌区 then 点击或拖入手牌 · 点击中央卡牌收回. No selected card, no damage or attack effects, no hover animation.
Action row y64-70%: buttons 收回 / 返回, 普通攻击, 确认破防. All disabled/subdued because no cards selected, labels still readable.
Bottom hand y74-96%: exactly EIGHT upright slightly overlapping playing cards centered x22-78%. Their top-left value and suit, large central value, and effect label must ALL be visible, left to right:
2♠ 守护; 5♥ 强攻; 9♦ 回响; 1♥ 回响; 3♠ 回响; 5♠ 回响; 4♣ 守护; 7♠ 强攻.
Make cards broad, highly readable, overlap minimally, no lifted card. Suit decoration only, never change numbers. Use large central numeral with a few decorative suit symbols instead of complicated pip counting. Every card must show its own effect label clearly on a visible non-overlapped area. Do not add new card titles or mechanics.
Bottom-left x3-19% a compact draw-controls panel: 手牌 8/9; 牌库 19 · 弃牌 0; two distinct clearly labeled buttons 抽3张 and 换2抽4; buttons disabled and caption 本回合已补牌. Small top-of-hand hint 保留一张手牌用于攻击. Bottom-right x82-97% compact effect legend: 强攻：伤害+2 / 守护：格挡+3 / 回响：下次抽牌+1. Effects are only active on attack; small heading 攻击附效. Avoid extra lore, tiny paragraphs, placeholder text, unreadable ornament, card-collecting menus, gameplay changes.
All artwork should feel implementable with one static background, one enemy sprite, shared card frames, three icons, 9-sliced panels and simple UI tweens. No expensive volumetric lighting or 3D scene requirements.
STYLE SPECIFICATION:

```

## 复古幸运牌桌

文件：01-retro-lucky-table.png

```text
Retro lucky card table: deep forest green #123D32, brass gold #B99752, ivory #F4EAD2. A richly illustrated but restrained felt gaming table, dark walnut edge, subtle stitched felt border, thin brass rules and round brass lucky-number tokens. Enemy is a hand-painted antique ivory dice creature with a chipped corner and little brass limbs, charming and mysterious. Ivory paper poker cards with engraved double line borders, red/black suits, understated gold badge effect labels. Rectangular beveled ivory/brass buttons, subtle letterpress texture, refined vintage board game visual identity. Warm flat lighting; no ornate fantasy architecture. Player and enemy panels resemble tasteful inset brass nameplates. Highest readability.
```

## 神秘命运塔罗

文件：02-mystic-fate-tarot.png

```text
Mystic fate tarot: deep purple #281B42, midnight blue #111C35, aged dark gold #AA8850, text parchment #F0E5CF. Hand-illustrated celestial divination table with sparse star-map markings and flowing ink-like midnight shapes; no dense busy imagery behind text. Arched panel frames, delicate gold constellation lines, crescent motifs and suspended medallions for lucky numbers. Enemy is the SAME cracked dice creature rendered as a floating midnight indigo stone die, gold crack and a small smoky cloak, visually strong but contained in its panel. Cards use light parchment faces for readability, narrow tarot arch borders, stylized engraved suit icons, original given numbers and effects unchanged. Buttons shaped as restrained elongated gold-edged ritual plaques. Distinct editorial tarot illustration style, not a recolored casino screen. Static painted background, no actual particle fog needed.
```

## 明亮卡通桌游

文件：03-bright-cartoon-boardgame.png

```text
Bright cartoon tabletop board game: cream #FFF5E4, lake blue #309CB6, coral orange #F27862, dark ink #233F4A. Cheerful flat illustrated board-game surface, generous cream negative space, small playful paper-cut motifs confined to margins. Thick friendly rounded outlines, big pill-shaped buttons and chunky tokens, offset sticker-style shadows with no real lighting. Lucky numbers on blue and orange round cardboard tokens. Enemy is the SAME cracked dice creature in cute expressive cartoon style: white rounded die, tiny blue feet, a coral bandage over a chipped corner, mildly mischievous eyebrows. Cards have white/cream faces, bold dark numbers and red suits, thick rounded frames, simple colored effect badges, all 8 values readable. Player/enemy UI are rounded sticker panels. Friendly, clean, approachable, strong graphic shape differences from vintage/tarot designs; easy to implement with flat sprites.
```

## 霓虹街机牌局

文件：04-neon-arcade-card-game.png

```text
Neon arcade card game: near-black #090D17, electric cyan #25DCEB, magenta #EF438F, pale ice text #EDF9FF. Crisp arcade vector illustration, restrained perspective grid only in background margins, angular chamfered UI panels, thin cyan/magenta luminous edges with baked-in modest glow. No blur over text, no intense bloom, no sci-fi data clutter. Lucky numbers on chamfered arcade tokens, cyan 7 and pink 2. Enemy is the SAME cracked dice creature redesigned as a small outlined black/chrome arcade dice robot, cyan face pips and one magenta cracked corner, confident angular silhouette. Cards have dark navy faces with LARGE pale numerals, cyan spade/club symbols and magenta heart/diamond symbols, thin bright angular frames, explicit Chinese effect badges. Hard-edged arcade button shapes, strong graphic composition. Keep readable card faces, no holographic transparency, all visual glow should be achievable by prepainted sprites without postprocessing.
```

