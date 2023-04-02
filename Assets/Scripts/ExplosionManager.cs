using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    GameObject explosion;
    float expandSpeed = 0.2f;
    float fadeSpeed = 0.01f;
    Vector3 scaleChange;

    // Start is called before the first frame update
    void Start()
    {
        scaleChange = new Vector3(expandSpeed, expandSpeed, expandSpeed);
        //explosion = GameObject.Find("Explosion/orb");
    }

    // Update is called once per frame
    void Update()
    {
        // explosion = GameObject.Find("Explosion(Clone)");
        Color tempcolor = GetComponent<Renderer>().material.color;
        tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, fadeSpeed);
        GetComponent<Renderer>().material.color = tempcolor;

        transform.localScale += scaleChange;
        if (transform.localScale.x > 15) Destroy(gameObject);
    }
}
