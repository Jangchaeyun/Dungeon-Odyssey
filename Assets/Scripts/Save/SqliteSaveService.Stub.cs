using UnityEngine;

namespace DungeonOdyssey.Save
{
    /// <summary>
    /// 2차 개발용 스텁. 기본 시스템 완성 후 SQLite 구현으로 교체합니다.
    /// </summary>
    public class SqliteSaveService : ISaveService
    {
        public bool Exists(int slotIndex)
        {
            Debug.LogWarning("[Save] SqliteSaveService는 2차 개발 예정입니다. JsonSaveService를 사용하세요.");
            return false;
        }

        public void Save(SaveData data)
        {
            Debug.LogWarning("[Save] SqliteSaveService는 2차 개발 예정입니다.");
        }

        public SaveData Load(int slotIndex)
        {
            Debug.LogWarning("[Save] SqliteSaveService는 2차 개발 예정입니다.");
            return null;
        }

        public void Delete(int slotIndex)
        {
            Debug.LogWarning("[Save] SqliteSaveService는 2차 개발 예정입니다.");
        }
    }
}
