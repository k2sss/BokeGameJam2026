# 场景管理系统使用说明

本文档说明如何在 Unity 场景中配置和使用本项目的场景切换系统，并介绍相关脚本与 API。

---

## 一、系统概览

场景管理系统由以下几部分组成：

| 模块 | 脚本 / 资产 | 职责 |
|------|-------------|------|
| 场景流程 | `SceneFlowManager` | 统一入口，负责异步加载与转场编排 |
| 转场 UI | `SceneTransitionUI` | 黑屏淡出 / 过关图文渐变 |
| 关卡配置 | `LevelDatabaseSO` + `LevelDatabase.asset` | 关卡顺序、转场素材、主菜单场景名 |
| 游戏状态 | `GameStateManager` | 冻结输入（Playing / GameOver / Transitioning） |
| 触发器 | `ExitDoor` | 玩家触碰出口 → 加载下一关 |
| UI 重试 | `GameOverUI` | 死亡后 Retry → 重载当前关 |
| 存档 | `SaveManager` | 本地 `save.json`，关卡进度与解锁 |
| 主界面 | `MainMenuController` + `MainMenuView` | 逻辑与视图分离；美术拖引用 |

### 加载流程

```
触发（ExitDoor / Retry / 代码调用）
    ↓
PrepareForSceneLoad（暂停游戏、隐藏 GameOver UI）
    ↓
转场淡出（SimpleFade 或 LevelComplete）
    ↓
SceneManager.LoadSceneAsync
    ↓
转场淡入
    ↓
ResetToPlaying
```

---

## 二、在场景中的配置步骤

### 1. 准备关卡配置表

1. 打开 `Assets/_Game/Data/ScriptableObjects/LevelDatabase.asset`
2. 按通关顺序填写 `levels` 列表，每项包含：
   - **sceneName**：与 `.unity` 场景文件名一致（需加入 Build Settings）
   - **displayName**：显示用名称（可选）
   - **backgroundSprite / characterSprite**：过关转场用图（可选，留空则显示纯色占位）
3. 设置 **mainMenuSceneName**：全部关卡通关后返回的主菜单场景名

当前配置示例：

| 顺序 | sceneName | 说明 |
|------|-----------|------|
| — | MainMenu | 主界面（非关卡列表项） |
| 0 | level1 | 第 1 关 |
| 1 | level2 | 第 2 关 |
| 2 | level3 | 第 3 关 |
| 3 | level4 | 第 4 关（最后一关） |
| 通关后 | MainMenu | 主菜单 |

> 场景名须与 `.unity` 文件名一致（不含扩展名）。常量见 `GameConstants.SceneNames`。

### 2. 注册 Build Settings

**File → Build Settings** 中 Add Open Scenes，确保以下场景均已注册：

- 主菜单场景 `MainMenu`
- 所有关卡场景：`level1`、`level2`、`level3`、`level4`

未注册的场景调用 `LoadScene` 时会在 Console 报错，并自动恢复游戏（不会卡死）。

### 3. 放置 SceneFlowManager

在**启动场景**（或第一个加载的关卡场景）中：

1. 新建空物体，命名为 `SceneFlowManager`
2. 添加组件 `SceneFlowManager`
3. 在 Inspector 中将 `LevelDatabase.asset` 拖入 **Level Database** 字段
4. （可选）同一物体上添加 `SceneTransitionUI`；若不添加，运行时由 Manager 自动创建

**注意：**

- `SceneFlowManager` 使用 `DontDestroyOnLoad`，**全局只需一份**
- 不要在每个关卡场景各放一份；切换场景后会自动保留
- 若新场景中也有一份，重复实例会被自动销毁

### 4. 放置 GameStateManager

场景中需存在 `GameStateManager`（通常已有 `GameStateMgr` 物体）。  
转场期间 `IsPlaying == false`，`PlayerController` 会自动拒绝输入。

### 5. 配置出口门 ExitDoor

在关卡末尾放置出口物体：

1. 添加 **Collider2D**，勾选 **Is Trigger**
2. 添加脚本 **ExitDoor**
3. 确保玩家的 `PlayerBody` 物体 Tag 为 **Player**

玩家触碰后会自动调用 `LoadNextLevel()`；最后一关则跳转主菜单。

### 6. GameOver 重试（已接入）

`GameOverUI` 的 Retry 按钮已改为调用 `SceneFlowManager.ReloadCurrentLevel()`，使用 SimpleFade 黑屏重载当前关，无需额外配置。

### 7. 从代码 / UI 手动切换场景

```csharp
// 加载下一关（末关回主菜单）
SceneFlowManager.Instance.LoadNextLevel();

// 重载当前关（黑屏转场，用于重试）
SceneFlowManager.Instance.ReloadCurrentLevel();

// 按索引加载指定关（过关图文转场）
SceneFlowManager.Instance.LoadLevel(0);

// 加载主菜单
SceneFlowManager.Instance.LoadMainMenu();

// 按场景名加载（可指定转场模式）
SceneFlowManager.Instance.LoadScene("level1", TransitionMode.LevelComplete);
SceneFlowManager.Instance.LoadScene("MainMenu", TransitionMode.SimpleFade);
```

调用前请确认 `SceneFlowManager.Instance != null`。

---

## 三、开发调试

以下测试脚本仅用于开发，**正式场景可不挂载**：

| 脚本 | 按键 | 功能 |
|------|------|------|
| `SceneFlowTest` | O / P / M | LoadNextLevel / ReloadCurrentLevel / LoadMainMenu |
| `SceneTransitionTest` | U / I | SimpleFade / LevelComplete 转场预览 |
| `GameStateTest` | T / Y / R | GameOver / Transitioning / ResetToPlaying |

编辑器菜单 **Game → Tests → Validate LevelDatabase** 可验证关卡配置表逻辑。

---

## 四、API 参考

### SceneFlowManager

路径：`_Game/Scripts/Core/SceneFlowManager.cs`

| 成员 | 说明 |
|------|------|
| `Instance` | 单例访问（继承自 `BaseMonoManager`） |
| `IsLoading` | 是否正在执行加载流程；加载中重复请求会被忽略 |
| `CurrentSceneName` | 当前激活场景名 |
| `CurrentLevelIndex` | 当前关在配置表中的索引，未找到为 -1 |
| `IsLastLevel` | 当前关是否为最后一关 |
| `LoadScene(sceneName, mode)` | 按场景名加载；默认 SimpleFade |
| `LoadLevel(index)` | 按索引加载；使用 LevelComplete 转场及该关配置的 Sprite |
| `LoadNextLevel()` | 加载下一关；有下一关用 LevelComplete，末关回主菜单用 SimpleFade |
| `ReloadCurrentLevel()` | 重载当前场景；SimpleFade 黑屏转场 |
| `LoadMainMenu(mode)` | 加载主菜单；默认 SimpleFade |

### SceneTransitionUI

路径：`_Game/Scripts/UI/SceneTransitionUI.cs`

| 方法 | 说明 |
|------|------|
| `PlayLevelTransitionOut(bg, character)` | 过关淡出：黑屏 → 展示背景与角色图 |
| `PlayLevelTransitionIn()` | 过关淡入：隐藏转场层，露出新场景 |
| `PlaySimpleFadeOut()` | 简单淡出到黑屏 |
| `PlaySimpleFadeIn()` | 从黑屏淡入 |
| `HideImmediate()` | 立即隐藏转场 UI |

一般由 `SceneFlowManager` 内部调用；动画使用 DOTween，不受 `timeScale=0` 影响。

### GameStateManager

路径：`_Game/Scripts/Core/GameStateManager.cs`

| 成员 / 方法 | 说明 |
|-------------|------|
| `CurrentState` | 当前状态：`Playing` / `GameOver` / `Transitioning` |
| `IsPlaying` | 是否可操作（`PlayerController` 依赖此属性） |
| `IsTransitioning` | 是否处于转场中 |
| `EnterGameOver()` | 进入死亡状态，暂停并显示 Game Over UI |
| `EnterTransitioning()` | 从 Playing 进入转场（GameOver 状态下无效） |
| `PrepareForSceneLoad()` | 场景加载前调用；可从 Playing 或 GameOver 进入转场 |
| `ResetToPlaying()` | 恢复游玩状态；场景加载完成后调用 |

### LevelDatabaseSO

路径：`_Game/Scripts/Data/LevelDatabaseSO.cs`  
资产：`_Game/Data/ScriptableObjects/LevelDatabase.asset`

| 成员 / 方法 | 说明 |
|-------------|------|
| `MainMenuSceneName` | 主菜单场景名 |
| `Levels` | 只读关卡列表 |
| `LevelCount` | 关卡数量 |
| `GetIndexByScene(sceneName)` | 按场景名查索引，未找到返回 -1 |
| `GetLevel(index)` | 获取指定关卡配置 |
| `GetNextLevel(currentIndex)` | 获取下一关配置；已是最后一关返回 null |
| `IsLastLevel(index)` | 是否为最后一关 |
| `ResolveNextSceneName(index)` | 按索引解析下一目标场景名 |
| `ResolveNextSceneNameByCurrentScene(name)` | 按当前场景名解析下一目标 |

### LevelEntry

路径：`_Game/Scripts/Data/LevelEntry.cs`

| 字段 | 说明 |
|------|------|
| `sceneName` | 场景名 |
| `displayName` | 显示名 |
| `backgroundSprite` | 过关转场背景图 |
| `characterSprite` | 过关转场角色图 |
| `IsValid` | 是否包含有效 sceneName |

### TransitionMode

路径：`_Game/Scripts/Data/TransitionMode.cs`

| 值 | 说明 |
|----|------|
| `LevelComplete` | 过关转场：背景 + 角色图渐变 |
| `SimpleFade` | 简单转场：纯黑屏淡入淡出 |

### GameConstants.SceneNames

路径：`_Game/Scripts/Data/GameConstants.cs`

场景名占位常量，代码中引用以避免拼写错误：

- `MainMenu`、`level1`、`level2`、`level3`、`level4`

### ExitDoor

路径：`_Game/Scripts/Gameplay/ExitDoor.cs`

玩家（Tag=`Player` 且带 `PlayerBody`）进入 Trigger 后：

1. 检查 `IsPlaying` 且未在加载
2. `EnterTransitioning()`
3. `LoadNextLevel()`

`LoadNextLevel()` 行为：当前在**主菜单**时进入第一关（`level1`）；在关卡内时进入下一关，末关通关后回主菜单。

内置 `triggered` 标志，防止同一次停留重复触发。

### BaseMonoManager

路径：`_Game/Scripts/Core/BaseMonoManager.cs`

MonoBehaviour 单例基类。子类重写 `PersistAcrossScenes => true` 时自动 `DontDestroyOnLoad` 并销毁重复实例。`SceneFlowManager` 已启用跨场景常驻。

---

## 五、常见问题

**Q：`SceneFlowManager not found in scene`**

场景中未放置 `SceneFlowManager`，或未能在 Awake 前访问 Instance。请在启动场景添加并配置 Level Database。

**Q：加载失败，Console 提示 Build Settings**

目标场景未加入 Build Settings，或 sceneName 与文件名不一致。

**Q：过关后没有显示转场图**

在 `LevelDatabase.asset` 对应关卡的 `backgroundSprite` / `characterSprite` 中拖入 Sprite；留空则显示黑色占位。

**Q：转场时玩家仍能操作**

确认 `GameStateManager` 存在；转场期间 `IsPlaying` 应为 false。

**Q：切换场景后 Manager 消失**

`SceneFlowManager` 必须在某个场景中存在且 Awake 成功；若仅依赖测试脚本动态创建，离开 Play 后会丢失，正式使用请在场景中手动放置。

---

## 六、主界面 UI 搭建（美术 / 策划）

> 主界面完整说明见 [MainMenuGuide.md](MainMenuGuide.md)（架构、按钮行为、测试、待办缺口）。

主界面采用 **Controller + View 引用** 模式，代码不生成 UI。请在 Editor 中自行摆 Canvas 并拖引用。

### 场景结构建议

```
MainMenu.unity
├── Main Camera
├── EventSystem
├── MainMenuSystems          ← MainMenuController + LevelDatabase
└── MainMenuCanvas           ← Canvas + MainMenuView
    ├── Background           (Image)
    ├── TitleArea            (Text × 2)
    ├── MenuArea             (Button × 5)
    ├── ShowcaseArea         (Image + CharacterCarousel)
    └── Panels/
        └── OverwriteSaveDialog   ← OverwriteSaveDialogView
```

### Inspector 拖引用清单

| 组件 | 需要拖入的引用 |
|------|----------------|
| `MainMenuController` | `MainMenuView`、`OverwriteSaveDialogView`、`LevelDatabase` |
| `MainMenuView` | 5 个菜单 Button、可选继续禁用遮罩、`CharacterCarousel` |
| `OverwriteSaveDialogView` | `panelRoot`、动态/固定文案 Text、确认/取消 Button |
| `CharacterCarousel` | `displayImage`、可选 `manualSprites` 或 `LevelDatabase` |

### 右键校验

在 `MainMenuView` / `OverwriteSaveDialogView` 上 **右键 → Validate References**，缺引用时 Console 会报错。

### 按钮接线

由 `MainMenuView.Bind()` 在运行时自动注册，**无需**在 Button 的 OnClick 里手动绑 `MainMenuController` 方法。

---

## 七、相关文件索引

```
_Game/
├── Data/ScriptableObjects/LevelDatabase.asset   # 关卡配置资产
├── Scripts/
│   ├── Core/
│   │   ├── SceneFlowManager.cs
│   │   ├── SaveManager.cs
│   │   ├── GameStateManager.cs
│   │   ├── BaseMonoManager.cs
│   │   ├── SceneFlowTest.cs          # 调试
│   │   ├── SceneTransitionTest.cs    # 调试
│   │   └── GameStateTest.cs          # 调试
│   ├── UI/
│   │   ├── SceneTransitionUI.cs
│   │   ├── GameOverUI.cs
│   │   ├── MainMenuController.cs
│   │   ├── MainMenuView.cs
│   │   ├── OverwriteSaveDialogView.cs
│   │   ├── ExitGameDialogView.cs
│   │   ├── SettingsDialogView.cs
│   │   ├── LevelAchievementView.cs
│   │   ├── LevelAchievementItemView.cs
│   │   ├── CreditsView.cs
│   │   └── CharacterCarousel.cs
│   ├── Data/
│   │   ├── LevelDatabaseSO.cs
│   │   ├── LevelEntry.cs
│   │   ├── TransitionMode.cs
│   │   └── GameConstants.cs
│   └── Gameplay/
│       └── ExitDoor.cs
└── Scenes/
```

场景加载完成后 `SceneFlowManager.FinishLoad` 会触发 BGM 切换，详见 [AudioSystemGuide.md](AudioSystemGuide.md)。
