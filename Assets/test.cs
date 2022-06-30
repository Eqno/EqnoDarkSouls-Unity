using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    private float gravity = 10, velocity = 0;
    private CharacterController Controller;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Controller.isGrounded) velocity = 0;
        velocity -= gravity * Time.deltaTime;
        
        Controller.Move(new Vector3(3, velocity, 0) * Time.deltaTime);
    }
}
