using UnityEngine;
using System.Collections;

public class RLLook : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] public Transform rlhead;
    [SerializeField] private Transform rlbase;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 30f;
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 2f;
    
    private bool canSeePlayer = false;
    public bool CanSeePlayerImpublic => canSeePlayer;

    void Update()
    {
        if (target != null)
        {
            canSeePlayer = CanSeePlayer();
            
            if (canSeePlayer)
            {
                // ТОЛЬКО поворот по Y (360° без подъема/опускания)
                Vector3 lookDirection = (target.position - transform.position).normalized;
                lookDirection.y = 0; // Игнорируем высоту!
                
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                
                // ПЛАВНЫЙ поворот только по Y
                rlhead.rotation = Quaternion.RotateTowards(
                    rlhead.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
                
                // База только Y
                Vector3 baseEuler = rlbase.eulerAngles;
                baseEuler.x = 0; baseEuler.z = 0;
                baseEuler.y = rlhead.eulerAngles.y;
                rlbase.rotation = Quaternion.Euler(baseEuler);
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (target == null) return false;
        
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > detectionRange) return false;
        
        // Raycast НА УРОВНЕ ПОЛА (не видит прыжки!)
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f; // Низ турели
        Vector3 targetPoint = new Vector3(target.position.x, rayOrigin.y, target.position.z); // На уровне турели
        Vector3 directionToPlayer = (targetPoint - rayOrigin).normalized;
        
        Debug.DrawRay(rayOrigin, directionToPlayer * detectionRange, Color.green);
        
        if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, detectionRange))
        {
            bool seesPlayer = hit.transform == target;
            Debug.Log($"Turret sees player: {seesPlayer} (hit: {hit.transform?.name})");
            return seesPlayer;
        }
        
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = canSeePlayer ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
