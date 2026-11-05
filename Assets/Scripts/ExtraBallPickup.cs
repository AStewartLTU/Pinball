using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class ExtraBallPickup : MonoBehaviour
{
    private PinballGameManager manager;
    private ExtraBallSpawner spawner;
    private bool collected;

    public void Initialize(PinballGameManager owner, ExtraBallSpawner source)
    {
        manager = owner;
        spawner = source;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponentInParent<PinballBall>() == null) return;
        collected = true;
        if (manager != null) manager.GiveExtraBall();
        if (spawner != null) spawner.Collected(this);
        Destroy(gameObject);
    }
}
