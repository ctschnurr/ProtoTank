using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public enum State
    {
        idle,
        tracking,
        dead
    }

    Vector3 playerPosition = new Vector3(0, 0, 0);

    GameObject parent;

    GameObject chassisRotater;
    GameObject barrelRotater;
    GameObject barrelChassis;
    GameObject barrel;
    GameObject shotOrigin;
    GameObject player;

    public float launchVelocity = 500f;
    public GameObject projectile;

    public float distance;

    public float barrelV;
    float barrelVertical;

    float ShootDelay = 2;
    float lastShotTimer;

    State state = State.idle;

    //values for internal use
    private Quaternion _lookRotation;
    private Vector3 _direction;

    public float speed = 0.25f;

    float singleStep;
    public float nextY;

    Vector3 targetAngle = new Vector3(0f, 0f, 0f);
    Vector3 currentAngle;
    Vector3 nextAngle;

    Color torchedBarrel;
    Color torchedChassis;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.gameObject;

        chassisRotater = parent.transform.Find("ChassisRotater").gameObject; // ("TurretPrefab/ChassisRotater");
        barrelChassis = parent.transform.Find("ChassisRotater/BarrelChassis").gameObject; // ("TurretPrefab/ChassisRotater/BarrelChassis");
        barrelRotater = parent.transform.Find("ChassisRotater/BarrelRotater").gameObject; // GameObject.Find("TurretPrefab/ChassisRotater/BarrelRotater");
        barrel = parent.transform.Find("ChassisRotater/BarrelRotater/Barrel").gameObject; // GameObject.Find("TurretPrefab/ChassisRotater/BarrelRotater/Barrel");
        shotOrigin = parent.transform.Find("ChassisRotater/BarrelRotater/Barrel/ShotOrigin").gameObject; // GameObject.Find("TurretPrefab/ChassisRotater/BarrelRotater/Barrel/ShotOrigin");
        player = GameObject.Find("Player").gameObject;

        lastShotTimer = Time.time;

        barrelRotater.transform.localRotation = Quaternion.Euler(80, 0, 0);

        singleStep = speed * Time.deltaTime;

        torchedBarrel = barrel.GetComponent<Renderer>().material.color;
        torchedBarrel = new Color(torchedBarrel.r - 0.3f, torchedBarrel.g - 0.3f, torchedBarrel.b - 0.3f);
        torchedChassis = barrelChassis.GetComponent<Renderer>().material.color;
        torchedChassis = new Color(torchedChassis.r - 0.5f, torchedChassis.g - 0.5f, torchedChassis.b - 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = player.transform.position;
        distance = Vector3.Distance(playerPosition, transform.position);

        switch (state)
        {
            case State.idle:
                chassisRotater.transform.Rotate(Vector3.up * 0.1f);

                currentAngle = barrelRotater.transform.eulerAngles;
                targetAngle.x = 80;
                nextAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime), currentAngle.y, currentAngle.z);
                barrelRotater.transform.eulerAngles = nextAngle;

                if (distance < 30) state = State.tracking;
                break;

            case State.tracking:
                Vector3 playerDirection = playerPosition;

                // left/right rotation for barrel chassis
                Vector3 targetDirection = playerPosition - chassisRotater.transform.position;
                Vector3 newDirection = Vector3.RotateTowards(chassisRotater.transform.forward, targetDirection, singleStep, 0.0f);
                newDirection.y = 0;
                chassisRotater.transform.rotation = Quaternion.LookRotation(newDirection);
                //

                // up/down rotation for barrel
                currentAngle = barrelRotater.transform.eulerAngles;
                barrelV = 500 / distance;
                barrelV = Mathf.Clamp(barrelV, 40, 60);
                targetAngle.x = barrelV;
                nextAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime), currentAngle.y, currentAngle.z);
                barrelRotater.transform.eulerAngles = nextAngle;
                //

                // adjusting shot power based on distance to player
                launchVelocity = 30 * distance;
                launchVelocity = Mathf.Clamp(launchVelocity, 300, 600);
                //

                if (Vector3.Distance(playerPosition, transform.position) < 20 && Vector3.Distance(playerPosition, chassisRotater.transform.position) > 10) Fire();
                if (Vector3.Distance(playerPosition, transform.position) > 25) state = State.idle;
                break;

            case State.dead:
                barrel.GetComponent<Rigidbody>().isKinematic = false;
                barrel.GetComponent<Renderer>().material.color = torchedBarrel;
                barrelChassis.GetComponent<Renderer>().material.color = torchedChassis;
                break;
        }

    }

    public void Fire()
    {
        if ((Time.time - lastShotTimer > ShootDelay))
        {
            GameObject shot = Instantiate(projectile, shotOrigin.transform.position, shotOrigin.transform.rotation);
            shot.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, launchVelocity, 0));

            lastShotTimer = Time.time;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Shell")
        {
            state = State.dead;
        }
    }
}
