using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class PinballBall : MonoBehaviour
{
    private const float OutOfBoundsY = -7f;
    [SerializeField, Min(0f)] private float launchImpulse = 14f;
    [SerializeField, Min(1f)] private float maxSpeed = 24f;
    private Rigidbody2D body;
    private PinballGameManager manager;
    private bool launched;
    private bool drained;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 1f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Initialize(PinballGameManager owner)
    {
        manager = owner;
        launched = false;
        drained = false;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void Update()
    {
        if (!launched && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            launched = true;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.AddForce(new Vector2(-0.38f, 1f).normalized * launchImpulse, ForceMode2D.Impulse);
        }
    }

    private void FixedUpdate()
    {
        // The drain trigger catches normal losses; this catches balls that miss it and
        // would otherwise disappear below the camera without spending a life or respawning.
        if (body.bodyType == RigidbodyType2D.Dynamic && transform.position.y <= OutOfBoundsY)
        {
            Drain();
            return;
        }

        if (body.bodyType == RigidbodyType2D.Dynamic && body.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
    }

    public void Drain()
    {
        if (drained) return;
        drained = true;
        if (manager != null) manager.DrainBall(this);
        else Destroy(gameObject);
    }
}

