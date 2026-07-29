using System.IO;
using UnityEngine;

namespace DungeonOdyssey.Save
{
    /// <summary>
    /// 1차 개발용 JSON 저장. 2차에서 SQLite 구현체로 교체 예정.
    /// </summary>
    public class JsonSaveService : ISaveService
    {
        private readonly string _rootPath;

        public JsonSaveService()
        {
            _rootPath = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        public bool Exists(int slotIndex)
        {
            return File.Exists(GetPath(slotIndex));
        }

        public void Save(SaveData data)
        {
            data.lastSavedAtUtc = System.DateTime.UtcNow.ToString("o");
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetPath(data.slotIndex), json);
            Debug.Log($"[Save] JSON saved → {GetPath(data.slotIndex)}");
        }

        public SaveData Load(int slotIndex)
        {
            var path = GetPath(slotIndex);
            if (!File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public void Delete(int slotIndex)
        {
            var path = GetPath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private string GetPath(int slotIndex)
        {
            return Path.Combine(_rootPath, $"slot_{slotIndex}.json");
        }
    }
}
