using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using DeltaDNA;

public class PlayerManager : MonoBehaviour {

    public int playerLevel = 1 ;
    public int playerHealth = 100 ;
    public int playerCoins = 10 ;

    public int bricksRemaining = 0; 
    public HudManager hud;

	// Use this for initialization
	void Start () {

        hud = GameObject.FindObjectOfType<HudManager>();          
        UpdatePlayerStatistics();
    }

    public void SetLevel(int l)
    {
        playerLevel = l;
        hud.SetLevel(playerLevel);
    }
    public void SetHealth(int h)
    {
        playerHealth = h;
        hud.SetHealth(playerHealth);
    }
    public void SetCoins(int c)
    {
        playerCoins = c;
        hud.SetCoins(playerCoins);
    }
    public void SetBricksRemaining(int f)
    {
        bricksRemaining = f;
        hud.SetBricksRemaining(bricksRemaining);
    }

    public void UpdatePlayerStatistics()
    { 
        hud.SetCoins(playerCoins);
        hud.SetHealth(playerHealth);
        hud.SetLevel(playerLevel);
        hud.SetBricksRemaining(bricksRemaining);
    } 
}