using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class PinballBumper : MonoBehaviour
{
    [SerializeField, Min(0)] private int points = 100;
    [SerializeField, Min(0f)] private float kickImpulse = 7f;
    private PinballGameManager manager;

    private void Awake() => manager = FindFirstObjectByType<PinballGameManager>();

    private void OnCollisionEnter2D(Collision2D collision) => Hit(collision.collider);
    private void OnTriggerEnter2D(Collider2D other) => Hit(other);

    private void Hit(Collider2D other)
    {
        PinballBall ball = other.GetComponentInParent<PinballBall>();
        if (ball == null) return;
        if (manager != null) manager.AddScore(points);
        Vector2 away = ((Vector2)ball.transform.position - (Vector2)transform.position).normalized;
        ball.GetComponent<Rigidbody2D>().AddForce(away * kickImpulse, ForceMode2D.Impulse);
    }
}
