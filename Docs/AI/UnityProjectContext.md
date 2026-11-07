# Lucky Seven — project context

Analyzed 2026-11-06. Root: `E:/MyUnityProjects/Lucky Seven`. No Git repository at baseline.

Updated 2026-11-07: the user selected retro pixel adventure and authorized implementation. The Main scene now uses the pixel skin (PixelGraphic, BattleHUDView.Pixel, PixelBattleFx), a generated four-character atlas, wider overlapping hands and persistent disabled supply controls. Core rules/config are unchanged. See Docs/PixelAdventureIntegration.md and Docs/Validation/PixelAdventure for current evidence; Backups/BeforePixelAdventure20261107 preserves the prior project files. The historical implementation notes below describe earlier stages.

## Confirmed baseline

- Unity 6000.3.7f1 (696ec25a53d1), installed at `D:/6000.3.7f1/Editor/Unity.exe`.
- Installed WebGLSupport includes Emscripten; Windows player module also present.
- Fresh Universal 3D template: URP 17.3.0, Input System 1.18.0 (activeInputHandler 1), uGUI 2.0.0 with TMP, Test Framework 1.6.0.
- Only SampleScene and template readme scripts existed. No game assemblies, networking usage, tests or local guidance. No Unity MCP bridge.
- Original settings snapshots are in `Docs/Validation/*.before.txt`. Original SampleScene is preserved.
- Editor baseline contained a Unity cloud project-link HTTP 403 for the template's undefined project ID; unrelated to offline gameplay.

## Implementation contract

Current contract: `Docs/CardRulesV2.md`, implementing the user's later card-design decision. Original `Docs/Lucky_Seven_GameJam_设计文档.md` is retained as historical reference.
Pure addition of selected face values; persistent 27-card deck; 5 opening cards; per-encounter second lucky target chosen from 3; Draw 2 or Return 2 / Draw 3; hand cap 9 with explicit discard; lucky sums spend combo cards to break armor, then one separate attack card activates Strike/Ward/Echo. Normal attack is always an alternative. Three encounters, healing, victory/defeat and replay. No external runtime services.

`Assets/_LuckySeven/Scripts/Core` owns deterministic rules and session state, without Unity references. Runtime assembly contains Config, Game, UI and Presentation. Editor tools migrate the backed-up V1 scene/config and build WebGL into Builds/WebGLV2. NUnit assemblies validate rules and UI flow.

UI is uGUI + TMP with InputSystemUIInputModule. Current UI defaults to Simplified Chinese and supports persisted English switching through Unity Localization. Both locale font entries use the bundled bilingual Noto Sans SC TMP asset to avoid transient missing glyphs while text refreshes. Cards use vector-drawn poker faces and raised hover previews. Enemy stats and player health/healing are in GameConfig; deck, hand limit, draw counts and ability constants currently live in BattleSession. UI is serialized in Main and can be edited in the Inspector.

## Evidence boundaries

Installation is not a successful build. Build, NUnit and runtime results are recorded separately in `Docs/Validation` and `Docs/Framework.md`. itch.io publication and two-browser manual acceptance are not implied by local compilation.

V2 validation and actual-browser mouse playthrough evidence live in `Docs/Validation/V2`; V1 source backup lives in `Backups/V1BeforeCards`. `WebVisibleState` mirrors only visible HUD fields in Development WebGL builds for acceptance scripts. No state mutation endpoint.

Table-card interaction update: hand cards now overlap at the bottom; staged selections animate into a central table. Click or drag stages cards; clicking staged cards or dragging back returns them. Confirmation still owns spending. `CardMotion` handles EventSystem pointer/drag input with click suppression and invalid-drop return. `BattleHUDView.ConfigureTableLayout` performs a targeted scene migration, preserving scene objects, references, config and GUIDs. Validation lives in `Docs/Validation/TableCards`; previous UI/scene backup is `Backups/V2BeforeTable`.

Localization/poker update: `Docs/LocalizationAndPokerCards.md` supersedes the English-only prototype assumption. Unity Localization package and Addressables are installed; 63 zh-Hans/en keys plus TMP font Asset Tables with a shared bilingual font. Default zh-Hans, persisted language choice, asynchronous WebGL initialization, input disabled until locale refresh finishes. PokerCardFace draws card stock/suits/pips. CardMotion raises the face while keeping exposed neighbour input strips stable. Validation: `Docs/Validation/LocalizedCards`. Web builds now use non-Development configuration, including read-only visible-state telemetry for acceptance tests.

Resource Combo V1 (2026-11-07): 1/2/3+ cards spent to break grant +0/+2/+4 damage on the one subsequent breached attack. Selection changes preserve the bonus; attacks consume it; normal attacks never use it. TryPreviewAttack shares damage calculation with BeginAttack. Bilingual formula and preview use the existing HUD. See Docs/ResourceComboV1.md and Docs/Validation/ResourceComboV1. GameConfig stats remain unchanged.
