using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerTarget;
    public float followSpeed = 3f;
    public Vector3 cameraOffset = new Vector3(0f, 2f, -5f);
    
    private Vector3 initialOffset;

    void Start()
    {
        // Начальное смещение камеры
        initialOffset = cameraOffset;
    }

    void LateUpdate()
    {
        if (playerTarget == null) 
        {
            Debug.LogWarning("PlayerCamera: Player не определен!");
            return;
        }
        
        Vector3 targetPosition = playerTarget.position + initialOffset;
        
        // Плавное движение камеры
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Камера всегда смотрит на игрока
        transform.LookAt(playerTarget);
    }
}