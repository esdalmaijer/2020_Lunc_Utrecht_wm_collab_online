using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameExplanationButton : MonoBehaviour
{
    [SerializeField] GameExplanationController controller;
    [SerializeField] bool isNext;

    private SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Sprite component.
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Behaviour on mouse click.
    private void OnMouseDown()
    {
        // Start the click animation.
        StartCoroutine(ClickAnimation());

        // Perform the click action.
        if (isNext)
        {
            controller.NextPage();
        }
        else
        {
            controller.PreviousPage();
        }
    }

    // Click animation.
    private IEnumerator ClickAnimation()
    {
        // Change the colour.
        sprite.color = new Color(1.0f, 1.0f, 0.0f);

        // Wait for a wee bit.
        yield return new WaitForSeconds(0.3f);

        // Change the colour back.
        sprite.color = new Color(1.0f, 1.0f, 1.0f);
    }
}
