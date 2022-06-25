using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Variable
    public string KeyUp = "w";
    public string KeyDown = "s";
    public string KeyLeft = "a";
    public string KeyRight = "d";
    
    public float Duration = 0.1f;
    public bool inputEnable = true;

    public float Dup;
    public float Dright;

    private float targetDup;
    private float targetDright;
    private float velocityDup;
    private float velocityDright;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        targetDup = (Input.GetKey(KeyUp)?1:0) - (Input.GetKey(KeyDown)?1:0);
        targetDright = (Input.GetKey(KeyRight)?1:0) - (Input.GetKey(KeyLeft)?1:0);
        if (inputEnable == false) targetDup = targetDright = 0;
        Dup = Mathf.SmoothDamp(Dup, targetDup, ref velocityDup, Duration);
        Dright = Mathf.SmoothDamp(Dright, targetDright, ref velocityDright, Duration);
    }
}
