using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    GameObject explosion;
    float speed = 0.1f;
    Vector3 scaleChange;

    // Start is called before the first frame update
    void Start()
    {
        scaleChange = new Vector3(speed, speed, speed);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += scaleChange;
        if (transform.localScale.x > 5) Destroy(gameObject);
    }
}
