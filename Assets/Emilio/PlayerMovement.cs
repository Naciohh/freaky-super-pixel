using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float normalSpeed = 3.5f;
[SerializeField] private float transformedSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedStick = -2f;

    private float verticalVelocity;

    [Header("Forms")]
    [SerializeField] private GameObject normalForm;
    [SerializeField] private GameObject transformedForm;

    [Header("Animators")]
    [SerializeField] private Animator normalAnimator;
    [SerializeField] private Animator transformedAnimator;

    [Header("Aim")]
    [SerializeField] private Camera aimCamera;

    private CharacterController controller;

    private bool isTransformed = false;

    // Ultima direccion hacia el mouse (horizontal, normalizada). La usa PlayerCombat
    // para orientar el ataque exacto al mouse, sin el retraso del Slerp del cuerpo.
    private Vector3 aimDirection = Vector3.forward;
    public Vector3 AimDirection => aimDirection;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (aimCamera == null) aimCamera = Camera.main;

        normalForm.SetActive(true);
        transformedForm.SetActive(false);
    }

    void Update()
    {
        HandleAiming();
        HandleMovement();
    }

    // Gira al personaje para que mire hacia donde esta el mouse, sin afectar el
    // movimiento. La W/A/S/D siguen moviendolo en world space como siempre.
    void HandleAiming()
    {
        // Twin-stick: si se mueve el stick derecho del joystick, Emilio apunta con él.
        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 r = gp.rightStick.ReadValue();
            if (r.sqrMagnitude > 0.09f)   // deadzone ~0.3
            {
                Vector3 stickAim = CameraRelative(r);
                if (stickAim.sqrMagnitude > 0.001f)
                {
                    aimDirection = stickAim.normalized;
                    RotateTowards(aimDirection);
                }
                return;
            }
        }

        // Fallback: apuntado con el mouse (como siempre).
        if (aimCamera == null) return;

        // Plano horizontal a la altura del jugador.
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (!groundPlane.Raycast(ray, out float enter)) return;

        Vector3 mouseWorld = ray.GetPoint(enter);
        Vector3 lookDir = mouseWorld - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f) return;

        aimDirection = lookDir.normalized;
        RotateTowards(aimDirection);
    }

    // Convierte el input del stick (x=derecha, y=arriba) a una dirección en el piso
    // relativa a la cámara: "arriba" del stick = hacia adentro de la pantalla.
    private Vector3 CameraRelative(Vector2 input)
    {
        Vector3 f = aimCamera != null ? aimCamera.transform.forward : Vector3.forward;
        Vector3 r = aimCamera != null ? aimCamera.transform.right   : Vector3.right;
        f.y = 0f; r.y = 0f;
        f.Normalize(); r.Normalize();
        return r * input.x + f * input.y;
    }

    private void RotateTowards(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void HandleMovement()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        // Stick izquierdo del joystick (se suma al WASD; conserva la magnitud analógica).
        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 l = gp.leftStick.ReadValue();
            if (l.sqrMagnitude > 0.02f) { h += l.x; v += l.y; }
        }

        Vector3 move = new Vector3(h, 0f, v);
        if (move.sqrMagnitude > 1f) move.Normalize();   // clamp a 1 sin matar el analógico
        bool isMoving = move.sqrMagnitude > 0.01f;

        GetCurrentAnimator().SetBool("IsMoving", isMoving);

        // La rotacion del personaje la maneja HandleAiming() (mira al mouse).
        // Aca solo movemos en world-space, sin tocar la rotacion.

        // Gravedad: mantiene al personaje pegado al piso y lo hace bajar de los escalones.
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        verticalVelocity += gravity * Time.deltaTime;

        float currentSpeed = isTransformed ? transformedSpeed : normalSpeed;
        Vector3 velocity = move * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    public void TriggerAnimation(string triggerName)
    {
        var anim = GetCurrentAnimator();
        if (anim != null) anim.SetTrigger(triggerName);
    }

    public void SetTransformed(bool value)
    {
        isTransformed = value;
        normalForm.SetActive(!isTransformed);
        transformedForm.SetActive(isTransformed);
    }

    private Animator GetCurrentAnimator()
    {
        return isTransformed ? transformedAnimator : normalAnimator;
    }
}