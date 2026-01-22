using UnityEngine;
using System.Collections;

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
    [SerializeField] private bool usePoison = true;           // ВКЛ/ВЫКЛ яд
    [SerializeField] private float poisonDPS = 8f;     // DPS яда
    [SerializeField] private float poisonDuration = 5f; // Длительность

    private RLLook lookScript;
    private bool canShoot = true;
    private WaitForSeconds shootDelay;

    void Awake()
    {
        shootDelay = new WaitForSeconds(shootCooldown);
        lookScript = GetComponentInChildren<RLLook>();
        
        // ПРОВЕРКА безопасности
        if (rocketPrefab == null) Debug.LogError("RLLaunch: rocketPrefab НЕ НАЗНАЧЕН!");
        if (launchPoint == null) Debug.LogError("RLLaunch: launchPoint НЕ НАЗНАЧЕН!");
    }

    void Update()
    {
        // Стреляет только при точном прицеле
        if (canShoot && lookScript?.CanSeePlayerImpublic == true && IsTargetValid())
        {
            StartCoroutine(LaunchRocket());
        }
    }

    private bool IsTargetValid()
    {
        if (lookScript.target == null || launchPoint == null) return false;
        
        float distance = Vector3.Distance(transform.position, lookScript.target.position);
        float angle = Vector3.Angle(lookScript.rlhead.forward, 
            (lookScript.target.position - lookScript.rlhead.position).normalized);
        
        return distance <= shootRange && angle <= maxAimAngle;
    }

    private IEnumerator LaunchRocket()
    {
        canShoot = false;

        if (rocketPrefab != null && launchPoint != null)
        {
            // Создание ракеты
            GameObject rocket = Instantiate(rocketPrefab, launchPoint.position, launchPoint.rotation);
            Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();
            if (rocketRb != null)
                rocketRb.linearVelocity = launchPoint.forward * rocketSpeed;

            // ПЕРЕДАЧА ПАРАМЕТРОВ ЯДА
            FlyingRocket rocketScript = rocket.GetComponent<FlyingRocket>();
            if (rocketScript != null && usePoison)
            {
                rocketScript.SetPoison(poisonDPS, poisonDuration);
                Debug.Log($"Яд передан ракете: {poisonDPS} DPS на {poisonDuration}s");
            }
        }

        yield return shootDelay;
        canShoot = true;
    }
}
