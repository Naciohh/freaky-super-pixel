using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Encuadre")]
    [Tooltip("Distancia de la camara al jugador.")]
    public float distance = 22f;
    [Tooltip("Altura del punto al que mira la camara (centra a Emilio en pantalla).")]
    public float focusHeight = 1.5f;
    public float followSpeed = 5f;

    [Header("Angulo")]
    [Tooltip("Inclinacion vertical. Mas bajo = mas hacia el horizonte. Mas alto = mas picado.")]
    public float pitchAngle = 24f;
    [Tooltip("Giro horizontal. 45 = isometrica clasica.")]
    public float yawAngle = 45f;

    [Header("Collision")]
    public float collisionOffset = 0.5f;
    public LayerMask collisionLayers;

    void LateUpdate()
    {
        if (target == null) return;

        // Punto al que mira la camara: el jugador, un poco por encima del piso.
        Vector3 focusPoint = target.position + Vector3.up * focusHeight;

        // El angulo define hacia donde mira; la posicion se deriva de ahi para
        // que el jugador SIEMPRE quede centrado.
        Quaternion rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        Vector3 back = rotation * Vector3.back;

        Vector3 desiredPosition = focusPoint + back * distance;

        // Buscar obstaculos entre el jugador y la camara, ignorando al propio
        // jugador (y sus hijos) para que la camara no se pegue sobre el target.
        RaycastHit[] hits = Physics.RaycastAll(
            focusPoint,
            back,
            distance,
            collisionLayers,
            QueryTriggerInteraction.Ignore);

        foreach (var h in hits)
        {
            if (h.collider.transform == target ||
                h.collider.transform.IsChildOf(target))
                continue;

            if (h.distance < distance)
            {
                desiredPosition = h.point - back * collisionOffset;
                break;
            }
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // La camara mira al jugador con el angulo configurado.
        transform.rotation = rotation;
    }

    /// <summary>
    /// Coloca la camara al instante en su encuadre sobre el target, sin el lerp
    /// de seguimiento. Se usa al volver de una cinematica para que no "salte"
    /// deslizandose desde su posicion anterior.
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 focusPoint = target.position + Vector3.up * focusHeight;
        Quaternion rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        Vector3 back = rotation * Vector3.back;
        Vector3 desiredPosition = focusPoint + back * distance;

        RaycastHit[] hits = Physics.RaycastAll(
            focusPoint, back, distance, collisionLayers, QueryTriggerInteraction.Ignore);

        foreach (var h in hits)
        {
            if (h.collider.transform == target ||
                h.collider.transform.IsChildOf(target))
                continue;

            if (h.distance < distance)
            {
                desiredPosition = h.point - back * collisionOffset;
                break;
            }
        }

        transform.SetPositionAndRotation(desiredPosition, rotation);
    }
}
