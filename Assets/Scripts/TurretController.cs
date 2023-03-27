using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public enum State
    {
        idle,
        tracking
    }

    Vector3 playerPosition = new Vector3(0, 0, 0);

    GameObject chassisRotater;
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

    // Start is called before the first frame update
    void Start()
    {
        chassisRotater = GameObject.Find("TurretPrefab/ChassisRotater");
        barrel = GameObject.Find("TurretPrefab/ChassisRotater/BarrelRotater");
        shotOrigin = GameObject.Find("TurretPrefab/ChassisRotater/BarrelRotater/Barrel/ShotOrigin");
        player = GameObject.Find("Player").gameObject;

        lastShotTimer = Time.time;

        barrel.transform.localRotation = Quaternion.Euler(80, 0, 0);

        singleStep = speed * Time.deltaTime;
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
                if (distance < 25) SetState(State.tracking);
                break;

            case State.tracking:
                Vector3 playerDirection = playerPosition;

                Vector3 targetDirection = playerPosition - chassisRotater.transform.position;
                Vector3 newDirection = Vector3.RotateTowards(chassisRotater.transform.forward, targetDirection, singleStep, 0.0f);
                newDirection.y = 0;
                chassisRotater.transform.rotation = Quaternion.LookRotation(newDirection);

                Vector3 targetRotation = barell.transform.rotation;
                Vector3 newRotation = Vector3.RotateTowards(barrel.transform.forward, targetRotation, singleStep, 0.0f);
                barrel.transform.rotation = Quaternion.LookRotation(newRotation);


                // barrelVertical = distance;
                //barrelControl(barrelVertical);

                if (Vector3.Distance(playerPosition, transform.position) < 20 && Vector3.Distance(playerPosition, chassisRotater.transform.position) > 5) Fire();
                if (Vector3.Distance(playerPosition, transform.position) > 25) SetState(State.idle);
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

    public void SetState(State input)
    {
        switch (input)
        {
            case State.idle:
                state = State.idle;
                break;

            case State.tracking:
                state = State.tracking;
                break;
        }
    }

    void barrelControl(float vertical)
    {
        vertical = Mathf.Abs(vertical - 70);
        barrelV = Mathf.Clamp(vertical, 15, 80);

        barrel.transform.localRotation = Quaternion.Euler(barrelV, 0, 0);
    }
}
