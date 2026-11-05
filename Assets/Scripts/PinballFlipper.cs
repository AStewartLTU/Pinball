
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PinballFlipper : MonoBehaviour
{
    [SerializeField] private Key key = Key.A;
    [SerializeField] private Key alternateKey = Key.None;

    [SerializeField] private float restAngle = 0f;
    [SerializeField] private float pressedAngle = 45f;

    [SerializeField, Min(1f)]
    private float rotationSpeed = 1000f;

    [SerializeField]
    private Vector2 kickDirection = Vector2.up;

    [SerializeField, Min(0f)]
    private float kickImpulse = 12f;

    private readonly HashSet<PinballBall> ballsKickedThisPress =
        new HashSet<PinballBall>();

    private Rigidbody2D body;
    private bool held;
    private bool wasHeld;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.useFullKinematicContacts = true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            held = false;
            return;
        }

        bool primaryPressed = keyboard[key].isPressed;
        bool alternatePressed = false;

        if (alternateKey != Key.None)
        {
            alternatePressed = keyboard[alternateKey].isPressed;
        }
        else if (key == Key.A)
        {
            alternatePressed = keyboard.leftArrowKey.isPressed;
        }
        else if (key == Key.D)
        {
            alternatePressed = keyboard.rightArrowKey.isPressed;
        }

        held = primaryPressed || alternatePressed;
    }

    private void FixedUpdate()
    {
        if (held && !wasHeld)
        {
            ballsKickedThisPress.Clear();
        }

        wasHeld = held;

        float targetAngle = held ? pressedAngle : restAngle;

        float nextAngle = Mathf.MoveTowardsAngle(
            body.rotation,
            targetAngle,
            rotationSpeed * Time.fixedDeltaTime
        );

        body.MoveRotation(nextAngle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKick(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryKick(collision);
    }

    private void TryKick(Collision2D collision)
    {
        if (!held)
            return;

        PinballBall ball =
            collision.collider.GetComponentInParent<PinballBall>();

        if (ball == null)
            return;

        if (!ballsKickedThisPress.Add(ball))
            return;

        Rigidbody2D ballBody = ball.GetComponent<Rigidbody2D>();

        if (ballBody == null)
            return;

        Vector2 direction = kickDirection.normalized;

        ballBody.AddForce(
            direction * kickImpulse,
            ForceMode2D.Impulse
        );
    }
}
