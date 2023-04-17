using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    float expandSpeed = 150f;
    float fadeSpeed = 5f;
    Vector3 scaleChange;

    // AudioSource bombSound;
    // public AudioClip boomSound;

    // Start is called before the first frame update
    void Start()
    {
        expandSpeed = expandSpeed * Time.deltaTime;
        fadeSpeed = fadeSpeed * Time.deltaTime;

        scaleChange = new Vector3(expandSpeed, expandSpeed, expandSpeed);
        // bombSound = GetComponent<AudioSource>();
        // boomSound = Resources.Load<AudioClip>("boom");
        // bombSound.PlayOneShot(boomSound);

    }

    // Update is called once per frame
    void Update()
    {

        Color tempcolor = GetComponent<Renderer>().material.color;
        tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, fadeSpeed);
        GetComponent<Renderer>().material.color = tempcolor;

        if (transform.localScale.x < 20) transform.localScale += scaleChange;
        if (transform.localScale.x > 20)
        {
            Destroy(gameObject);
        }
    }
}
