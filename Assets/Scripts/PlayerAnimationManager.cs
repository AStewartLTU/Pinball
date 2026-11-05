using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetHorizontalAxis(float value)
    {
        ApplyHorizontalAxis(value);
    }

    public void SetHorizontalAxis(InputAction.CallbackContext value)
    {
        ApplyHorizontalAxis(value.ReadValue<float>());
    }

    public void SetPunch()
    {
        if (animator != null)
            animator.SetTrigger("Punch");
    }

    private void ApplyHorizontalAxis(float value)
    {
        if (animator != null)
            animator.SetFloat("HorizontalMovement", value);

        if (Mathf.Abs(value) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(value);
            transform.localScale = scale;
        }
    }
}
