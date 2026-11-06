# 来源与权利说明

## 本次模型产物

本项目的 C# 玩法、工程生成器、程序化几何、UI 与合成音频代码在当前会话中由 `gpt-6-astra` 生成。没有将其他项目的人工核心实现搬入本项目。人工参与为需求提出、项目编号确认及后续质检；截至开工版本未记录人工直接修补代码。

模型服务方记录为 OpenAI。套餐、具体后端快照及平台会话导出由账号持有人补充。模型输出适用条款应按账号实际使用的服务协议核验，可参考 https://openai.com/policies/terms-of-use/ 与 https://openai.com/policies/services-agreement/；本文件不代替实际账户协议，也不替用户确认训练和商用授权。

模型生成的原创源码及程序化资产对约定交付、训练、内部加工、模型商用的权利状态：**待采购商务与法务结合实际服务条款确认**。不额外捏造 MIT 或其他开源许可证。

## 引擎与依赖

Unity 编辑器、播放器及其运行库属于 Unity Technologies，使用适用的 Unity 服务与软件许可。URP、Core RP、Shader Graph、Collections、Burst 及其他传递依赖通过 Unity 包管理器解析，实际版本和来源以 `Packages/packages-lock.json` 为准。它们不是本项目新增的有效应用代码，不作为本次模型生成源码计量。

需要保留的依赖 LICENSE、NOTICE、Third Party Notices 将从实际解析包复制至 `ThirdPartyNotices`。依赖的存在不自动证明整包适用于采购方模型训练；不同许可限制由商务与法务逐项核实。构建产物包含 Unity 运行库，分发与源码训练应分别审查。

## 美术 音频 字体

主角、机械敌人、场景几何、材料颜色参数和音效波形均通过本项目代码生成，未引用外部模型、贴图、商业音乐或远程素材。程序生成方法保留在 `ArtFactory.cs`、`WorldBuilder.cs` 和 `SoundDesign.cs`。

中文界面调用目标 Windows 系统已安装的 Microsoft YaHei UI / Microsoft YaHei，回退 Arial；未复制或分发 Windows 字体文件，字体不计入交付资产。目标电脑若没有相应字体，需要安装合法中文字体或调整系统字体选择。

## 范围与状态

本地代码、资产处理、构建与打包不构成对外发布。未经用户授权不上传源码、不发布构建、不购买付费资产。真实许可标识、账号、设备地址等不进入公开交付摘要；本机原始 Unity 日志位于排除打包的 `Logs` 中。
