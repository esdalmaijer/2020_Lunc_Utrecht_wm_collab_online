using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptScene03 : MonoBehaviour
{
    [SerializeField] Treasure_Box Treasure_BoxPrefab;
    [SerializeField] Treasure_Box_front Treasure_Box_frontPrefab;
    [SerializeField] Coin feedbackPrefab;

    private static int totalcoins = MemTestManager.nCoins;
    

    private List<Coin> coinList = new List<Coin>();
    Vector3 coinposition = new Vector3(-6, 3, 0);
    Vector3 boxposition = new Vector3(0, 5, 0);
    Vector3 boxfrontposition = new Vector3(0, -2, 0);


    void Start()
    {
        Scorescene_Text.scoreValue = totalcoins;
        Instantiate(Treasure_BoxPrefab, boxposition, transform.rotation);
        Instantiate(Treasure_Box_frontPrefab, boxfrontposition, transform.rotation);

        for (int i = 0; i < totalcoins; i++)
        {
            coinposition.x = coinposition.x + i;
           
            feedbackPrefab.changetarget(true);
            coinList.Add(Instantiate(feedbackPrefab, coinposition, transform.rotation));

            
        }
    }
    void Update()
    {
        
    }

    
    
}

