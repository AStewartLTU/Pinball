//This tells the file what libraries we have access to
using UnityEngine;
//V This means we're using V this library
//                              V This tells us that we ended the line
// This tells us that we're defining a new MonoBehavior class named "CloudScript"
//                          V This should be the only word to change when you make a script
using System.Collections.Generic;

public class CloudScript : MonoBehaviour
{
    //V This tells Unity that the variable is "public" to other scripts
    // For us, that also means that it'll be visible in the Inspectior tab
    //   V This tells us the type of the variable. What kind of information it can hold
    //      Float is the type for decimal numbers
    //      V This is the "name" of the variable
    //          Which is that word we use to get its value or reference it
    //                 V We use an = sign to give this variable an "inital" value
    //                      V This is the value of the variable. Self explanatory
    //                          V Don't forget the semicolon
    public float Speed = 0.0f;

    

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Display the message "Hello!" on the console
        Debug.Log("Hello!");
    }

    // Update is called once per frame
    void Update()
    {
        //V Create a Vector3
        //      V called Pos
        //        V that is equal to
        //             The current position, of the transform
        //              in the same game object
        Vector3 Pos = this.gameObject.transform.position;
        //Set the value of Pos in X to be equal to Pos.x + Speed
        Pos.x = Pos.x + Speed;
        //Pos.x += Speed;

        //Update the position of the transform with the new value
        this.gameObject.transform.position = Pos;
    }
}
