using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedJoystick : Joystick
{
    void Awake()
    {
        // ensure fixed joystick is always visible
        gameObject.SetActive(true);
    }

    void Update()
    {
        // if any other script disables it, force enable again
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}
