# Git 使用说明

本项目使用本地 Git 仓库，主分支 `main`。初始版本包含资源与组合 V1：抽2、换2抽3、组合加伤0/2/4和伤害预览。尚未配置远程仓库。

## 纳入版本管理的内容

- `Assets`：源码、场景、资源及对应 `.meta`；不要单独删除或重新生成已提交的 `.meta`。
- `Packages`：manifest 与 packages-lock，保留依赖版本。
- `ProjectSettings`：Unity 项目设置，使用 6000.3.7f1。
- `Docs`、`Tools`：设计文档、验证记录、截图、录屏和工具脚本。
- `.gitignore`、`.gitattributes`：忽略规则、换行和 LFS 规则。

Library、Temp、Logs、UserSettings、Builds、Backups、Python 缓存和生成的 Addressables 构建输出不提交。文件仍保留在本机。`Backups` 是历史迁移备份，不是新克隆运行所需文件；旧的 Apply Pixel Adventure 迁移菜单仍检查本机备份，日常打开现有 Main 场景不需要运行它。

图片、字体、音频、模型、录屏和压缩文件使用 Git LFS；Unity 场景、预制体、配置和 `.meta` 保持可读文本。推送远程仓库时需要支持 Git LFS 的服务。

## 日常操作

在项目目录运行：

```powershell
git status
git switch -c feature/battle-rewards
# 完成一组相关改动并测试后，先检查差异。
git diff
git add Assets Packages ProjectSettings Docs Tools
git diff --cached --stat
git commit -m "Add post-battle reward selection"
```

上面的分支名和提交说明是未来功能的示例，并不代表战后奖励已经实现。提交前保存 Unity 场景和资源；确认没有把临时文件一起加入。

## 新电脑克隆

安装 Git、Git LFS 和 Unity 6000.3.7f1（含 Web Build Support）。配置远程仓库后：

```powershell
git lfs install
# 使用实际远程地址执行 git clone，然后进入项目目录。
git lfs pull
git lfs fsck
```

通过 Unity Hub 打开仓库目录，等待依赖和资源导入，再打开 `Assets/_LuckySeven/Scenes/Main.unity`。LFS 文件未下载时不要让 Unity 把指针文本当作资源导入。

## 本机配置与合并

本仓库使用仓库级 `core.autocrlf=false`，具体换行由 `.gitattributes` 控制。初始化提交沿用已有 Git 用户身份，不修改全局配置。

本机配置 UnityYAMLMerge 驱动，路径为 `D:/6000.3.7f1/Editor/Data/Tools/UnityYAMLMerge.exe`。`.gitattributes` 会随仓库克隆，`.git/config` 的本机工具路径不会；其他电脑需要按自己的 Unity 安装路径配置该驱动。遇到场景或预制体冲突，合并后仍需在 Unity 检查引用和场景内容。

远程仓库地址确定后再添加 `origin` 并推送；本次初始化没有创建远程仓库，也没有上传项目。
