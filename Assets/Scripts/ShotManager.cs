using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotManager : MonoBehaviour
{
    public GameObject explosion;
    Vector3 save = new Vector3();

    bool deadTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!deadTriggered)
        {
            deadTriggered = true;

            GetComponent<Collider>().enabled = !GetComponent<Collider>().enabled;
            save = transform.position;

            GameObject boom = Instantiate(explosion, save, transform.rotation);
            Destroy(gameObject);
        }
    }


}
