# U2 发条信使

《发条信使》是固定侧视的三维机械动作探索游戏。玩家携带能源芯，穿过齿轮工坊、升降井与钟面核心，获得永久二段跳，击败两阶段守门机械体并完成送达。

## 启动

Windows 构建完成后，双击根目录 `Launch.cmd`，或运行 `Builds/Windows/ClockworkCourier.exe`。初始窗口为 1440×900；推荐键盘与支持 Direct3D 11 的 Windows 10/11 电脑。游戏本体无需账号和联网。

开发环境固定为 Unity **6000.6.7f1**、URP **17.6.0**、Windows x64、Mono、.NET Standard。使用本机现有 Unity 6.6 和配套 URP 保持工具链一致；这是本项目实际选择，不将其冒称为 LTS。包版本见 `Packages/manifest.json` 与 `packages-lock.json`。首次导入可能需要访问 Unity 包注册表，编辑器构建需要有效 Unity 许可。

在 Unity Hub 中打开本目录，再打开 `Assets/Scenes/ClockworkCourier.unity` 并播放。若要重建代码生成的资产和场景，运行菜单 `Clockwork Courier/Rebuild scene and generated assets`；生成入口为 `Assets/Editor/ProjectBuild.cs`。Windows 构建可用菜单 `Clockwork Courier/Build Windows`，或 PowerShell：

```powershell
powershell -ExecutionPolicy Bypass -File .\Build.ps1 -Editor "E:\Program Files\6000.6.7f1\Editor\Unity.exe"
```

## 操作和规则

| 操作 | 按键 |
| --- | --- |
| 左右移动 | A / D 或方向键 |
| 跳跃 | Space；短按小跳，长按高跳 |
| 二段跳 | 在升降井安装能力后，空中再按 Space |
| 冲刺 | 左 Shift；空中一次，落地重置 |
| 普通攻击 | J；有前摇、有效阶段与恢复阶段 |
| 精准格挡 | K；面向来袭方向，在 0.22 秒窗口内接住攻击 |
| 中继站、能力、拉杆、送达 | E |
| 暂停 | Esc |

生命上限 6 格，电量上限 100。强化冲刺消耗 16 电量，低电量仍可基础冲刺。格挡消耗 10 电量，成功回流 26 电量；面对远程攻击可反击。电量缓慢恢复，中继站补满生命与电量。每次攻击对同一目标只命中一次，普通受伤后有保护时间。

敌人红色环提示即将攻击。压机的红色灯预告下压；伤害为 2 格。掉入深坑会触发死亡。主角死亡后可从最近激活的中继站重试；敌人与机关重建，永久能力、捷径及回收记录保留，击杀同一敌人不会重复奖励。

三段旅程共 6 个中继站。双级擒纵器位于升降井中段，靠近后按 E 安装；二段跳可进入工坊与升降井的高架支路，拉杆永久架起跨越缺口的捷径桥。守门体半血后进入过载阶段；低位冲击波可跳过，散射可躲避或格挡。击败它后在最右侧终端按 E 送达能源芯。

通关后解锁从三段区域起点开始的计时挑战，挑战不覆盖主旅程存档。暂停菜单提供重试、音量、屏幕震动、阴影质量及返回菜单。新旅程会要求确认后重置进度。

## 存档

正式存档：`%USERPROFILE%/AppData/LocalLow/ClockworkCourier/U2/courier-progress-v1.json`。中继站、永久能力与结算时保存，游戏过程中约每 20 秒保存一次。损坏存档保留为 `.invalid`，写入采用临时文件和备份，错误会提示。自动运行检查使用独立的 `smoke-progress-v1.json`，不覆盖正式存档。

## 工程结构

- `Assets/Scripts/Core`：状态、存档、计分去重和自动运行检查。
- `Assets/Scripts/Actors`：角色、敌人状态与弹丸。
- `Assets/Scripts/World`：三段关卡配置、升降台、压机、交互和障碍。
- `Assets/Scripts/Visual`：分件倒角模型、齿轮、关节动画、相机与代码生成音效。
- `Assets/Scripts/UI`：中文菜单、HUD、帮助、设置和结算。
- `Assets/Resources/GameTuning.asset`：可在 Inspector 中调整移动、跳跃、冲刺、格挡和攻击时序参数。
- `Assets/Generated`、`Assets/Prefabs`、`Assets/Scenes`：可复现的材质、网格、预制体和场景工程。
- `Validation`：真实检查结果与运行截图。截图须与记录中的源码版本对应。
- `Provenance`：生产来源、公开过程摘要、原始需求及源码版本关联。

主角、敌人、场景与音效由本项目代码生成，未下载第三方角色或美术素材。主角采用倒角壳体、关节轴承、液压杆、能源透镜、刀刃和背包结构，以及根据移动、攻击、格挡和受击状态驱动的关节动画。场景有 URP 光照、阴影、金属与搪瓷材质。完整视觉质量与手感仍以用户实际运行审核为准。

## 验证和交付状态

具体执行结果见 `Validation`，不能把设计目标当成实测通过。规则检查与自动状态检查不等于玩家已正常通关。试玩路线见 `PLAYTEST.md`。20—30 分钟流程与 1080p 60 FPS 为目标，完整通关时长、难度平衡和目标设备性能需要用户实测。

源码包含固定版本清单及归档哈希关联；`Library`、`Temp`、缓存及 Unity 编辑器不属于源码交付。依赖与权利说明见 `RIGHTS.md`；本会话标识已由 Codex 应用核验，套餐、后端快照与平台原始导出仍待补，不宣称已通过采购验收。
