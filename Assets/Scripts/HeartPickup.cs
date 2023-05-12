using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPickup : Pickup
{

    bool activated = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            gameObject.SetActive(false);
            activated = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !activated)
        {
            ActivateMe();
        }
    }

    public void ActivateMe()
    {
        int lives = player.GetLives();
        if (lives >= 4)
        {
            string[] output = new string[2];
            output[0] = "dialogue";
            output[1] = "Your health is already full!";
            screenManager.SetScreen(output);
        }
        else
        {
            player.AddHealth(1);
            activated = true;
        }
    }
}
