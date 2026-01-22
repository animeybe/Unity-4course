using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float rotationSpeed = 180f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    public AudioClip collectSound;
    
    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        randomOffset = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        FloatAnimation();
    }

    void FloatAnimation()
    {
        float newY = startPosition.y + Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(CollectCoin(other));
        }
    }

    IEnumerator CollectCoin(Collider player)
    {
        // ✅ ЗВУК ИГРАЕТ ПЕРВЫМ через AudioSource.PlayClipAtPoint
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, 0.7f);
        }
        
        // ВИЗУАЛЬНО исчезает
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        // Ждем окончания звука
        yield return new WaitForSeconds(collectSound != null ? collectSound.length : 0.3f);
        
        // CoinCollector получит событие через свой OnTriggerEnter
        Destroy(gameObject);
    }
}
