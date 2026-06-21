using UnityEngine;

/// <summary>
/// Movimiento de Emilio en el menú jugable.
/// WASD relativo a la cámara (W = hacia "arriba" en pantalla) y el personaje
/// gira para mirar hacia donde camina. Sin apuntado con mouse — pensado para
/// pasear por el menú, no para combate.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MenuPlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed         = 14f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Gravedad")]
    [SerializeField] private float gravity       = -20f;
    [SerializeField] private float groundedStick = -2f;

    [Header("Refs")]
    [SerializeField] private Camera   cam;
    [SerializeField] private Animator animator;
    [SerializeField] private string   moveBool = "IsMoving";

    private CharacterController controller;
    private float vy;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        // Direcciones relativas a la cámara, aplanadas al piso.
        Vector3 fwd   = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.transform.right   : Vector3.right;
        fwd.y = 0f;   fwd.Normalize();
        right.y = 0f; right.Normalize();

        Vector3 move = fwd * v + right * h;
        if (move.sqrMagnitude > 1f) move.Normalize();
        bool moving = move.sqrMagnitude > 0.01f;

        if (animator != null) animator.SetBool(moveBool, moving);

        // Mirar hacia donde camina.
        if (moving)
        {
            Quaternion target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        // Gravedad + movimiento.
        if (controller.isGrounded && vy < 0f) vy = groundedStick;
        vy += gravity * Time.deltaTime;
        Vector3 velocity = move * speed + Vector3.up * vy;
        controller.Move(velocity * Time.deltaTime);
    }
}
