# Bulk Policies & Defaults

先在地图上多选至少两名殖民者，再打开底部的“方案”页。着装、食物、用药、阅读四列各自在原有管理按钮下新增一个批量按钮；未多选殖民者时这些按钮不会显示。点击相应按钮后，从当前存档已有方案中选择要应用的方案。

阅读方案列右侧的齿轮按钮可以按存档设置殖民者、囚犯、奴隶和访客加入时自动应用的食物方案与医疗护理等级。首次使用时默认沿用游戏原版：食物使用默认方案，医疗护理等级使用对应类别在游戏设置中的默认值。设置随存档保存。

默认方案设置中，奴隶设置仅在启用 Ideology 时显示，访客设置仅在启用 Royalty 时显示；对应类别的自动应用逻辑也受相同 DLC 状态控制。

可选方案与截图中“着装方案”页面的方案一致：模组直接读取游戏当前存档的服装政策，不会创建或复制另一份方案。需要先安装 Harmony（Steam 创意工坊：<https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077>）。

批量操作只会更改对应的方案指派，不会强制小人立刻换装、改变饮食或执行其他行为；小人会按游戏原本的逻辑处理。

## 编译

需要安装 .NET 8 SDK 和 RimWorld 1.7 游戏程序集。在模组目录运行：

```powershell
dotnet build .\Source\BulkOutfitPolicy.csproj -p:RimWorldDir="D:\SteamLibrary\steamapps\common\RimWorld"
```

若 RimWorld 安装在其他位置，请将 `RimWorldDir` 改为对应安装目录。项目默认从 Steam Workshop 中 Harmony 的 1.7 文件夹引用 `0Harmony.dll`；若 Harmony 安装位置不同，也可通过 `HarmonyDir` 指定其 `1.7\Assemblies` 目录。编译后 DLL 会输出到 `1.7\Assemblies`。将整个 `BulkOutfitPolicy` 文件夹复制到 RimWorld 的 `Mods` 目录，并在游戏模组管理器中启用。
