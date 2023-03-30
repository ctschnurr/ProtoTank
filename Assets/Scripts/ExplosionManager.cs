using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    GameObject explosion;
    float speed = 0.05f; 
    Vector3 scaleChange;

    // Start is called before the first frame update
    void Start()
    {
        scaleChange = new Vector3(speed, speed, speed);
        //explosion = GameObject.Find("Explosion/orb");
    }

    // Update is called once per frame
    void Update()
    {
        explosion = GameObject.Find("Explosion(Clone)/orb");
        Color tempcolor = explosion.GetComponent<Renderer>().material.color;
        tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, 0.006f);
        explosion.GetComponent<Renderer>().material.color = tempcolor;

        transform.localScale += scaleChange;
        if (transform.localScale.x > 5) Destroy(gameObject);
    }
}
