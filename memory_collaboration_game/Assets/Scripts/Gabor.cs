using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;

public class Gabor : MonoBehaviour
{
    // GUI VARIABLES
    [SerializeField] bool masked = false;
    [SerializeField] bool claimable = false;
    [SerializeField] bool rotatable = false;
    [SerializeField] Color selectionColour = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    // VARIABLES
    private bool isInitialised = false;
    private bool isClaimed = false;
    private int owner;
    private bool rotationState = false;
    private SpriteRenderer gaborSprite;
    private GameObject noise;
    private SpriteRenderer noiseSprite;
    private GameObject highlight;
    private SpriteRenderer highlightSprite;
    private GameObject selectionRing;
    private SpriteRenderer selectionRingSprite;
    private Color thisPlayerColour = new Color(1.0f, 1.0f, 1.0f, 0.3f);

    // Start is called before the first frame update.
    void Start()
    {
        if (isInitialised == false)
        {
            Init();
        }
    }

    public void Init()
    {
        // Find the sprite renderer for this Gabor.
        gaborSprite = GetComponent<SpriteRenderer>();

        // Find the "noise" sprite that forms the background of this Gabor.
        noise = transform.Find("noise").gameObject;
        noiseSprite = noise.GetComponent<SpriteRenderer>();
        // Mask the Gabor if necessary.
        SetMasked(masked);

        // Find the "highlight" sprite that forms the background of this Gabor.
        highlight = transform.Find("highlight").gameObject;
        highlightSprite = highlight.GetComponent<SpriteRenderer>();
        highlightSprite.color = thisPlayerColour;
        // Set the colour to transparent, so that the highlight is invisible at 
        // the start.
        SetClaimed(false);

        // Find the "selection" sprite that forms the background of this Gabor.
        selectionRing = transform.Find("selection").gameObject;
        selectionRingSprite = selectionRing.GetComponent<SpriteRenderer>();
        selectionRingSprite.color = selectionColour;
        // Make the ring invisible.
        SetSelection(false);

        // Set the initialised bool.
        isInitialised = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the stimulus rotation if necessary.
        if (rotationState == true & rotatable == true) {
            // Get the position of the mouse cursor.
            var mouse = Input.mousePosition;
            // Get the position of this GameObject.
            var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
            // Compute the horizontal and vertical difference between the
            // positions of the cursor and this object.
            var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
            // Compute the angle between the cursor and this object.
            var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            // Set the angle of this object to point towards the cursor.
            SetOrientation(angle);
        }
    }

    // FUNCTIONS FOR INTERACTING WITH GABOR.
    // On MouseDown, set the rotation mode ON, and/or claim item.
    public void OnMouseDown()
    {
        if (claimable == true)
        {
            // Claim on behalf of the current player (player nr 0).
            bool claimSuccess = Claim(0, thisPlayerColour);
        }

        // Set the rotation state to True.
        if (rotatable == true)
        {
            rotationState = true;
            // Automatically unmask.
            if (masked)
            {
                SetMasked(false);
            }
        }
    }

    // On MouseUp, set the rotation mode OFF.
    public void OnMouseUp()
    {
        // Set the rotation state to False.
        if (rotatable == true)
        {
            rotationState = false;
        }
    }

    // Claim can be called to claim the Gabor.
    public bool Claim(int player, Color playerColour)
    {
        // Start with a default value for success.
        bool success = false;

        // Check if the item is already claimed.
        if (isClaimed == false)
        {
            // Set the item's claimed status to True.
            isClaimed = true;
            // Set the owner to the player who claimed the item.
            owner = player;
            // Set the highlight colour to the claiming player's colour.
            SetHighlightColour(playerColour);
            // Set success to True.
            success = true;
        }
        // Return the result of claiming.
        return success;
    }

    // FUNCTIONS FOR READING INFO FROM GABOR.
    public bool GetClaimed()
    {
        return isClaimed;
    }

    public bool GetMasked()
    {
        return masked;
    }

    public float GetOrientation()
    {
        // Get the rotation (in radians) along the z-axis.
        float angle = Mathf.Acos(transform.rotation.z) * 2;
        // Convert the rotation to degrees.
        angle *= Mathf.Rad2Deg;
        // Convert to the space we use for mouse-directed angle.
        // This is 0 on the right, -90 on the bottom, -180 and 180 on the left,
        // and 90 on top.
        angle = 180.0f - angle;
        return angle;
    }

    public (int, int) GetPosition()
    {
        // Get the current position (in the world).
        Vector3 myPos = transform.position;

        // Compute the pixel-to-world conversion.
        float yWorldUnits = Camera.main.orthographicSize * 2;
        float xWorldUnits = yWorldUnits * Screen.width / Screen.height;
        float xWorldToPixel = Screen.width / xWorldUnits;
        float yWorldToPixel = Screen.height / yWorldUnits;

        // Compute the position in pixels.
        float x = myPos.x;
        x -= Camera.main.transform.position.x;
        x += (xWorldUnits / 2.0f);
        x *= xWorldToPixel;

        float y = myPos.y;
        y -= Camera.main.transform.position.y;
        y += (yWorldUnits / 2.0f);
        y *= yWorldToPixel;

        return ((int)x, (int)y);
    }

    public int GetOwner()
    {
        return owner;
    }

    // FUNCTIONS FOR ASSIGNING INFO TO GABOR.
    public void SetClaimable(bool canBeClaimed)
    {
        claimable = canBeClaimed;
    }

    public void SetClaimed(bool isClaimed)
    {
        // Get the current colours.
        Color currentHighlightColour = highlightSprite.color;
        // Set the alpha channels to the regular (visible) levels.
        if (isClaimed)
        {
            currentHighlightColour.a = thisPlayerColour.a;
        }
        // Set the alpha channels to 0.
        else
        {
            currentHighlightColour.a = 0.0f;
        }
        // Set the new colour as the highlight's colour.
        highlightSprite.color = currentHighlightColour;
    }

    private void SetHighlightColour(Color colour)
    {
        highlightSprite.color = colour;
    }

    public void SetMasked(bool isMasked)
    {
        // Set the masked bool
        masked = isMasked;
        // Get the current mask colour.
        Color currentNoiseColour = noiseSprite.color;
        // Set the alpha channel to the regular (visible) levels.
        if (isMasked)
        {
            // Hide the Gabor.
            SetVisible(false);
            // Unhide the mask.
            currentNoiseColour.a = 1.0f;
        }
        // Set the alpha channel to 0.
        else
        {
            // Unide the Gabor.
            SetVisible(true);
            // Hide the mask
            currentNoiseColour.a = 0.0f;
        }
        // Set the new colour as the Gabor and highlight's colour.
        noiseSprite.color = currentNoiseColour;
    }

    public void SetOrientation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetPlayerColour(Color colour)
    {
        thisPlayerColour = colour;
    }

    public void SetPosition(int x, int y)
    {
        // Compute the pixel-to-world conversion.
        float yWorldUnits = Camera.main.orthographicSize * 2;
        float xWorldUnits = yWorldUnits * Screen.width / Screen.height;
        float xWorldToPixel = Screen.width / xWorldUnits;
        float yWorldToPixel = Screen.height / yWorldUnits;

        // Compute the position in the world.
        Vector3 newPos = new Vector3(0, 0, -10);
        newPos.x = (((float)x / xWorldToPixel) - (xWorldUnits / 2.0f)) + Camera.main.transform.position.x;
        newPos.y = (((float)y / yWorldToPixel) - (yWorldUnits / 2.0f)) + Camera.main.transform.position.y;

        // Set the position in world-units.
        transform.position = newPos;
    }

    public void SetRotatable(bool canBeRotated)
    {
        rotatable = canBeRotated;
    }

    public void SetSelection(bool selectionVisible)
    {
        // Get the current colour.
        Color currentSelectionRingColour = selectionRingSprite.color;
        // Make the selection ring visible or invisible.
        if (selectionVisible)
        {
            currentSelectionRingColour.a = 1.0f;
        }
        else
        {
            currentSelectionRingColour.a = 0.0f;
        }
        // Set the current colour.
        selectionRingSprite.color = currentSelectionRingColour;
    }

    public void SetVisible(bool isVisible)
    {
        // Get the current colour.
        Color currentGaborColour = gaborSprite.color;
        // Set the alpha channel to the regular (visible) levels.
        if (isVisible)
        {
            currentGaborColour.a = 1.0f;
        }
        // Set the alpha channel to 0.
        else
        {
            currentGaborColour.a = 0.0f;
        }
        // Set the new colour as the Gabor and highlight's colour.
        gaborSprite.color = currentGaborColour;
    }

}
