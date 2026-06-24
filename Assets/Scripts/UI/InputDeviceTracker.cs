using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rastrea si el jugador está usando JOYSTICK o TECLADO/MOUSE, según el último
/// dispositivo con el que interactuó. Los prompts/carteles lo leen para mostrar
/// el botón correcto (ej. "E" vs "X") y cambiar en vivo si se cambia de control.
///
/// Costo: unos pocos chequeos por frame (sticks + botones comunes). Despreciable.
/// Se crea solo (RuntimeInitializeOnLoadMethod) y sobrevive a los cambios de escena.
/// </summary>
public class InputDeviceTracker : MonoBehaviour
{
    public static bool UsingGamepad { get; private set; }

    // Los DualShock4/DualSense por HID pueden inundar de eventos (touchpad/giroscopio) y
    // exceder el presupuesto del Input System, que entonces DESCARTA input. Quitamos el
    // límite (0 = sin tope) para no perder entradas. Se corre antes de cargar la escena.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureInputBudget()
    {
        InputSystem.settings.maxEventBytesPerUpdate = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<InputDeviceTracker>() != null) return;
        var go = new GameObject("InputDeviceTracker");
        DontDestroyOnLoad(go);
        go.AddComponent<InputDeviceTracker>();

        // Estado inicial: si hay pad conectado y no hay teclado, arrancamos en modo pad.
        UsingGamepad = Gamepad.current != null && Keyboard.current == null;
    }

    void Update()
    {
        // Actividad en el joystick → modo pad.
        Gamepad gp = Gamepad.current;
        if (gp != null && GamepadActive(gp))
            UsingGamepad = true;

        // Actividad en teclado → modo teclado.
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame)
            UsingGamepad = false;

        // Actividad en mouse (mover o click) → modo teclado/mouse.
        Mouse m = Mouse.current;
        if (m != null && (m.leftButton.wasPressedThisFrame ||
                          m.rightButton.wasPressedThisFrame ||
                          m.delta.ReadValue().sqrMagnitude > 4f))
            UsingGamepad = false;
    }

    private static bool GamepadActive(Gamepad gp)
    {
        if (gp.leftStick.ReadValue().sqrMagnitude > 0.1f) return true;
        if (gp.rightStick.ReadValue().sqrMagnitude > 0.1f) return true;
        if (gp.leftTrigger.ReadValue() > 0.3f || gp.rightTrigger.ReadValue() > 0.3f) return true;
        return gp.buttonSouth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame
            || gp.buttonWest.wasPressedThisFrame  || gp.buttonNorth.wasPressedThisFrame
            || gp.leftShoulder.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame
            || gp.startButton.wasPressedThisFrame  || gp.selectButton.wasPressedThisFrame
            || gp.dpad.up.wasPressedThisFrame    || gp.dpad.down.wasPressedThisFrame
            || gp.dpad.left.wasPressedThisFrame  || gp.dpad.right.wasPressedThisFrame;
    }
}
