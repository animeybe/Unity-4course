using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerTarget;
    public float followSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, 5f, 0f);
    
    private Vector3 initialOffset;

    void Start()
    {
        // Запоминаем начальное смещение
        initialOffset = cameraOffset;
    }

    void LateUpdate()
    {
        if (playerTarget == null) 
        {
            Debug.LogWarning("PlayerCamera: No player assigned!");
            return;
        }
        
        // Целевая позиция камеры - используем МИРОВЫЕ координаты вместо локальных
        // Это предотвратит вращение камеры вместе с игроком
        Vector3 targetPosition = playerTarget.position + initialOffset;
        
        // Плавное движение с LERP
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Камера всегда смотрит на игрока, но не вращается с ним
        transform.LookAt(playerTarget);
        
        // ФИКСИРУЕМ ПОВОРОТ КАМЕРЫ - ВСЕГДА СВЕРХУ
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}