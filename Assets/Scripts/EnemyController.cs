using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum State
    {
        patrolling,
        chasing,
        tracking,
        retreating
    }

    GameObject parent;

    public State state = State.patrolling;
    public UnityEngine.AI.NavMeshAgent enemyAgent;

    Transform nextWaypoint;
    Transform waypoint1;
    Transform waypoint2;
    Transform waypoint3;
    Transform waypoint4;
    Transform waypoint5;
    Transform retreatTarget;
    public List<Transform> waypointList;

    GameObject chassis;
    GameObject barrel;
    GameObject player;
    public GameObject projectile;
    GameObject shotOrigin;

    float ShootDelay = 2;
    float lastShotTimer;

    public float distance;
    public float tankSightDistance = 10.0f;

    Vector3 targetAngle = new Vector3(0f, 0f, 0f);
    Vector3 currentAngle;
    Vector3 nextAngle;

    public float launchVelocity = 1000f;

    Vector3 playerPosition = new Vector3(0, 0, 0);

    bool facingPlayer = false;

    Vector3 playerLastPosition = new Vector3(0, 0, 0);
    Vector3 targetDirection;

    float attackRotateSpeed = 0.25f;

    Vector3 testRight;
    Vector3 testLeft;
    Vector3 testDirection;
    Vector3 testNow;

    public float barrelV;

    // Start is called before the first frame update
    void Start()
    {
        waypoint1 = GameObject.Find("waypoint1").transform;
        waypoint2 = GameObject.Find("waypoint2").transform;
        waypoint3 = GameObject.Find("waypoint3").transform;
        waypoint4 = GameObject.Find("waypoint4").transform;
        waypoint5 = GameObject.Find("waypoint5").transform;

        nextWaypoint = waypoint1;

        waypointList.Add(waypoint1);
        waypointList.Add(waypoint2);
        waypointList.Add(waypoint3);
        waypointList.Add(waypoint4);
        waypointList.Add(waypoint5);

        enemyAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        parent = transform.gameObject;

        chassis = parent.transform.Find("EnemyChassis").gameObject;
        barrel = parent.transform.Find("EnemyChassis/EnemyBarrel").gameObject;
        player = GameObject.Find("Player").gameObject;
        shotOrigin = parent.transform.Find("EnemyChassis/EnemyBarrel/Barrel/ShotOrigin").gameObject;

        barrel.transform.localRotation = Quaternion.Euler(80, 0, 0);

        attackRotateSpeed = attackRotateSpeed * Time.deltaTime;
        lastShotTimer = Time.time;

        testRight = chassis.transform.localPosition;
        testRight.x += 10;
        testRight.z += 10;

        testLeft = chassis.transform.localPosition;
        testLeft.x -= 10;
        testLeft.z -= 10;
        testNow = testRight;
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = player.transform.position;
        distance = Vector3.Distance(playerPosition, transform.position);

        switch (state)
        {
            case State.patrolling:
                chassis.transform.Rotate(Vector3.up * 0.1f);

                currentAngle = barrel.transform.eulerAngles;
                targetAngle.x = 80;
                nextAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime), currentAngle.y, currentAngle.z);
                barrel.transform.eulerAngles = nextAngle;

                enemyAgent.SetDestination(nextWaypoint.position);
                if (Vector3.Distance(nextWaypoint.position, transform.position) < 1) nextWaypoint = NextWaypoint(nextWaypoint);
                if (Vector3.Distance(playerPosition, transform.position) < 25 && Vector3.Distance(playerPosition, transform.position) > 10) SetState(State.chasing);
                break;

            case State.chasing:
                targetDirection = playerPosition - chassis.transform.position;
                Vector3 newDirection = Vector3.RotateTowards(chassis.transform.forward, targetDirection, attackRotateSpeed, 0.0f);
                chassis.transform.rotation = Quaternion.LookRotation(newDirection);

                enemyAgent.SetDestination(playerPosition);
                if (Vector3.Distance(playerPosition, transform.position) < 20) SetState(State.tracking);
                break;

            case State.tracking:
                if (Vector3.Distance(playerPosition, transform.position) > 25) SetState(State.chasing);
                if (Vector3.Distance(playerPosition, transform.position) < 10) SetState(State.retreating);

                float angle = Vector3.Angle((playerPosition - transform.position), transform.forward);
                if (angle < 25f) facingPlayer = true;
                else if (angle > 25f) facingPlayer = false;

                switch (facingPlayer)
                {
                    case false:
                        targetDirection = playerPosition - transform.position;
                        newDirection = Vector3.RotateTowards(transform.forward, targetDirection, attackRotateSpeed, 0.0f);
                        transform.rotation = Quaternion.LookRotation(newDirection);
                        break;

                    case true:
                        float chassisAngle = Vector3.Angle((playerPosition - chassis.transform.position), chassis.transform.forward);

                        currentAngle = barrel.transform.eulerAngles;
                        barrelV = 1500 / distance;
                        barrelV = Mathf.Clamp(barrelV, 70, 90);
                        targetAngle.x = barrelV;

                        nextAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime), currentAngle.y, currentAngle.z);
                        barrel.transform.eulerAngles = nextAngle;

                        if (chassisAngle > 3f)
                        {
                            targetDirection = playerPosition - chassis.transform.position;
                            newDirection = Vector3.RotateTowards(chassis.transform.forward, targetDirection, attackRotateSpeed, 0.0f);
                            newDirection.y = 0;
                            chassis.transform.rotation = Quaternion.LookRotation(newDirection);
                        }
                        else if (chassisAngle < 3f) Fire();
                        break;
                }
                break;

            case State.retreating:
                enemyAgent.SetDestination(retreatTarget.position);


                targetDirection = playerPosition - chassis.transform.position;
                newDirection = Vector3.RotateTowards(chassis.transform.forward, targetDirection, attackRotateSpeed, 0.0f);
                newDirection.y = 0;
                chassis.transform.rotation = Quaternion.LookRotation(newDirection);

                if (Vector3.Distance(playerPosition, transform.position) < 20 && Vector3.Distance(playerPosition, transform.position) > 10) SetState(State.tracking);
                if (Vector3.Distance(playerPosition, retreatTarget.position) < 1) retreatTarget = ClosestWaypoint(waypointList);
                break;
        }
    }

    Transform ClosestWaypoint(List<Transform> waypoints)
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (Transform waypoint in waypoints)
        {
            float dist = Vector3.Distance(waypoint.position, transform.position);
            if (dist < minDist && dist > 15)
            {
                nearest = waypoint;
                minDist = dist;
            }
        }
        return nearest;
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

    Transform NextWaypoint(Transform input)
    {
        waypointList.Remove(input);
        waypointList.Add(input);

        return waypointList[0];
    }

    public void SetState(State input)
    {
        //CancelInvoke();
        if (enemyAgent.isStopped == true) enemyAgent.isStopped = false;

        switch (input)
        {
            case State.tracking:
                playerLastPosition = player.transform.position;
                enemyAgent.isStopped = true;
                state = State.tracking;
                break;

            case State.retreating:
                retreatTarget = ClosestWaypoint(waypointList);
                state = State.retreating;
                break;

            default:
                state = input;
                break;
        }
    }
}
