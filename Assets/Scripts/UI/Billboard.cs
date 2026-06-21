using UnityEngine;

/// <summary>
/// Hace que el objeto (un cartel sprite) siempre mire a la cámara, quedando
/// paralelo a la pantalla (efecto cartel/holograma). Si no se asigna cámara,
/// usa Camera.main.
/// </summary>
[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    void LateUpdate()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        // Alinear con la rotación de la cámara → el sprite queda siempre de frente
        // y el texto se lee derecho, sin espejarse.
        transform.rotation = cam.transform.rotation;
    }

    public void SetCamera(Camera cam) => targetCamera = cam;
}
