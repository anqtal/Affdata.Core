# Affdata.Core 仓库规则

## 库边界

- Core 是直接依赖 `UnityEngine` 的 Unity UPM 库，不再维持独立纯 .NET 类库入口。
- 只包含 Affdata 项目共用代码；场景实现、皮肤贴图、渲染队列和游戏单例留在各项目。
- 网格定义集中在 `NoteMeshDefinitions`，调用方负责缓存和释放生成的 `Mesh`。

## 提交与推送

- 每次成功提交后，立即将当前分支推送到 GitHub 的 `origin`，不要将推送留到后续任务。
- 已有上游分支时执行 `git push`；首次推送分支时执行 `git push -u origin HEAD`。
- 推送失败时明确报告原因及尚未推送的提交，不得把仅本地提交视为交付完成，也不要强制推送。
