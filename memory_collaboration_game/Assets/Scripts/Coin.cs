using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
   
    // variables 
    private float speed = 30.0f;
    public Vector2 target = new Vector2(6f, -4f);
    // bool Scene03 = false; 
    

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void changetarget(bool Scene03)
    {
        if(Scene03 == true)
        {
            
            target = new Vector3(1, -3, 0) + Random.insideUnitSphere * 1;
        }
        if(Scene03 == false)
        {
            target.x = 9f;
            target.y = -5f;
        }
    }

        // Update is called once per frame
    void Update()
    {
        
        Vector2 position = transform.position;
        Vector2 endPosition = new Vector2(position.x,-2);

        if(position != endPosition) {
            
            float step = speed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, target, step);
        }
    }

   

   
}
