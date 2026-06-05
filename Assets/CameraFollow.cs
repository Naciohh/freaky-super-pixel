using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera")]
    public Vector3 offset = new Vector3(-18f, 20f, -18f);
    public float followSpeed = 5f;

    [Header("Collision")]
    public float collisionOffset = 0.5f;
    public LayerMask collisionLayers;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        Vector3 direction =
            (desiredPosition - target.position).normalized;

        float distance =
            Vector3.Distance(target.position, desiredPosition);

        // Buscar obstaculos entre el jugador y la camara, ignorando al propio
        // jugador (y sus hijos) para que la camara no se pegue sobre el target.
        RaycastHit[] hits = Physics.RaycastAll(
            target.position,
            direction,
            distance,
            collisionLayers,
            QueryTriggerInteraction.Ignore);

        float closest = distance;
        foreach (var h in hits)
        {
            if (h.collider.transform == target ||
                h.collider.transform.IsChildOf(target))
                continue;

            if (h.distance < closest)
            {
                closest = h.distance;
                desiredPosition = h.point - direction * collisionOffset;
            }
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // SIEMPRE misma rotación isométrica
        transform.rotation =
            Quaternion.Euler(45f, 45f, 0f);
    }
}