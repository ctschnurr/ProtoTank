using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;

    public enum State
    {
        controlEnabled,
        controlDisabled
    }

    public State state = State.controlEnabled;

    float moveSpeed = 100f;
    float barrelSpeed = 12f;
    float rotateSpeed = 25f;
    float throttleFactor = 2f;
    bool throttle = false;
    GameObject chassis;
    GameObject barrel;
    GameObject player;

    GameObject controlsScreen;

    public GameObject projectile;
    GameObject shotOrigin;
    public float launchVelocity = 1000f;

    float barrelH;
    float barrelV;

    float vert;

    float ShootDelay = 1;
    float lastShotTimer;

    float barrelVertical = 50.0f;

    Vector3 targetAngle = new Vector3(0f, 0f, 0f);
    Vector3 currentAngle;
    Vector3 nextAngle;
    static Vector3 reset;
    static Vector3 respawn;

    public float fliptimer;
    bool flipped = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        controlsScreen = GameObject.Find("ControlsScreen");
        controlsScreen.SetActive(false);

        player = GameObject.Find("Player");
        chassis = GameObject.Find("Player/ChassisRotater");
        barrel = GameObject.Find("Player/ChassisRotater/BarrelRotater");
        shotOrigin = GameObject.Find("Player/ChassisRotater/BarrelRotater/Barrel/ShotOrigin");

        lastShotTimer = Time.time;

        reset = transform.position;
        reset.y += 1;

        moveSpeed = moveSpeed * Time.deltaTime;
        barrelSpeed = barrelSpeed * Time.deltaTime;
        rotateSpeed = rotateSpeed * Time.deltaTime;
    }

    void barrelControl(float horizontal, float vertical)
    {
        barrelH = Mathf.Clamp(horizontal, -45, 45);
        barrelV = Mathf.Clamp(vertical, 60, 80);
        chassis.transform.localRotation = Quaternion.Euler(0, barrelH, 0);
        barrel.transform.localRotation = Quaternion.Euler(barrelV, 0, 0);
    }

    public void Respawn()
    {
        transform.position = respawn;
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {

        switch (state)
        {
            case State.controlDisabled:

                return;

            case State.controlEnabled:
                if (throttle) vert = Input.GetAxis("Vertical") * (moveSpeed * throttleFactor);
                if (!throttle) vert = Input.GetAxis("Vertical") * moveSpeed;

                float horiz = Input.GetAxis("Horizontal") * rotateSpeed;

                vert *= Time.deltaTime;
                transform.Translate(0, 0, vert);

                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) transform.Rotate(0.0f, horiz, 0.0f);

                float mouseY = Input.GetAxis("Mouse Y") * barrelSpeed;
                float barrelHorizontal = barrelH + Input.GetAxis("Mouse X") * barrelSpeed;
                barrelVertical -= mouseY;
                barrelControl(barrelHorizontal, barrelVertical);

                if (Input.GetMouseButton(0))
                {
                    if ((Time.time - lastShotTimer > ShootDelay))
                    {
                        GameObject shot = Instantiate(projectile, shotOrigin.transform.position, shotOrigin.transform.rotation);
                        shot.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, launchVelocity, 0));

                        lastShotTimer = Time.time;
                    }
                }

                if (flipped == true)
                {
                    currentAngle = transform.eulerAngles;
                    targetAngle.x = 0;
                    targetAngle.y = currentAngle.y;
                    targetAngle.z = 0;
                    nextAngle = new Vector3(Mathf.LerpAngle(currentAngle.x, targetAngle.x, 0.05f), currentAngle.y, Mathf.LerpAngle(currentAngle.z, targetAngle.z, 0.05f));
                    transform.eulerAngles = nextAngle;

                    fliptimer = fliptimer - 0.025f;

                    if (fliptimer <= 0)
                    {
                        flipped = false;
                    }
                }

                if (Input.GetKey(KeyCode.Escape))
                {
                    gameManager.PauseGame();
                }

                if (Input.GetKeyDown(KeyCode.F) && flipped == false)
                {
                    // transform.localRotation = Quaternion.Euler(0, transform.localRotation.y, 0);

                    //transform.Translate(0, 5, 0);
                    // transform.position = transform.position + new Vector3(0, 3, 0);

                    flipped = true;
                    fliptimer = 5;
                }

                if (Input.GetKey(KeyCode.LeftShift)) throttle = true;
                if (Input.GetKeyUp(KeyCode.LeftShift)) throttle = false;

                if (Input.GetKey(KeyCode.Tab)) controlsScreen.SetActive(true);
                if (Input.GetKeyUp(KeyCode.Tab)) controlsScreen.SetActive(false);
                return;
        }


        
    }

    public static void SetRespawn(Vector3 input)
    {
        respawn = input;
        respawn.y += 1;
    }

    public void SetState(State input)
    {
        state = input;
    }
}
