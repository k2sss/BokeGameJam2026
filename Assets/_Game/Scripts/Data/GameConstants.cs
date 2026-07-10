public static class GameConstants
{
    public static class Tags
    {
        public const string Player = "Player";
        public const string Death = "Death";
    }

    public static class AudioNames
    {
        public const string GameOver = "GameOver";
    }

    /// <summary>
    /// 场景名称占位常量。后续在 Build Settings 中注册真实场景后与此处保持一致。
    /// </summary>
    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string Level01 = "Level_01";
        public const string Level02 = "Level_02";
        public const string Level03 = "Level_03";
        public const string Level04 = "Level_04";

        /// <summary>当前开发用示例场景，与 SampleScene.unity 对应。</summary>
        public const string SampleScene = "SampleScene";
    }
}
