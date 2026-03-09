using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arkaplan : MonoBehaviour
{
    public Joystick MoveJoy;
    float HorizontalX;
    public float speed;
    void Update()
    {
        
        string controller = PlayerPrefs.GetString("Controller");
        if (controller == "joystick")
        {
            HorizontalX = MoveJoy.Horizontal;

        }
        else
        {
            HorizontalX = Input.GetAxis("Horizontal");
        }
    }
    void FixedUpdate()
    {

        transform.Translate((transform.right * -HorizontalX * speed) * Time.fixedDeltaTime);

    }
}
