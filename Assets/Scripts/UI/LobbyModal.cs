using System;
using UnityEngine;

/// <summary>
/// Estado compartido de los "popups" del lobby (panel de controles, selector de
/// dificultad, etc.). Mientras hay al menos uno abierto:
///   - Se congela el movimiento de Emilio (<see cref="PlayerMovement.InputLocked"/>),
///     porque en esa parte ya no se puede "navegar" el escenario.
///   - Se avisa por <see cref="Changed"/> para que la leyenda WASD/Joystick se oculte.
///
/// Cada popup llama <see cref="Open"/> al abrirse y <see cref="Close"/> al cerrarse.
/// Usa un contador para soportar popups encimados sin desbloquear de más.
/// </summary>
public static class LobbyModal
{
    private static int _open;

    /// <summary>True si hay al menos un popup del lobby abierto.</summary>
    public static bool IsOpen => _open > 0;

    /// <summary>Se dispara cuando se pasa de 0→1 (true) o de 1→0 (false) popups.</summary>
    public static event Action<bool> Changed;

    public static void Open()
    {
        _open++;
        if (_open == 1)
        {
            PlayerMovement.InputLocked = true;
            Changed?.Invoke(true);
        }
    }

    public static void Close()
    {
        if (_open <= 0) return;
        _open--;
        if (_open == 0)
        {
            PlayerMovement.InputLocked = false;
            Changed?.Invoke(false);
        }
    }

    /// <summary>Reset duro (por si cambia de escena con un popup abierto).</summary>
    public static void ResetState()
    {
        bool wasOpen = _open > 0;
        _open = 0;
        if (wasOpen) Changed?.Invoke(false);
    }
}
