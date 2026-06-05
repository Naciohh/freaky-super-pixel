using UnityEngine;

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
        if (aimCamera == null) return;

        // Plano horizontal a la altura del jugador.
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (!groundPlane.Raycast(ray, out float enter)) return;

        Vector3 mouseWorld = ray.GetPoint(enter);
        Vector3 lookDir = mouseWorld - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
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

        Vector3 move = new Vector3(h, 0f, v).normalized;
        bool isMoving = move.magnitude > 0.01f;

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