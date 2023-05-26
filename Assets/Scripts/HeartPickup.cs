using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPickup : Pickup
{
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();

        speed *= Time.deltaTime;
        posChange = new Vector3(0, speed, 0);

        self = transform.gameObject;

        MissionManager.OnRunReset += Reset;

        pickupSound = GetComponent<AudioSource>();
        pickupClip = Resources.Load<AudioClip>("pickup");
    }

    public override void ActivateMe()
    {
        int lives = player.GetLives();
        if (lives >= 4)
        {
            // string[] output = new string[2];
            // output[0] = "dialogue";
            // output[1] = "Your health is already full!";
            // screenManager.SetScreen(output);
        }
        else
        {
            pickupSound.PlayOneShot(pickupClip);

            player.AddHealth(1);
            activated = true;

        }
    }
}
