using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// Definición de un logro. Carga sprite/video de Resources de forma perezosa (lazy).
public class AchievementDef
{
    public AchievementId id;
    public string displayName;
    public string spriteResource;   // ruta relativa dentro de una carpeta Resources/, sin extensión
    public string videoResource;

    private Sprite _sprite;
    private VideoClip _video;

    public Sprite Sprite => _sprite != null ? _sprite : (_sprite = Resources.Load<Sprite>(spriteResource));

    // Los logros "solo display" no tienen video: videoResource vacío => null.
    public VideoClip Video
    {
        get
        {
            if (_video != null) return _video;
            if (string.IsNullOrEmpty(videoResource)) return null;
            return _video = Resources.Load<VideoClip>(videoResource);
        }
    }
}

// Tabla estática de los 6 logros + metas de los contadores.
public static class AchievementCatalog
{
    public const int SpiderGoal = 10;     // Control de plagas
    public const int SkeletonGoal = 15;   // Sepulturero en racha

    public const string ContainerResource = "Achievements/Sprites/logros_contenedor";

    // El orden ES el orden de la grilla del popup.
    public static readonly AchievementDef[] All =
    {
        new AchievementDef { id = AchievementId.DeCompras,           displayName = "De compras",           spriteResource = "Achievements/Sprites/logro_mercadito", videoResource = "Achievements/Videos/logro_nivel1" },
        new AchievementDef { id = AchievementId.ControlDePlagas,     displayName = "Control de plagas",    spriteResource = "Achievements/Sprites/logro_aranas",    videoResource = "Achievements/Videos/logro_aranas" },
        new AchievementDef { id = AchievementId.SepultureroEnRacha,  displayName = "Sepulturero en racha", spriteResource = "Achievements/Sprites/logro_esqueleto", videoResource = "Achievements/Videos/logro_esqueletos" },
        new AchievementDef { id = AchievementId.UnOsoWacho,          displayName = "Un oso wacho",         spriteResource = "Achievements/Sprites/logro_oso",       videoResource = "Achievements/Videos/logro_oso" },
        new AchievementDef { id = AchievementId.ElFamosoEasterEgg,   displayName = "El famoso easter egg", spriteResource = "Achievements/Sprites/logro_easteregg", videoResource = "Achievements/Videos/logro_easteregg" },
        new AchievementDef { id = AchievementId.AxelElCapo,          displayName = "Axel el capo",         spriteResource = "Achievements/Sprites/logro_axel",      videoResource = "Achievements/Videos/logro_axel" },

        // Solo display (siempre bloqueados; sin video). videoResource vacío a propósito.
        new AchievementDef { id = AchievementId.CazadorDeOsos,   displayName = "Cazador de osos",   spriteResource = "Achievements/Sprites/logro_cazador",    videoResource = "" },
        new AchievementDef { id = AchievementId.Coleccionista,   displayName = "Coleccionista",     spriteResource = "Achievements/Sprites/logro_logros",     videoResource = "" },
        new AchievementDef { id = AchievementId.MaestroDelParry, displayName = "Maestro del parry", spriteResource = "Achievements/Sprites/logro_parry",      videoResource = "" },
        new AchievementDef { id = AchievementId.ParryAlOso,      displayName = "Parry al oso",      spriteResource = "Achievements/Sprites/logro_parry_boss", videoResource = "" },
        new AchievementDef { id = AchievementId.Pesadilla,       displayName = "Pesadilla",         spriteResource = "Achievements/Sprites/logro_pesadilla",  videoResource = "" },
        new AchievementDef { id = AchievementId.PrimeraSangre,   displayName = "Primera sangre",    spriteResource = "Achievements/Sprites/logro_sangre",     videoResource = "" },
        new AchievementDef { id = AchievementId.Speedrunner,     displayName = "Speedrunner",       spriteResource = "Achievements/Sprites/logro_speedrun",   videoResource = "" },
        new AchievementDef { id = AchievementId.ModoZen,         displayName = "Modo zen",          spriteResource = "Achievements/Sprites/logro_zen",        videoResource = "" },
    };

    private static Dictionary<AchievementId, AchievementDef> _map;

    public static AchievementDef Get(AchievementId id)
    {
        if (_map == null)
        {
            _map = new Dictionary<AchievementId, AchievementDef>();
            foreach (var d in All) _map[d.id] = d;
        }
        return _map[id];
    }
}
