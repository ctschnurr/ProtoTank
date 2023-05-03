using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum State
    {
        patrolling
    }

    public State state = State.patrolling;
    public UnityEngine.AI.NavMeshAgent enemyAgent;
    GameObject enemyObject;

    Transform nextWaypoint;
    Transform waypoint1;
    Transform waypoint2;
    Transform waypoint3;
    Transform waypoint4;
    Transform waypoint5;
    public List<Transform> waypointList;

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
        enemyObject = transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.patrolling:
                enemyAgent.SetDestination(nextWaypoint.position);
                if (Vector3.Distance(nextWaypoint.position, transform.position) < 1) nextWaypoint = NextWaypoint(nextWaypoint);
                // if (Vector3.Distance(playerPosition, transform.position) < 2.5) SetState(State.chasing);
                break;
        }
    }

    Transform NextWaypoint(Transform input)
    {
        waypointList.Remove(input);
        waypointList.Add(input);

        return waypointList[0];
    }
}
