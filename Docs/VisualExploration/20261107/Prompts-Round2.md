# 幸运七：第二轮视觉探索提示词

日期：2026-11-07。使用内置 image_gen 工具，各方向独立生成。图片仅用于视觉比较。

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

## 东方水墨牌局

文件：05-oriental-ink.png

```text
Oriental Chinese ink painting card-table art direction. Rice-paper ivory #F1E9D5, charcoal ink #262B2A, cinnabar seal red #B74434, muted celadon #7A9C8B. Strong negative space. Background is pale rice paper with very faint mountain ink washes ONLY at far side margins. Thin broken brushstroke panel outlines, restrained square seal accents, lucky numbers on two simple ink-ring seals. Title 幸运七 is expressive black brush calligraphy, but ALL gameplay Chinese is exceptionally crisp printed sans-serif. Enemy is the same cracked dice creature illustrated as a squat weathered inkstone dice spirit with a chipped corner, red pips, tiny brushstroke feet and a small cinnabar paper talisman, contained beside enemy stats. No added enemy or lore. Hand cards remain rectangular poker cards, pale rice-paper faces, LARGE clean Arabic numbers, exact suits and effects as specified; seals for effect labels. Buttons are quiet rectangular pale-ink slabs, disabled states still readable. Do not turn cards into Mahjong tiles or Chinese number characters. This must feel like a restrained contemporary Chinese ink game, not a gold fantasy casino. Static painting, no 3D or real ink simulation.
```

## 剪纸童话剧场

文件：06-paper-cut-storybook.png

```text
Layered hand-cut paper storybook theatre art direction, visibly tactile fibrous paper and cut edge shadows. Oat paper #EBD9B7, dusty terracotta #BE6E51, sage #728E75, dark plum ink #443749. Desaturated autumnal colors, no bright cyan/coral sticker style. A shallow miniature paper theatre stage framing the interface, scalloped kraft-paper status plaques attached by illustrated paper tabs, two hanging layered cardboard lucky-number discs. Background features quiet cut-paper leaf silhouettes around the extreme margins, empty central area made of flat parchment with an inset stitched paper boundary. Enemy is the same cracked dice creature built from folded cream paper with exposed torn corner, printed pips and accordion paper legs; an appealing handmade silhouette. Hand cards use pale layered cardstock with clipped corners, dark large numerals and terracotta hearts/diamonds, sage/ink spades/clubs; attached paper effect labels, precise given data. Buttons resemble matte paper tabs with subtle PREPAINTED offset shadows. This is a sophisticated handmade collage storybook, not a flat cartoon outlined sticker screen. No actual complex 3D, no extra characters, text must stay crisp and high contrast.
```

## 复古像素冒险

文件：07-retro-pixel-rpg.png

```text
Authentic 16-bit pixel-art card RPG interface, not neon arcade. Deep navy #222B45, desaturated slate #586481, warm ivory #F2E5BC, amber #D7A647, limited brick red accents. Cohesive clean nearest-neighbor stepped pixel edges, chunky rectangular pixel borders and small palette, no blur, no smooth gradients, no glow. Very subdued tiled stone game-board backdrop, small pixel ornamental corner sprites. Enemy is the same cracked dice creature rendered as a memorable chunky pixel dice golem with amber eyes, one chipped corner and stubby feet. Lucky numbers on two chunky pixel coin tokens. Poker cards have warm cream faces with large clear dark pixel Arabic numerals, red hearts/diamonds, black spades/clubs, rectilinear pixel frame and attached effect label strip. EXACTLY 8 cards with original values/effects. Full UI uses clean readable modern Chinese bitmap-like lettering at sufficiently large size; do not sacrifice Chinese readability to tiny pixel fonts. All panels, buttons and health bars use deliberate consistent stepped pixel design. Still top HUD, center empty play area, bottom overlapping hand. Simulate an expertly polished pixel indie game screenshot; no elaborate dungeon scene or unrelated RPG equipment.
```

## 极简几何牌桌

文件：08-minimal-geometric.png

```text
Premium minimalist geometric editorial card-game interface. Warm off-white #F5F2EB, charcoal #23272C, cobalt #355BD7, vermilion #D75B43. Swiss graphic-design clarity and playful abstract board-game geometry. Flat matte surfaces, no gradients, textures, glow, ornaments, shadows or thick cartoon outlines. Strict whitespace, fine one-pixel rules, large highly legible Arabic numerals, restrained bold Chinese sans-serif hierarchy. Enemy is the same cracked die represented by a striking flat geometric isometric white cube with charcoal outline, cobalt pips, one small vermilion cracked triangular corner and two simple rectangular feet. Upper HUD separated into clean grid blocks; lucky numbers on one cobalt and one vermilion flat circle. Center play area a clean pale gray inset rectangle with a fine dashed boundary, all original labels. Cards white with crisp rectangular fine black frames, single large centered numeral and one understated suit, tiny colored rectangular effect tags with clear Chinese text; all 8 original values/effects and slight hand overlap preserved. Buttons are clean squared rectangular blocks; disabled controls muted gray but readable. Background only a few oversized pale geometric shapes confined to edges. Elegant, calm, distinctive playable tabletop game interface, NOT a dashboard, not a corporate slide. Very low asset count, all components plausible flat UI sprites.
```

