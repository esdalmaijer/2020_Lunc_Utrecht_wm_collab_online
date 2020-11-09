using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{

    private float speed = 10.0f;
    private Vector2 target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = transform.position;
        Vector2 endPosition = new Vector2(position.x,-2);

        if(position != endPosition) {
            /*position.y = position.y - 0.05f;
            transform.position = position;*/
            float step = speed * Time.deltaTime;
            target = new Vector2(6f, -2f);

            transform.position = Vector2.MoveTowards(transform.position, target, step);
        }

     
    }
}
