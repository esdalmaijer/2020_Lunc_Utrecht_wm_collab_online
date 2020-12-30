using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingConfirmButton : MonoBehaviour
{
    // Allow the parent object to be assigned by the parent, by declaring
    // it public.
    public MemGameManager parent;

    // Declare the sprite renderer (will be assigned in Start).
    private SpriteRenderer sprite;

    // Sync a click on this object with a parent function.
    private void OnMouseDown()
    {
        Debug.Log("CLICK!");
        // Start the clickanimation.
        StartCoroutine(ClickAnimation(finished => { 
            // Confirm the rating in the MemGameManager.
            parent.RatingConfirmation();
        }));
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the SpriteRenderer.
        sprite = this.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Click animation.
    private IEnumerator ClickAnimation(System.Action<bool> finished)
    {
        // Set the colour to yellow.
        sprite.color = new Color(1.0f, 1.0f, 0.0f);
        // Wait for a bit.
        yield return new WaitForSeconds(0.2f);
        // Return the colour to normal.
        sprite.color = new Color(1.0f, 1.0f, 1.0f);
        // Let the calling function know that we're done here.
        finished(true);
    }
}
