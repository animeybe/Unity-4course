using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    
    // Настройки анимации
    public float rotationSpeed = 180f; // Скорость вращения (градусов в секунду)
    public float floatHeight = 0.5f;   // Высота плавания
    public float floatSpeed = 2f;      // Скорость плавания
    
    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        // Запоминаем начальную позицию
        startPosition = transform.position;
        
        // Случайное смещение для разнообразия анимации
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
        
        // Поворачиваем монету ребром (на 90 градусов вокруг Z)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        // Вращение вокруг оси Y
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        
        // Плавающее движение вверх-вниз
        FloatAnimation();
    }

    void FloatAnimation()
    {
        // Плавное движение вверх-вниз using sine wave
        float newY = startPosition.y + Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinCollector collector = other.GetComponent<CoinCollector>();
            if (collector != null)
            {
                collector.CollectCoin(value);
            }
            Destroy(gameObject);
        }
    }
}