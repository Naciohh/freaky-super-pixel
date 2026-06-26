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

    [Header("Encuadre fijo")]
    [Tooltip("Si esta activo, la camara NO sigue al jugador: queda fija en su posicion/rotacion " +
             "actual de la escena, enmarcando todo el cuarto. Usado en el lobby.")]
    public bool staticFraming = false;

    [Header("Orbit anti-pared")]
    [Tooltip("Si una pared taparia la vista, la camara ORBITA alrededor del jugador (gira el yaw) " +
             "hasta encontrar un angulo libre, en vez de acercarse. Mantiene la distancia. " +
             "OJO: la rotacion puede marear en cuartos chicos; preferir clampToRoom.")]
    public bool orbitAroundObstacles = false;
    [Tooltip("Velocidad (grados/seg) con la que gira hacia el angulo libre.")]
    public float yawTurnSpeed = 120f;

    [Header("Clamp al cuarto (sin rotar)")]
    [Tooltip("Mantiene la camara dentro de los limites del cuarto (en X/Z) para que no atraviese " +
             "paredes ni se vea el exterior, SIN rotar. Cerca de una pared la vista se ajusta sola. " +
             "Pensado para usar con un pitch alto (picado) en el lobby.")]
    public bool clampToRoom = false;
    [Tooltip("Limite minimo (x,z) donde puede estar la camara.")]
    public Vector2 roomMin = new Vector2(-34f, -46f);
    [Tooltip("Limite maximo (x,z) donde puede estar la camara.")]
    public Vector2 roomMax = new Vector2(12f, -1f);

    // Yaw actual (puede diferir de yawAngle cuando esta orbitando para esquivar una pared).
    private float _currentYaw;
    private bool _yawInit;

    void LateUpdate()
    {
        if (staticFraming) return;   // camara fija: se queda donde la dejo la escena
        if (target == null) return;

        // Punto al que mira la camara: el jugador, un poco por encima del piso.
        Vector3 focusPoint = target.position + Vector3.up * focusHeight;

        if (!_yawInit) { _currentYaw = yawAngle; _yawInit = true; }

        // Buscar el yaw libre mas cercano y girar suave hacia el (orbitar la pared).
        float targetYaw = orbitAroundObstacles ? FindClearYaw(focusPoint) : yawAngle;
        _currentYaw = Mathf.MoveTowardsAngle(_currentYaw, targetYaw, yawTurnSpeed * Time.deltaTime);

        Quaternion rotation = Quaternion.Euler(pitchAngle, _currentYaw, 0f);
        Vector3 back = rotation * Vector3.back;
        Vector3 desiredPosition = focusPoint + back * distance;

        if (clampToRoom)
        {
            // Solo limitamos X/Z (NO la altura): cerca de una pared la camara queda
            // arriba y cerca, y al mirar siempre a Emilio se inclina sola en picado.
            // No atraviesa paredes ni muestra el exterior, y NO rota en compas (no marea).
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, roomMin.x, roomMax.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, roomMin.y, roomMax.y);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(focusPoint - transform.position);
        }
        else
        {
            // Red de seguridad: si quedara tapada, acerca la camara para no atravesar la pared.
            bool blocked = false;
            if (Physics.SphereCast(
                    focusPoint, collisionOffset, back,
                    out RaycastHit nearest, distance, collisionLayers, QueryTriggerInteraction.Ignore)
                && nearest.collider.transform != target
                && !nearest.collider.transform.IsChildOf(target))
            {
                desiredPosition = focusPoint + back * Mathf.Max(nearest.distance, 0f);
                blocked = true;
            }

            if (blocked)
                transform.position = desiredPosition;
            else
                transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            transform.rotation = rotation;
        }
    }

    // Devuelve el yaw libre mas cercano a yawAngle. Prueba el yaw base y luego
    // desvios crecientes a ambos lados, con histeresis hacia el lado donde ya
    // esta orbitando para evitar que la camara oscile.
    private float FindClearYaw(Vector3 focusPoint)
    {
        if (!IsYawBlocked(focusPoint, yawAngle)) return yawAngle;

        int preferred = Mathf.DeltaAngle(yawAngle, _currentYaw) >= 0f ? 1 : -1;
        for (int step = 1; step <= 18; step++)
        {
            float off = step * 10f;
            if (!IsYawBlocked(focusPoint, yawAngle + preferred * off)) return yawAngle + preferred * off;
            if (!IsYawBlocked(focusPoint, yawAngle - preferred * off)) return yawAngle - preferred * off;
        }
        return _currentYaw; // sin angulo libre: mantiene el actual (la red de seguridad evita el clip)
    }

    private bool IsYawBlocked(Vector3 focusPoint, float yaw)
    {
        Vector3 back = Quaternion.Euler(pitchAngle, yaw, 0f) * Vector3.back;
        if (Physics.SphereCast(
                focusPoint, collisionOffset, back,
                out RaycastHit h, distance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform == target || h.collider.transform.IsChildOf(target)) return false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Coloca la camara al instante en su encuadre sobre el target, sin el lerp
    /// de seguimiento. Se usa al volver de una cinematica para que no "salte"
    /// deslizandose desde su posicion anterior.
    /// </summary>
    public void SnapToTarget()
    {
        if (staticFraming) return;   // en modo fijo no se reposiciona
        if (target == null) return;

        Vector3 focusPoint = target.position + Vector3.up * focusHeight;

        _currentYaw = orbitAroundObstacles ? FindClearYaw(focusPoint) : yawAngle;
        _yawInit = true;
        Quaternion rotation = Quaternion.Euler(pitchAngle, _currentYaw, 0f);
        Vector3 back = rotation * Vector3.back;
        Vector3 desiredPosition = focusPoint + back * distance;

        if (Physics.SphereCast(
                focusPoint, collisionOffset, back,
                out RaycastHit nearest, distance, collisionLayers, QueryTriggerInteraction.Ignore)
            && nearest.collider.transform != target
            && !nearest.collider.transform.IsChildOf(target))
        {
            desiredPosition = focusPoint + back * Mathf.Max(nearest.distance, 0f);
        }

        transform.SetPositionAndRotation(desiredPosition, rotation);
    }
}
