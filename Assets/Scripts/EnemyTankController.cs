using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyTankController : Objective
{
    public enum State
    {
        patrolling,
        chasing,
        tracking,
        retreating,
        dead
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
    GameObject barrelObj;
    GameObject player;
    public GameObject projectile;
    GameObject shotOrigin;

    Renderer barrelRenderer;
    Renderer bodyRenderer;
    Renderer chassisRenderer;
    Renderer leftTreadRenderer;
    Renderer rightTreadRenderer;

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

    float attackRotateSpeed = 1f;

    public float barrelV;
    bool enemyActive = false;
    bool damaged = false;
    bool vulnerable = true;

    int lives = 3;
    int livesMax = 3;

    int damageCount = 6;
    int damageCountReset = 6;

    float damageTimer = 0.1f;
    float damageTimerReset = 0.1f;

    Color normalColor;
    Color normalColorBarrel;
    Color damageColor;

    Color torchedBarrel;
    Color torchedChassis;

    AudioSource tankSound;
    public AudioClip tankEngine;
    public AudioClip tankFire;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

        waypoint1 = GameObject.Find("waypoint1").transform;
        waypoint2 = GameObject.Find("waypoint2").transform;
        waypoint3 = GameObject.Find("waypoint3").transform;
        waypoint4 = GameObject.Find("waypoint4").transform;
        waypoint5 = GameObject.Find("waypoint5").transform;

        SetWaypoints();
        spawnPoint = transform.position;

        enemyAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        parent = transform.gameObject;
        subjectObject = parent;

        chassis = parent.transform.Find("EnemyChassis").gameObject;
        barrel = parent.transform.Find("EnemyChassis/EnemyBarrel").gameObject;
        player = GameObject.Find("Player").gameObject;
        shotOrigin = parent.transform.Find("EnemyChassis/EnemyBarrel/Barrel/ShotOrigin").gameObject;
        barrelObj = parent.transform.Find("EnemyChassis/EnemyBarrel/Barrel").gameObject;

        barrelRenderer = parent.transform.Find("EnemyChassis/EnemyBarrel/Barrel").GetComponent<Renderer>();
        bodyRenderer = parent.transform.Find("Body").GetComponent<Renderer>();
        chassisRenderer = parent.transform.Find("EnemyChassis/Chassis").GetComponent<Renderer>();
        leftTreadRenderer = parent.transform.Find("TreadSkirtL").GetComponent<Renderer>();
        rightTreadRenderer = parent.transform.Find("TreadSkirtR").GetComponent<Renderer>();

        barrel.transform.localRotation = Quaternion.Euler(80, 0, 0);

        attackRotateSpeed = attackRotateSpeed * Time.deltaTime;
        lastShotTimer = Time.time;

        normalColor = bodyRenderer.material.color;
        normalColorBarrel = barrelRenderer.material.color;
        damageColor = new Color(1, 1, 1);

        torchedBarrel = normalColorBarrel;
        torchedBarrel = new Color(torchedBarrel.r - 0.3f, torchedBarrel.g - 0.3f, torchedBarrel.b - 0.3f);
        torchedChassis = new Color(normalColor.r - 0.8f, normalColor.g - 0.8f, normalColor.b - 0.8f);

        if (transform.parent != null)
        {
            if (transform.parent.gameObject.tag == "MissionGroup" || transform.parent.gameObject.tag == "ObjectiveGroup") managed = true;
        }

        tpShader = Shader.Find("Transparent/Diffuse");

        MissionManager.OnRunReset += Reset;

        tankSound = GetComponent<AudioSource>();
        tankEngine = Resources.Load<AudioClip>("tankEngine");
        tankFire = Resources.Load<AudioClip>("tankFire");
    }

    void SetWaypoints()
    {
        waypointList.Clear();

        waypointList.Add(waypoint1);
        waypointList.Add(waypoint2);
        waypointList.Add(waypoint3);
        waypointList.Add(waypoint4);
        waypointList.Add(waypoint5);

        nextWaypoint = waypoint1;
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = player.transform.position;
        distance = Vector3.Distance(playerPosition, transform.position);

        if (gameManager.state == GameManager.State.active) enemyActive = true;
        else enemyActive = false;

        if (enemyActive)
        {
            if (!tankSound.isPlaying && state != State.dead)
            {
                tankSound.clip = tankEngine;
                tankSound.loop = true;
                tankSound.Play();
            }

            if (enemyAgent.isStopped == true && state != State.dead) tankSound.pitch = 1;
            else if (enemyAgent.isStopped == false && state != State.dead) tankSound.pitch = 1.3f;

            switch (state)
            {
                case State.patrolling:
                    enemyAgent.SetDestination(nextWaypoint.position);

                    chassis.transform.Rotate(Vector3.up * 0.1f);

                    currentAngle = barrel.transform.eulerAngles;
                    targetAngle.x = 80;
                    nextAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime), currentAngle.y, currentAngle.z);
                    barrel.transform.eulerAngles = nextAngle;

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
                            barrelV = 1700 / distance;
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

                    if (Vector3.Distance(playerPosition, transform.position) > 20) SetState(State.tracking);
                    if (Vector3.Distance(transform.position, retreatTarget.position) < 1) retreatTarget = ClosestWaypoint(waypointList);
                    if (Vector3.Distance(playerPosition, transform.position) > 30) SetState(State.patrolling);
                    break;

                case State.dead:
                    enemyAgent.isStopped = true;
                    tankSound.Stop();

                    barrelObj.GetComponent<Rigidbody>().isKinematic = false;
                    barrelRenderer.material.color = torchedBarrel;
                    chassisRenderer.material.color = torchedChassis;
                    bodyRenderer.material.color = torchedChassis;
                    leftTreadRenderer.material.color = torchedChassis;
                    rightTreadRenderer.material.color = torchedChassis;
                    break;
            }
        }
        else if (enemyActive == false)
        {
            enemyAgent.isStopped = true;

            if (tankSound.isPlaying && tankSound.clip == tankEngine)
            {
                tankSound.Stop();
            }
        }


        if (damaged)
        {
            if (damageTimer < 0)
            {
                RunDamageBlink();
                damageTimer = damageTimerReset;
            }
            else if (damageTimer > 0)
            {
                damageTimer -= Time.deltaTime;
            }
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

            tankSound.PlayOneShot(tankFire);
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

            case State.dead:
                enemyAgent.isStopped = true;
                state = State.dead;
                RunComplete();
                break;

            default:
                state = input;
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Explosion")
        {
            if (vulnerable)
            {
                enemyActive = false;
                damaged = true;
                vulnerable = false;
                lives--;

                if (lives == 0)
                {
                    SetState(State.dead);
                }
            }
        }
    }

    void RunDamageBlink()
    {
        if (damageCount > 0)
        {
            if (bodyRenderer.material.color == normalColor)
            {
                bodyRenderer.material.color = damageColor;
                chassisRenderer.material.color = damageColor;
                leftTreadRenderer.material.color = damageColor;
                rightTreadRenderer.material.color = damageColor;

                damageCount--;
            }
            else if (bodyRenderer.material.color == damageColor)
            {
                bodyRenderer.material.color = normalColor;
                chassisRenderer.material.color = normalColor;
                leftTreadRenderer.material.color = normalColor;
                rightTreadRenderer.material.color = normalColor;

                damageCount--;
            }
        }
        else if (damageCount == 0)
        {
            enemyActive = true;
            damaged = false;
            vulnerable = true;
            damageCount = damageCountReset;
        }
    }

    void Reset()
    {
        if (transform.position != spawnPoint)
        {
            transform.position = spawnPoint;
            transform.rotation = Quaternion.identity;
        }

        lives = livesMax;
        if (state == State.dead)
        {
            barrelObj.GetComponent<Rigidbody>().isKinematic = true;
            barrel.transform.localRotation = Quaternion.Euler(80, 0, 0);

            barrelRenderer.material.color = normalColorBarrel;
            chassisRenderer.material.color = normalColor;
            bodyRenderer.material.color = normalColor;
            leftTreadRenderer.material.color = normalColor;
            rightTreadRenderer.material.color = normalColor;

            SetWaypoints();
            SetState(State.patrolling);
        }
    }
}
