using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigShotManager : MonoBehaviour
{
    public GameObject explosion;
    Vector3 save = new Vector3();

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
        GetComponent<Collider>().enabled = !GetComponent<Collider>().enabled;
        save = transform.position;
        //save.y -= 0.5f;

        GameObject boom = Instantiate(explosion, save, transform.rotation);
        Destroy(gameObject);
    }
}
