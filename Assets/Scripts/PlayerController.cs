using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float barrelSpeed = 1.0f;
    GameObject barrelChassis;
    GameObject barrel;
    GameObject player;

    public GameObject projectile;
    GameObject shotOrigin;
    public float launchVelocity = 1000f;

    float barrelH;
    float barrelV;

    float ShootDelay = 1;
    float lastShotTimer;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        barrelChassis = GameObject.Find("Player/BarrelChasis");
        barrel = GameObject.Find("Player/BarrelChasis/BarrelRotater");
        shotOrigin = GameObject.Find("Player/BarrelChasis/BarrelRotater/Barrel/ShotOrigin");

        lastShotTimer = Time.time;
    }

    void barrelControl(float horizontal, float vertical)
    {
        barrelH = Mathf.Clamp(horizontal, 225, 315);
        barrelV = Mathf.Clamp(vertical, 105, 135);
        barrelChassis.transform.localRotation = Quaternion.Euler(90, 0, barrelH);
        barrel.transform.localRotation = Quaternion.Euler(90, 0, barrelV);
    }

    // Update is called once per frame
    void Update()
    {
        float vert = Input.GetAxis("Vertical") * moveSpeed;
        float horiz = Input.GetAxis("Horizontal");

        vert *= Time.deltaTime;

        transform.Translate(0, 0, vert);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) transform.Rotate(0.0f, horiz, 0.0f);

        float barrelHorizontal = barrelH + Input.GetAxis("Mouse X") * barrelSpeed;
        float barrelVertical = barrelV + Input.GetAxis("Mouse Y") * barrelSpeed;
        barrelControl(barrelHorizontal, barrelVertical);

        if (Input.GetKey(KeyCode.Space))
        {
            if ((Time.time - lastShotTimer > ShootDelay))
            {
                GameObject shot = Instantiate(projectile, shotOrigin.transform.position, shotOrigin.transform.rotation);
                shot.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, launchVelocity, 0));

                lastShotTimer = Time.time;
            }
        }

    }
}
