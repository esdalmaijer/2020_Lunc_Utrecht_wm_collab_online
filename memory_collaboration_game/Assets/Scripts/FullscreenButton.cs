using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenButton : Button
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Fullscreen on click.
    void OnPointerDown()
    {
        // Toggle fullscreen mode.
        Screen.fullScreen = !Screen.fullScreen;
    }

    void OnClick()
    {
        // Toggle fullscreen mode.
        Screen.fullScreen = !Screen.fullScreen;
    }
}
