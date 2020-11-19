using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptScene03 : MonoBehaviour
{
    [SerializeField] Treasure_Box Treasure_BoxPrefab;
    [SerializeField] Coin feedbackPrefab;

    private static int totalcoins = MemTestManager.nCoins;
    //private static int totalcoins = 2;
    private List<Coin> coinList = new List<Coin>();

    

    Vector3 coinposition = new Vector3(-6, 3, 0);

    void Start()
    {
        Vector3 boxposition = new Vector3(-8, 0, 0);
        Instantiate(Treasure_BoxPrefab, boxposition, transform.rotation);

        for (int i = 0; i < totalcoins; i++)
        {
            coinposition.x = coinposition.x + i;
            //Coin.target = Coin.target.y +1f;
            feedbackPrefab.changetarget(true);
            coinList.Add(Instantiate(feedbackPrefab, coinposition, transform.rotation));
        }
    }
    void Update()
    {
        
    } 
}

