using UnityEngine;

public class Flipper : MonoBehaviour
{
    public KeyCode key = KeyCode.LeftArrow;
    public float restAngle = 0f;
    public float activeAngle = 45f;
    public float rotationSpeed = 900f;

    void Update()
    {
        float targetAngle = Input.GetKey(key) ? activeAngle : restAngle;
        float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, angle);


    }








}
