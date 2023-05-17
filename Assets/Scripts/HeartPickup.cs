using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPickup : Pickup
{

    bool activated = false;
    bool up = false;

    Vector3 posChange;
    float speed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

        speed *= Time.deltaTime;
        posChange = new Vector3(0, speed, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            gameObject.SetActive(false);
            activated = false;
        }

        if (transform.parent.localPosition.y > 1.25f) up = false;
        if (transform.parent.localPosition.y < 1f) up = true;

        if (up) transform.parent.localPosition += posChange;
        if (!up) transform.parent.localPosition -= posChange;

        transform.parent.Rotate(Vector3.up * 0.5f);
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
