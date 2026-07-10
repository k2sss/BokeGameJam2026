/// <summary>
/// 关卡星级数据接口（扩展功能占位）。
/// 主流程 UI 可依赖此接口；完整星级系统在扩展 Phase 实现。
/// </summary>
public interface ILevelStarProvider
{
    /// <summary>获取某关历史最高星数（0~3）。</summary>
    int GetStarCount(int levelIndex);

    /// <summary>过关结算写入星数（同关取较高值）。扩展 Phase 实现。</summary>
    void SettleLevelStars(int levelIndex, int starCount);

    /// <summary>清空所有关卡星级。扩展 Phase 实现。</summary>
    void ResetAllStars();
}
