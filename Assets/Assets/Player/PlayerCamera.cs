using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerTarget;
    public float followSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, 5f, 0f);
    
    private Vector3 initialOffset;

    void Start()
    {
        initialOffset = cameraOffset;
    }

    void LateUpdate()
    {
        if (playerTarget == null) 
        {
            Debug.LogWarning("PlayerCamera: No player assigned!");
            return;
        }
        
        Vector3 targetPosition = playerTarget.position + initialOffset;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        transform.LookAt(playerTarget);
        
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}