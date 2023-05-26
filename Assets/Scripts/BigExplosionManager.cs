using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigExplosionManager : MonoBehaviour
{
    float expandSpeed = 50f;
    float fadeSpeed = 1f;
    Vector3 scaleChange;

    AudioSource bombSound;
    public AudioClip boomSound;

    // Start is called before the first frame update
    void Start()
    {
        expandSpeed = expandSpeed * Time.deltaTime;
        fadeSpeed = fadeSpeed * Time.deltaTime;

        scaleChange = new Vector3(expandSpeed, expandSpeed, expandSpeed);

        bombSound = GetComponent<AudioSource>();
        boomSound = Resources.Load<AudioClip>("explosion");

        bombSound.pitch = 0.8f;
        bombSound.PlayOneShot(boomSound);

    }

    // Update is called once per frame
    void Update()
    {

        Color tempcolor = GetComponent<Renderer>().material.color;
        tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, fadeSpeed);
        GetComponent<Renderer>().material.color = tempcolor;

        if (transform.localScale.x < 40) transform.localScale += scaleChange;
        if (transform.localScale.x > 40)
        {
            //Destroy(gameObject);
            GetComponent<Collider>().enabled = false;
        }

        if (!bombSound.isPlaying) Destroy(gameObject);
    }
}
