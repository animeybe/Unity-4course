using UnityEngine;
using System.Collections;
using System;

public class RLLaunch : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float shootRange = 20f;
    [SerializeField] [Range(5f, 30f)] private float maxAimAngle = 10f;
    [SerializeField] private Transform launchPoint;

    [Header("Firing")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private float rocketSpeed = 15f;
    [SerializeField] private float shootCooldown = 2f;

    [Header("Poison")]
    [SerializeField] private bool usePoison;
    [SerializeField] [Range(0f, 20f)] private float poisonDPS = 5f;
    [SerializeField] [Range(1f, 10f)] private float poisonDuration = 4f;

    private RLLook lookScript;
    private bool canShoot = true;
    private WaitForSeconds shootDelay;

    void Awake()
    {
        shootDelay = new WaitForSeconds(shootCooldown);
        lookScript = GetComponentInChildren<RLLook>();
    }

    void Update()
    {
        if (canShoot && lookScript?.CanSeePlayerImpublic == true && IsTargetValid())
            StartCoroutine(LaunchRocket());
    }

    private bool IsTargetValid()
    {
        if (lookScript.target == null) return false;
        
        var distance = Vector3.Distance(transform.position, lookScript.target.position);
        var angle = Vector3.Angle(lookScript.rlhead.forward, 
            (lookScript.target.position - lookScript.rlhead.position).normalized);
            
        return distance <= shootRange && angle <= maxAimAngle;
    }

    private IEnumerator LaunchRocket()
    {
        canShoot = false;

        if (rocketPrefab != null && launchPoint != null)
        {
            var rocket = Instantiate(rocketPrefab, launchPoint.position, launchPoint.rotation);
            var rocketRb = rocket.GetComponent<Rigidbody>();
            rocketRb.linearVelocity = launchPoint.forward * rocketSpeed;

            var rocketScript = rocket.GetComponent<FlyingRocket>();
            if (usePoison && rocketScript != null)
                rocketScript.SetPoison(poisonDPS, poisonDuration);
        }

        yield return shootDelay;
        canShoot = true;
    }
}
