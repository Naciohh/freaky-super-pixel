using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float normalSpeed = 3.5f;
[SerializeField] private float transformedSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Forms")]
    [SerializeField] private GameObject normalForm;
    [SerializeField] private GameObject transformedForm;

    [Header("Animators")]
    [SerializeField] private Animator normalAnimator;
    [SerializeField] private Animator transformedAnimator;

    private CharacterController controller;

    private bool isTransformed = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        normalForm.SetActive(true);
        transformedForm.SetActive(false);
    }

    void Update()
    {
        HandleMovement();
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

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float currentSpeed = isTransformed ? transformedSpeed : normalSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);
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