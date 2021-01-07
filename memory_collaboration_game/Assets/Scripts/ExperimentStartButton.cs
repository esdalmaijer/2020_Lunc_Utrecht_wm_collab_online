using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentStartButton : MonoBehaviour
{
    [SerializeField] bool isStartButton;
    [SerializeField] TextMesh buttonText;
    [SerializeField] string nextScene;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Start the next scene, but only if we're in fullscreen mode.
        if (isStartButton)
        {
            if (Screen.fullScreen)
            {
                if (buttonText.color.r != 1.0f)
                {
                    buttonText.color = new Color(1.0f, 1.0f, 1.0f);
                }
            }
            else
            {
                if (buttonText.color.r != 0.7f)
                {
                    buttonText.color = new Color(0.7f, 0.7f, 0.7f);
                }
            }
        }
    }

    // Button action.
    void OnMouseDown()
    {
        if (isStartButton)
        {
            // Start the next scene, but only if we're in fullscreen mode.
            if (Screen.fullScreen)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
            }
        }
        else
        {
            ToggleFullscreen();
        }
    }

    // Fullscreen function.
    void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
