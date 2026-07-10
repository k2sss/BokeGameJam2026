public static class GameConstants
{
    public static class Tags
    {
        public const string Player = "Player";
        public const string Death = "Death";
        public const string AttachPoint = "AttachPoint";
    }

    /// <summary>音量默认值，也用于主菜单等非关卡场景。</summary>
    public const float DefaultAudioVolume = 1f;

    public static class AudioNames
    {
        public const string MainMenuBGM = "MainMenuBGM";
        public const string Level1BGM = "Level1BGM";
        public const string Level2BGM = "Level2BGM";
        public const string Level3BGM = "Level3BGM";
        public const string Level4BGM = "Level4BGM";
        public const string GameOverBGM = "GameOverBGM";
        public const string ButtonClick = "ButtonClick";
        public const string LevelClear = "LevelClear";
        public const string AttachPoint = "AttachPointSfx";
        public const string Countdown = "CountdownSfx";
        public const string Collision = "CollisionSfx";
        public const string PlayerHit = "PlayerHitSfx";
        public const string Fall = "FallSfx";
        public const string WaterSplash = "WaterSplashSfx";
        public const string StoryTyping = "StoryTypingSfx";

        /// <summary>兼容旧名，请改用 <see cref="GameOverBGM"/>。</summary>
        public const string GameOver = GameOverBGM;
    }

    /// <summary>本地存档文件名，完整路径见 SaveManager.SaveFilePath。</summary>
    public const string SaveFileName = "save.json";

    /// <summary>关卡总数，与 LevelDatabase.asset 中 levels 数量一致。</summary>
    public const int LevelCount = 4;

    /// <summary>
    /// 场景名称常量，格式为 level + 序号。
    /// 须与 Build Settings 中场景文件名（不含 .unity）完全一致。
    /// </summary>
    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string Level1 = "level1";
        public const string Level2 = "level2";
        public const string Level3 = "level3";
        public const string Level4 = "level4";
    }
}
