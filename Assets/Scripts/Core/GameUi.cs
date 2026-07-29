namespace DungeonOdyssey.Core
{
    /// <summary>
    /// 모달 UI가 열려 입력/전투를 막을 때 사용.
    /// </summary>
    public static class GameUi
    {
        public static bool IsBlocking { get; set; }
        public static bool IsPaused { get; set; }
    }
}
