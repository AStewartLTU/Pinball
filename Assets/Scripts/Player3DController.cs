using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private BoxCollider platform;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (platform == null)
        {
            GameObject platformObject = GameObject.Find("Cube");
            if (platformObject != null)
                platform = platformObject.GetComponent<BoxCollider>();
        }

        if (platform != null)
            AlignToPlatform();
    }

    private void AlignToPlatform()
    {
        Bounds bounds = platform.bounds;
        float edgeInset = controller.radius + 0.05f;
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, bounds.min.x + edgeInset, bounds.max.x - edgeInset);
        position.z = Mathf.Clamp(position.z, bounds.min.z + edgeInset, bounds.max.z - edgeInset);
        position.y = bounds.max.y + controller.height * 0.5f - controller.center.y;

        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    private void Update()
    {
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            input.x = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        }

        input = Vector2.ClampMagnitude(input, 1f);
        Vector3 move = new Vector3(input.x, 0f, input.y);
        Vector3 horizontalDisplacement = move * movementSpeed * Time.deltaTime;

        if (platform != null)
        {
            Bounds bounds = platform.bounds;
            float edgeInset = controller.radius + 0.05f;
            Vector3 position = transform.position;
            float nextX = Mathf.Clamp(position.x + horizontalDisplacement.x, bounds.min.x + edgeInset, bounds.max.x - edgeInset);
            float nextZ = Mathf.Clamp(position.z + horizontalDisplacement.z, bounds.min.z + edgeInset, bounds.max.z - edgeInset);
            horizontalDisplacement.x = nextX - position.x;
            horizontalDisplacement.z = nextZ - position.z;
        }

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 displacement = horizontalDisplacement + Vector3.up * (verticalVelocity * Time.deltaTime);
        controller.Move(displacement);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}