using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Estado global de logros (a nivel perfil, NO por slot de guardado).
// Auto-bootstrap + DontDestroyOnLoad, al estilo del resto del proyecto.
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    // Lo escucha el banner para reproducir el video.
    public event Action<AchievementDef> OnUnlocked;

    private readonly HashSet<AchievementId> _unlocked = new HashSet<AchievementId>();
    private int _spiders;     // contador POR PARTIDA (no se persiste)
    private int _skeletons;   // contador POR PARTIDA (no se persiste)

    private string SavePath => Path.Combine(Application.persistentDataPath, "achievements.json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("AchievementManager");
        DontDestroyOnLoad(go);
        go.AddComponent<AchievementManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    public bool IsUnlocked(AchievementId id) => _unlocked.Contains(id);

    public void Unlock(AchievementId id)
    {
        if (_unlocked.Contains(id)) return;
        _unlocked.Add(id);
        Save();
        var def = AchievementCatalog.Get(id);
        Debug.Log($"[Achievement] Unlocked: {def.displayName}");
        OnUnlocked?.Invoke(def);
    }

    // Reinicia los contadores por partida. Lo llama AchievementTriggers al empezar el nivel.
    public void ResetRunCounters()
    {
        _spiders = 0;
        _skeletons = 0;
    }

    public void AddSpiderKill()
    {
        _spiders++;
        if (_spiders >= AchievementCatalog.SpiderGoal) Unlock(AchievementId.ControlDePlagas);
    }

    public void AddSkeletonKill()
    {
        _skeletons++;
        if (_skeletons >= AchievementCatalog.SkeletonGoal) Unlock(AchievementId.SepultureroEnRacha);
    }

    // --- Persistencia ---
    [Serializable]
    private class SaveBlob { public List<string> unlocked = new List<string>(); }

    private void Load()
    {
        _unlocked.Clear();
        try
        {
            if (File.Exists(SavePath))
            {
                var blob = JsonUtility.FromJson<SaveBlob>(File.ReadAllText(SavePath));
                if (blob?.unlocked != null)
                    foreach (var s in blob.unlocked)
                        if (Enum.TryParse(s, out AchievementId id)) _unlocked.Add(id);
            }
        }
        catch (Exception e) { Debug.LogWarning($"[Achievement] Load failed: {e.Message}"); }
    }

    private void Save()
    {
        try
        {
            var blob = new SaveBlob();
            foreach (var id in _unlocked) blob.unlocked.Add(id.ToString());
            File.WriteAllText(SavePath, JsonUtility.ToJson(blob, true));
        }
        catch (Exception e) { Debug.LogWarning($"[Achievement] Save failed: {e.Message}"); }
    }

    // Helper de debug: borra todo el progreso (para testear). Llamar desde un menú o tecla temporal.
    public void DebugResetAll()
    {
        _unlocked.Clear();
        ResetRunCounters();
        Save();
        Debug.Log("[Achievement] Progreso reseteado.");
    }
}
