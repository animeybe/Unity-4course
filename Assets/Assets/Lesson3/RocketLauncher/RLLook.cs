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
                Vector3 lookDirection = (target.position - transform.position).normalized;
                lookDirection.y = 0;
                
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                
                rlhead.rotation = Quaternion.RotateTowards(
                    rlhead.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
                
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
        
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        Vector3 targetPoint = new Vector3(target.position.x, rayOrigin.y, target.position.z);
        Vector3 directionToPlayer = (targetPoint - rayOrigin).normalized;
        
        Debug.DrawRay(rayOrigin, directionToPlayer * detectionRange, Color.green);
        
        if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, detectionRange))
        {
            bool seesPlayer = hit.transform == target;
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
