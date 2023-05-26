using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public static PlayerController player;
    public static ScreenManager screenManager;
    public static MissionManager missionManager;

    public bool activated = false;
    public bool up = false;

    public Vector3 posChange;
    public float speed = 0.1f;

    public GameObject self;

    public AudioSource pickupSound;
    public AudioClip pickupClip;

    // Start is called before the first frame update
    void Start()
    {
        self = transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            if (!pickupSound.isPlaying)
            {
                self.SetActive(false);
                activated = false;
            }
        }

        if (transform.localPosition.y > 1f) up = false;
        if (transform.localPosition.y < 0.5f) up = true;

        // if (up) transform.localPosition += posChange;
        // if (!up) transform.localPosition -= posChange;

        transform.Rotate(Vector3.up * 0.5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !activated)
        {
            ActivateMe();
        }
    }

    public virtual void ActivateMe()
    {

    }

    public void Reset()
    {
        self.SetActive(true);
        activated = false;
    }
}
