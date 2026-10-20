# Affdata.Core

AffdataEdit 和 AffdataPlay 共用的纯 C# 核心库。`.NET` 与 Unity UPM 编译同一份
`Runtime/` 源码，不依赖 `UnityEngine`、场景、资源或游戏单例。

当前包含 `Affdata.Core.Geometry` 下的 Arc 曲线类型、X/Y 插值、坐标转换和
Qi/Qo 相机缓动。进度参数不会自动限制在 `[0, 1]`；未知曲线类型按直线处理，
与两个项目原有实现一致。项目特有的原版精度模拟、渲染和运行状态仍在各自项目中。

颜色共用代码位于 `Affdata.Core.Colors`：`Rgba32` 表示四个 8 位颜色通道，
`HexColor` 统一 AFF 的 `#RGB`、`#RGBA`、`#RRGGBB`、`#RRGGBBAA` 解析与输出。
`Affdata.Core.Chart.ArcColors.GoldTrace` 保存两边共用的金色 Trace 颜色。
各项目的皮肤配置、专用色板及 Unity 颜色适配保留在各自项目。

## .NET

目标框架为 `netstandard2.1`，使用 .NET SDK：

```sh
dotnet build DotNet~/Affdata.Core.csproj
```

其他 .NET 项目可以通过 `ProjectReference` 引用该 `.csproj`。
构建输出在 `DotNet~/bin/` 和 `DotNet~/obj/`；Unity 忽略带 `~` 后缀的目录，
避免把生成的源码或 DLL 再次导入。

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
将来发布 Core 的远程仓库后，可把 UPM 依赖改为固定 tag 或提交的 Git URL。

当前尚未配置远程仓库或发布包。迁入代码的来源及 MIT 许可见
[Third Party Notices.md](<Third Party Notices.md>)；
整个包的发布许可证需在正式开源前确定。
