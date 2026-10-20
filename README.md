# Affdata.Core

AffdataEdit 和 AffdataPlay 共用的 Unity UPM 库，直接依赖 `UnityEngine`。
范围包括两边共用的谱面解析、谱面数据及游戏场景渲染代码：
网格定义、曲线与坐标、分段采样、排序规则、颜色和相关数学计算。
`Runtime/` 编译为一个 `Affdata.Core` 程序集；不再提供独立的纯 .NET 项目入口。
只放两边共用的代码，不放完整场景、游戏单例或项目专用功能。
谱面解析与共用谱面数据模型按实际复用需求逐步迁入；下面列出当前已经迁入的内容。

当前包含 `Affdata.Core.Geometry` 下的 Arc 曲线类型、X/Y 插值、坐标转换和
Qi/Qo 相机缓动。进度参数不会自动限制在 `[0, 1]`；未知曲线类型按直线处理，
与两个项目原有实现一致。项目特有的原版精度模拟、渲染提交和运行状态仍在各自项目中。

`ArcSampling.FillProgress` 共用 project-arc 的 Arc 分段进度：原始时长小于 1 秒时
每秒 14 段，否则每秒 7 段，再乘以不小于 1 的 smoothness；用 double 累加进度，
并单独追加准确终点。连接段可延长到后续 Arc 的开始时间；直线 Trace 默认只用两端点。
两边的跨 timing 变速选项保留多段采样，坐标精度模式仍由各项目管理。
`Affdata.Core.Chart.ArcRenderOrder` 共用 Play 的 Arc / Trace 面排序：
按曲线端点高度、颜色及左右面排序，同优先级使用相同的谱面与分段排序键，
不按相机距离重排 Arc 面。场景绘制通道和其他 Note 的排序仍由各项目管理。
Hold 渐隐依赖 Play 的实际判定状态，留在 Play 中，不属于共享库。

颜色共用代码位于 `Affdata.Core.Colors`：`Rgba32` 表示四个 8 位颜色通道，
`HexColor` 统一 AFF 的 `#RGB`、`#RGBA`、`#RRGGBB`、`#RRGGBBAA` 解析与输出。
`Affdata.Core.Chart.ArcColors.GoldTrace` 保存两边共用的金色 Trace 颜色。
各项目的皮肤配置、专用色板及 Unity 颜色适配保留在各自项目。

## 共用网格

`Affdata.Core.Geometry.NoteMeshDefinitions` 集中生成 Arc 段、头部、阴影、ArcTap、
音效 ArcTap、Tap/Hold 平面、连接线、ArcCap、高度指示器及 Slide 括号、判定线网格。
`CreateSprite` 保留皮肤 Sprite 的轮廓、轴心与图集 UV。
每次工厂调用都会创建新 `Mesh`，调用方负责缓存，并在不再使用时 `Destroy`。
`Transform` 只用于适配新建网格的坐标及 UV，不应修改已被其他调用方缓存的网格。

音效 ArcTap 的几何和 UV 以 project-arc 的 `sfx_l.obj` / `sfx_d.obj` 为参考，
保留翼部、上下尖刺和倾斜核心；两个子网格分别对应外部皮肤的翼部与核心贴图。
忽略 8 个零面积三角形后为 108 + 12 个三角形。源模型 Z 轴已反向，
游戏世界中的缩放为 4.2，中心相对 Arc 点为 `(0, -0.45, -0.6)`。
贴图、材质和渲染队列由各项目管理，不包含在 Core 中。
连接线宽度同样对齐 project-arc，为 0.15；Tap / Hold 宽度分别为 3.82 / 3.825，
普通 ArcTap 中心高度为 -0.45，Arc / ArcTap 阴影高度分别为 0.05 / 0.06，
高度指示器宽 1.2、底部高度 0.05。ArcTap 阴影基础深度为 1.2，
两边共用随速度变化的 1～1.5 倍缩放。游戏相机的 Tap 长度也共用 project-arc
的距离、速度公式及长度取整；Edit 的正交编辑视图保留固定长度。
剩余尺寸、UV 和运行时差异记录在对应工厂方法旁的注释中。

Slide 曲面的顶点和三角形连接也在 `NoteMeshDefinitions.Slide.cs` 中共用，
统一为 Edit 的每排 2 个端点、每段 2 个三角形，相邻段复用顶点。
两边提供采样端点、谱面范围、时间进度及视觉距离，由 Core 写入相同的 UV0–2 格式。
采样时机、谱面状态、材质及渲染提交仍由各项目管理；Play 的额外批次数据和采样差异在 Core 注释中说明。

## Unity UPM

本地目录结构：

```text
Affdata/
├── Affdata.Core/
├── AffdataEdit/
└── AffdataPlay/
```

在两个 Unity 项目的 `Packages/manifest.json` 的 `dependencies` 中加入：

```json
"net.affdata.core": "file:../../Affdata.Core"
```

相对路径以项目的 `Packages/` 目录为起点，参见
[Unity 本地包路径文档](https://docs.unity3d.com/Manual/upm-localpath.html)。
有 `.asmdef` 的程序集还需添加 `Affdata.Core` 引用；默认 `Assembly-CSharp`
通过包的 `autoReferenced` 自动引用。

```csharp
using Affdata.Core.Geometry;

float x = ArcCurve.X(0f, 1f, 0.5f, ArcCurveType.Si);
float worldX = ArcCoordinates.ArcXToWorld(x);
```

## Git 与维护

三个目录分别维护各自的 Git 仓库。共享算法只在 Core 中修改和提交；
Edit、Play 提交自己的适配代码及包依赖。换机器时将三个仓库克隆到同一父目录。
本地包直接使用 Core 工作目录的内容，开发时无需复制代码或 DLL。
远程仓库：[anqtal/Affdata.Core](https://github.com/anqtal/Affdata.Core)（当前为私有仓库）。
每次提交后立即推送当前分支；首次推送使用 `git push -u origin HEAD`，
之后使用 `git push`。具体规则见 [AGENTS.md](AGENTS.md)。
将来发布包时，可把 UPM 依赖改为固定 tag 或提交的 Git URL。

当前尚未发布包。迁入代码和模型数据的来源说明见
[Third Party Notices.md](<Third Party Notices.md>)；
整个包的发布许可证需在正式开源前确定。
