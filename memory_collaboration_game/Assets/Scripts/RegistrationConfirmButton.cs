using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistrationConfirmButton : MonoBehaviour
{
    // Allow the parent object to be assigned by the parent, by declaring
    // it public.
    public RegistrationPage parent;

    // Sync a click on this object with a parent function.
    private void OnMouseDown()
    {
        parent.ConfirmationClick();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
