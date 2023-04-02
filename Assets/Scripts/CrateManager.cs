using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateManager : MonoBehaviour
{
    float decay_timer = 0.0f;
    float fadeSpeed = 0.0008f;

    bool destroyed = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyed)
        {
            decay_timer += 0.05f;

            if (decay_timer > 25)
            {
                Color tempcolor = GetComponent<Renderer>().material.color;
                tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, fadeSpeed);

                GetComponent<Renderer>().material.color = tempcolor;

                if (GetComponent<Renderer>().material.color.a == 0) Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Explosion")
        {
            destroyed = true;
        }
    }
}
