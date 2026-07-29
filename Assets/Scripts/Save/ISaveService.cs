namespace DungeonOdyssey.Save
{
    public interface ISaveService
    {
        bool Exists(int slotIndex);
        void Save(SaveData data);
        SaveData Load(int slotIndex);
        void Delete(int slotIndex);
    }
}
