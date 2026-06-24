using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Navegación de menús con joystick que complementa al StandaloneInputModule:
/// - D-pad arriba/abajo mueve la selección (las flechas del control).
/// - Botón de abajo (buttonSouth = ✕/Cross en PS, A en Xbox) y R1 confirman.
/// El stick (navegación) y el teclado los sigue manejando el StandaloneInputModule;
/// el submit del pad se maneja acá por Input System (posición correcta en cada control),
/// por eso se sacó "joystick button 0" del eje Submit legacy para no disparar doble.
///
/// Es un singleton que se crea solo (RuntimeInitializeOnLoadMethod) y sobrevive a los
/// cambios de escena, así sirve para todos los menús (pausa, game over, victoria,
/// main menu, selección de dificultad) sin configurarlo en cada escena.
/// </summary>
public class GamepadMenuNavigator : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<GamepadMenuNavigator>() != null) return;
        var go = new GameObject("GamepadMenuNavigator");
        DontDestroyOnLoad(go);
        go.AddComponent<GamepadMenuNavigator>();
    }

    void Update()
    {
        Gamepad gp = Gamepad.current;
        EventSystem es = EventSystem.current;
        if (gp == null || es == null) return;

        bool up   = gp.dpad.up.wasPressedThisFrame;
        bool down = gp.dpad.down.wasPressedThisFrame;

        if (up || down)
            Navigate(es, down);

        // Confirmar con el botón de ABAJO (buttonSouth) = ✕/Cross en PlayStation,
        // A en Xbox, o con R1. Ambos por el Input System → posición correcta en cada
        // control, sin depender del número de botón legacy.
        if (gp.buttonSouth.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame)
            Submit(es);
    }

    private void Navigate(EventSystem es, bool down)
    {
        GameObject current = es.currentSelectedGameObject;

        // Si no hay nada seleccionado (o quedó inactivo), seleccionar el primer botón
        // disponible: así game over / victoria quedan navegables sin tocarlos.
        if (current == null || !current.activeInHierarchy)
        {
            Selectable first = FirstSelectable();
            if (first != null) es.SetSelectedGameObject(first.gameObject);
            return;
        }

        Selectable cur = current.GetComponent<Selectable>();
        if (cur == null) return;

        Selectable next = down ? cur.FindSelectableOnDown() : cur.FindSelectableOnUp();
        if (next != null) es.SetSelectedGameObject(next.gameObject);
    }

    private void Submit(EventSystem es)
    {
        GameObject current = es.currentSelectedGameObject;
        if (current == null || !current.activeInHierarchy) return;
        ExecuteEvents.Execute(current, new BaseEventData(es), ExecuteEvents.submitHandler);
    }

    private static Selectable FirstSelectable()
    {
        var all = Selectable.allSelectablesArray;
        foreach (var s in all)
            if (s != null && s.IsActive() && s.IsInteractable())
                return s;
        return null;
    }
}
