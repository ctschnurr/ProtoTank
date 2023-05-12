using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DroneController : Objective
{
    public enum State
    {
        patrolling,
        dead
    }

    public GameObject waypointHolder;

    [HideInInspector]
    public State state = State.patrolling;
    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent enemyDrone;

    static System.Random waypointRand = new System.Random();

    static DroneController instance;

    GameObject parent;
    GameObject body;

    Transform nextWaypoint;

    [HideInInspector]
    public List<Transform> waypointList;
    [HideInInspector]
    public List<GameObject> pieces;

    bool enemyActive = false;
    bool up = false;

    Vector3 posChange;
    float speed = 0.1f;
    Vector3 scaleChange;

    float decay_timer = 5.0f;
    float fadeSpeed = 0.1f;

    [HideInInspector]
    public Component[] droneParts;
    [HideInInspector]
    public Vector3[] resetPositions;
    [HideInInspector]
    public Quaternion[] resetRotations;

    Color tempcolor;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

        enemyDrone = GetComponent<UnityEngine.AI.NavMeshAgent>();

        parent = transform.gameObject;
        subjectObject = parent;
        body = parent.transform.Find("Body").gameObject;

        spawnPoint = transform.position;

        speed *= Time.deltaTime;
        posChange = new Vector3(0, speed, 0);
        scaleChange = new Vector3(speed * 2, speed * 2, speed * 2);

        fadeSpeed *= Time.deltaTime;

        droneParts = GetComponentsInChildren<Renderer>();

        int numberOfPieces = body.transform.childCount;
        pieces = new List<GameObject>();

        resetPositions = new Vector3[numberOfPieces];
        resetRotations = new Quaternion[numberOfPieces];

        for (int i = 0; i < numberOfPieces; i++)
        {
            Transform dronePieceTransform = body.transform.GetChild(i);
            GameObject dronePiece = dronePieceTransform.gameObject;

            resetPositions[i] = dronePiece.transform.localPosition;
            resetRotations[i] = dronePiece.transform.localRotation;

            pieces.Add(dronePiece);
        }

        if (transform.parent != null)
        {
            if (transform.parent.gameObject.tag == "MissionGroup" || transform.parent.gameObject.tag == "ObjectiveGroup") managed = true;
        }

        tpShader = Shader.Find("Transparent/Diffuse");
        normalShader = Shader.Find("Standard");

        MissionManager.OnRunReset += Reset;

        instance = this;

        SetWaypoints();
        nextWaypoint = NextWaypoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.state == GameManager.State.active) enemyActive = true;
        else enemyActive = false;

        if (enemyActive)
        {
            switch (state)
            {
                case State.patrolling:
                    enemyDrone.isStopped = false;
                    enemyDrone.SetDestination(nextWaypoint.position);

                    if (body.transform.localPosition.y > 1.25f) up = false;
                    if (body.transform.localPosition.y < 1f) up = true;

                    if (up) body.transform.localPosition += posChange;
                    if (!up) body.transform.localPosition -= posChange;

                    if (Vector3.Distance(nextWaypoint.position, transform.position) < 1) nextWaypoint = NextWaypoint();
                    break;

                case State.dead:
                    enemyDrone.isStopped = true;
                    decay_timer -= Time.deltaTime;

                    if (decay_timer < 0)
                    {
                        foreach (Renderer partRenderer in droneParts)
                        {
                            partRenderer.material.shader = tpShader;

                            tempcolor = partRenderer.material.color;
                            tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, 0.005f);
                            partRenderer.material.color = tempcolor;

                            if(tempcolor.a == 0)
                            {
                                parent.gameObject.SetActive(false);
                            }
                        }
                    }
                    break;
            }
        }
        else if (enemyActive == false)
        {
            enemyDrone.isStopped = true;
        }
    }

    public static void SetWaypoints()
    {
        int numberOfWaypoints = instance.waypointHolder.transform.childCount;

        for (int i = 0; i < numberOfWaypoints; i++)
        {
            Transform waypointTransform = instance.waypointHolder.transform.GetChild(i);

            instance.waypointList.Add(waypointTransform);
        }
    }

    Transform NextWaypoint()
    {
        int waypointRandom = waypointRand.Next(1, instance.waypointList.Count);

        return instance.waypointList[waypointRandom];
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Explosion")
        {
            state = State.dead;
            RunComplete();

            parent.GetComponent<Collider>().enabled = false;
            foreach (GameObject piece in pieces)
            {
                piece.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    void Reset()
    {
        if (transform.position != spawnPoint)
        {
            transform.position = spawnPoint;
            transform.rotation = Quaternion.identity;
        }

        if (state == State.dead)
        {

            for (int i = 0; i < pieces.Count; i++)
            {
                GameObject piece = pieces[i];

                piece.SetActive(true);
                piece.GetComponent<Rigidbody>().isKinematic = true;

                piece.transform.localPosition = resetPositions[i];
                piece.transform.localRotation = resetRotations[i];
            }

            foreach (Renderer partRenderer in droneParts)
            {
                tempcolor = partRenderer.material.color;
                tempcolor.a = 1f;
                partRenderer.material.color = tempcolor;

                partRenderer.material.shader = normalShader;
            }

            state = State.patrolling;
        }
    }
}
