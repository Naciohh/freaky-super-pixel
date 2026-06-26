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
    public VideoClip Video => _video != null ? _video : (_video = Resources.Load<VideoClip>(videoResource));
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
