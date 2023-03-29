using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float barrelSpeed = 1.0f;
    GameObject chassis;
    GameObject barrel;
    GameObject player;

    public GameObject projectile;
    GameObject shotOrigin;
    public float launchVelocity = 800f;

    float barrelH;
    float barrelV;

    float ShootDelay = 1;
    float lastShotTimer;

    float barrelVertical = 80.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        chassis = GameObject.Find("Player/ChassisRotater");
        barrel = GameObject.Find("Player/ChassisRotater/BarrelRotater");
        shotOrigin = GameObject.Find("Player/ChassisRotater/BarrelRotater/Barrel/ShotOrigin");

        lastShotTimer = Time.time;


    }

    void barrelControl(float horizontal, float vertical)
    {
        barrelH = Mathf.Clamp(horizontal, -45, 45);
        barrelV = Mathf.Clamp(vertical, 50, 80);
        chassis.transform.localRotation = Quaternion.Euler(0, barrelH, 0);
        barrel.transform.localRotation = Quaternion.Euler(barrelV, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float vert = Input.GetAxis("Vertical") * moveSpeed;
        float horiz = Input.GetAxis("Horizontal");

        vert *= Time.deltaTime;

        transform.Translate(0, 0, vert);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) transform.Rotate(0.0f, horiz, 0.0f);

        float mouseY = Input.GetAxis("Mouse Y") * barrelSpeed;
        float barrelHorizontal = barrelH + Input.GetAxis("Mouse X") * barrelSpeed;
        barrelVertical -= mouseY;
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

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

    }
}
