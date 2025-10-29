using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    private int coinsCollected = 0;
    
    public void CollectCoin(int value)
    {
        coinsCollected += value;
        Debug.Log($"Coins collected: {coinsCollected}");
    }
    
    public int GetCoinCount()
    {
        return coinsCollected;
    }
}