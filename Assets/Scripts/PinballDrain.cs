using UnityEngine;

public sealed class PinballDrain : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PinballBall ball = other.GetComponentInParent<PinballBall>();
        if (ball != null) ball.Drain();
    }
}
