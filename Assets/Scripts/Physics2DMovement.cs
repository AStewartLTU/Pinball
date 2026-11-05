using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Physics2DMovement : MonoBehaviour
{
    //Variables
    //A reference to the rigidbody of this object
    public Rigidbody reggiebody;
    
    //The variable telling this script how much force to apply to the object
    public float jumpForce = 10.0f;

    public PlayerSound playerSoundManager;


    private void Awake()
    {

        playerSoundManager = this.gameObject.GetComponent<PlayerSound>();

    }
    private void ChangeMovement(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<float>();


    }

    
    public float speed = 2.0f;

    private void MovementFunction(float horizontalScalar, float zValue)
    {
        reggiebody.linearVelocity =
           // Whenever we set the value of a vector, we can either manipulate its components separately or not
           // we need to "make a new vector" with the new values
           new Vector3(horizontalScalar * speed, //This means that the velocity of the 
           reggiebody.linearVelocity.y, zValue); ; // object in X will  be equal to something -speed and +speed
                                            // depeding on the player input

    }
    private void Update()
    {
        reggiebody.linearVelocity =
            // Whenever we set the value of a vector, we can either manipulate its components separately or not
            // we need to "make a new vector" with the new values
            new Vector3(horizontalMovement * speed  , //This means that the velocity of the 
            reggiebody.linearVelocity.y, 0); // object in X will  be equal to something -speed and +speed
                                             // depeding on the player input

        MovementFunction(horizontalMovement, 0);

    }

  
    //This value, which will always be within -1 and 1 will tell us the direction of the movement
    public PlayerInput playerInput;
    public float horizontalMovement = 0;
    public Transform jumpPoint;
    public float jumpRayLength;

    public void Jump()
    {

        Debug.DrawRay(jumpPoint.position, Vector3.down * jumpRayLength, Color.red, 0.1f);

        // Raycasting lets us shoot a "ray" and see if we hit something
        // the origin of the raycast (where its coming from)
        //The direction
        //The length
        RaycastHit hit;
        if (Physics.Raycast(jumpPoint.position, Vector3.down, out hit, jumpRayLength))
        {
            if (hit.transform.gameObject.CompareTag("Cube Floor"))
            {
                reggiebody.AddForce(Vector3.up * jumpForce);


                Debug.Log("Jumping");



                //When you get here, end the function and return
                return;

            }
        }
        Debug.Log("No floor to jump on!");

        

      


    }
}

