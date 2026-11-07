# 像素角色图集生成记录

2026-11-07，内置 image_gen。透明背景，使用已选方案 Docs/VisualExploration/20261107/07-retro-pixel-rpg.png 作为风格参考。

输出：Assets/_LuckySeven/Art/PixelAdventure/Characters.png。

Unity TextureImporter 切为 Hero / Die / Shadow / Rogue 四张精灵，没有通过外部图像处理改变图集。UI背景、边框、图标、牌面数值由项目内像素几何绘制。

## 完整提示词

```text
Use the attached selected game screenshot ONLY as art style reference. Generate a PRODUCTION CHARACTER SPRITE ATLAS with genuine transparent background, square canvas, EXACTLY FOUR sprites in a precise 2x2 equal grid. Each sprite centered in its own square quadrant with at least 12% empty margin from quadrant boundaries. No text, letters, numbers, labels, frames, panels, ground platforms, UI, gridlines, background or drop shadows. Fully isolated transparent cutouts, consistent chunky 16-bit pixel art, limited navy, ivory, amber, brick-red palette, hard stepped edges with pixel clusters, no blur. Top LEFT: the small friendly card-playing mage from reference, full body, navy/brick red hat and robe, ivory face, holding one tiny ivory playing card; quarter turned facing right. Top RIGHT: the cracked ivory dice golem from reference, full body, amber eyes, black dice pips, navy stone feet, chipped top corner, facing left. Bottom LEFT: a floating 'deviation shadow' enemy, a compact indigo hooded shadow creature with two amber eyes and two angular asymmetric ivory shoulder shards, no legs, facing left, similarly sized and readable. Bottom RIGHT: 'rogue seven' final enemy, a compact regal ivory dice knight with a brick-red cape and small angular amber crown, black pips and fierce amber eyes, facing left; no printed numeral. Each sprite takes 70-76% of its own quadrant height and width. All four should be equally visually weighted. Keep material shading simple in 3-4 flat tones. This atlas will be sliced into four independent sprites in Unity; no element may cross its own quadrant. Sprite artwork only, no user-interface screenshot.
```

