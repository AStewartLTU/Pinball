using System.Collections;
using UnityEngine;

public sealed class ExtraBallSpawner : MonoBehaviour
{
    [SerializeField] private ExtraBallPickup pickupPrefab;
    [SerializeField] private Vector2 delaySeconds = new Vector2(30f, 60f);
    [SerializeField] private Vector2 lowerLeft = new Vector2(-3.1f, -2.5f);
    [SerializeField] private Vector2 upperRight = new Vector2(2.2f, 4.5f);
    private PinballGameManager manager;
    private ExtraBallPickup current;

    private void Start()
    {
        manager = FindFirstObjectByType<PinballGameManager>();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => manager != null && manager.IsPlaying);
            yield return new WaitForSeconds(Random.Range(delaySeconds.x, delaySeconds.y));
            if (manager == null || !manager.IsPlaying || current != null) continue;
            Vector2 point = new Vector2(Random.Range(lowerLeft.x, upperRight.x), Random.Range(lowerLeft.y, upperRight.y));
            current = Instantiate(pickupPrefab, point, Quaternion.identity);
            current.Initialize(manager, this);
        }
    }

    public void Collected(ExtraBallPickup pickup)
    {
        if (current == pickup) current = null;
    }
}
