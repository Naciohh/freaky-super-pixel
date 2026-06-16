using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public const int SlotCount = 3;
    private const string LastSlotKey = "LastSaveSlot";

    public static string SlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");

    public static void Save(int slot, SaveData data)
    {
        if (data == null || slot < 0 || slot >= SlotCount) return;

        // Serializamos el SaveData completo (con enemigos, oleada, boss, etc.),
        // solo completando los metadatos.
        data.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        data.slotLabel = $"Ranura {slot + 1}";

        File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, true));
        PlayerPrefs.SetInt(LastSlotKey, slot);
        PlayerPrefs.Save();
    }

    public static SaveData Load(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return null;
        string path = SlotPath(slot);
        if (!File.Exists(path)) return null;
        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveManager: no se pudo cargar slot {slot}: {e.Message}");
            return null;
        }
    }

    public static SaveData LoadLast()
    {
        if (!PlayerPrefs.HasKey(LastSlotKey)) return null;
        int slot = PlayerPrefs.GetInt(LastSlotKey);
        if (slot < 0 || slot >= SlotCount) return null;
        return Load(slot);
    }

    public static bool HasAnySave()
    {
        for (int i = 0; i < SlotCount; i++)
            if (File.Exists(SlotPath(i))) return true;
        return false;
    }

    public static void Delete(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return;
        string path = SlotPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }
}
