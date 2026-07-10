# Assets 目录结构说明

> BokeGameJam2026 项目 Assets 目录组织规范（重组后版本）

## 设计原则

1. **`_` 前缀分区**：`_Game` 与 `_ThirdParty` 在 Project 窗口置顶，一眼区分自研与第三方
2. **最小 Resources**：仅放置必须 `Resources.Load` 的资源
3. **自研集中**：所有团队内容统一放在 `_Game/` 下
4. **插件只读**：第三方资源不修改，升级时整体替换

---

## 顶层结构

```
Assets/
├── _ThirdParty/              第三方插件（只读）
├── AmplifyShaderEditor/      着色器编辑器（保持默认路径，不可移动）
├── _Game/                    自研游戏内容
├── Resources/                运行时动态加载资源
├── Settings/                 渲染管线与项目配置
└── UniversalRenderPipelineGlobalSettings.asset
```

---

## `_ThirdParty/` — 第三方插件

| 子目录 | 来源 | 用途 |
|--------|------|------|
| `DOTween/` | Demigiant | 动画补间库 |
| `OdinInspector/` | Sirenix | Inspector 增强编辑器 |
| `RotaryHeart/` | Rotary Heart | SerializableDictionaryLite 工具 |

**注意：**
- 不要修改此目录下的任何文件
- `AmplifyShaderEditor` 因内部硬编码路径，保留在 `Assets/AmplifyShaderEditor/`，不在此目录

---

## `_Game/` — 自研内容

所有 Jam 期间新增的资源与脚本均放在此目录。

```
_Game/
├── Art/                      美术资源（按类型分类）
│   ├── Sprites/              精灵图、UI 图
│   ├── Audio/                原始音频文件（mp3/wav/ogg）
│   ├── Materials/            材质
│   ├── Shaders/              用 ASE 创建的自定义 Shader
│   └── Animations/           动画 Clip、Animator Controller
│
├── Data/                     ScriptableObject 资产文件
│   └── ScriptableObjects/
│
├── Prefabs/                  预制体
│
├── Scenes/                   游戏场景
│   └── SampleScene.unity
│
└── Scripts/                  C# 脚本
    ├── Core/                 框架与全局系统（Manager、单例基类）
    ├── Data/                 SO 类定义（C# 脚本，非资产）
    ├── Gameplay/             游戏逻辑（角色、敌人、关卡等）
    └── Editor/               仅编辑器脚本
        └── Tests/            调试与测试脚本
```

### Scripts 分层规则

| 目录 | 放什么 | 示例 |
|------|--------|------|
| `Core/` | 基础设施、Manager、工具类 | `BaseMonoManager`, `AudioManager` |
| `Data/` | ScriptableObject **类定义** | `AudioClipSO.cs` |
| `Gameplay/` | 具体游戏玩法逻辑 | 玩家控制器、敌人 AI |
| `Editor/Tests/` | 测试/调试（仅 Editor 编译） | `AudioTest.cs` |

> **区分**：`_Game/Data/` 存放 SO **资产**（`.asset` 文件），`_Game/Scripts/Data/` 存放 SO **C# 类**。

---

## `Resources/` — 运行时加载

遵循**最小化原则**，只放必须通过 `Resources.Load` 加载的内容。

```
Resources/
├── Audio/                    AudioClipSO 配置（Resources.LoadAll 路径 "Audio"）
│   └── TestAudio.asset
└── DOTweenSettings.asset     DOTween 全局配置
```

### 音频工作流

```
_Game/Art/Audio/break.mp3          ← 原始音频（Inspector 中引用）
         │ GUID 引用
         ▼
Resources/Audio/TestAudio.asset    ← 运行时配置（AudioClipSO）
         │ Resources.LoadAll<AudioClipSO>("Audio")
         ▼
_Game/Scripts/Core/AudioManager.cs ← AudioAssetLoadPath = "Audio"
```

**新增音频步骤：**
1. 将 `.mp3` / `.wav` 放入 `_Game/Art/Audio/`
2. 在 `_Game/Data/ScriptableObjects/` 或通过菜单 `Create > SO > AudioClip` 创建 SO
3. 将 SO 复制或链接到 `Resources/Audio/`（或在 `Resources/Audio/` 直接创建）
4. 确保 SO 的 `audioClipList` 引用了原始 clip

---

## `Settings/` — 项目配置

不参与 gameplay，存放渲染管线与编辑器模板。

```
Settings/
├── UniversalRP.asset           URP 渲染管线资产
├── Renderer2D.asset            2D 渲染器
├── Lit2DSceneTemplate.scenetemplate
└── Templates/                  编辑器场景模板（非游戏场景）
    └── URP2DSceneTemplate.unity
```

> 游戏场景在 `_Game/Scenes/`，模板场景在 `Settings/Templates/`，避免混淆。

---

## 新增资源速查

| 我要添加… | 放哪里 |
|-----------|--------|
| 精灵/UI 图 | `_Game/Art/Sprites/` |
| 音频文件 | `_Game/Art/Audio/` |
| 材质 | `_Game/Art/Materials/` |
| 自定义 Shader | `_Game/Art/Shaders/` |
| 预制体 | `_Game/Prefabs/` |
| 游戏场景 | `_Game/Scenes/` |
| SO 资产 | `_Game/Data/ScriptableObjects/` |
| 需运行时加载的 SO | `Resources/` 对应子目录 |
| 全局 Manager | `_Game/Scripts/Core/` |
| 玩法脚本 | `_Game/Scripts/Gameplay/` |
| 测试脚本 | `_Game/Scripts/Editor/Tests/` |
| 第三方插件 | `_ThirdParty/`（通常从 Asset Store 导入） |

---

## 注意事项

1. **移动资源请在 Unity Editor 内进行**（拖拽），以保留 `.meta` GUID，避免引用丢失
2. **不要移动 `AmplifyShaderEditor/`**，插件内部有硬编码路径
3. **`Resources/` 不要滥用**：所有资源都放进去会导致打包体积增大、加载变慢；项目变大后考虑迁移到 Addressables
4. **Build Settings 场景路径**：当前为 `Assets/_Game/Scenes/SampleScene.unity`

---

## 重组历史

- **重组日期**：2026-07-08
- **变更摘要**：
  - 新建 `_ThirdParty/`、`_Game/` 双分区
  - 第三方插件从 `Plugins/`、`Rotary Heart/` 迁入 `_ThirdParty/`
  - 自研脚本、场景、音频迁入 `_Game/`
  - URP 模板场景从 `Settings/Scenes/` 移至 `Settings/Templates/`
  - 删除空占位目录 `Art/`、`Scripts/Utils/`
