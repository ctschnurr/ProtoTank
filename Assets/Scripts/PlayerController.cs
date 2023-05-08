using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;
    MissionManager missionManager;
    DialogueManager dialogueManager;
    ScreenManager screenManager;

    public enum State
    {
        controlEnabled,
        controlDisabled,
        dead
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

    Renderer bodyRenderer;
    Renderer chassisRenderer;
    Renderer leftTreadRenderer;
    Renderer rightTreadRenderer;

    GameObject checkpointPointer;
    bool checkpointerOn = false;
    bool pointerToggle = false;
    Vector3 pointerTarget;

    Vector3 pointerSizeChange;
    Vector3 pointerMin = new Vector3(0.0005f, 0.0005f, 0.0005f);
    float pointerSizeSpeed = 0.0000005f;

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

    Color normalColor;
    Color damageColor;

    bool damaged = false;
    bool vulnerable = true;

    int damageCount = 6;
    int damageCountReset = 6;

    float damageTimer = 0.1f;
    float damageTimerReset = 0.1f;

    int lives = 3;
    int livesMax = 3;

    public delegate void PlayerDamageAction();
    public static event PlayerDamageAction OnPlayerDamage;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();

        checkpointPointer = GameObject.Find("Player/ChassisRotater/WaypointPointer");
        checkpointPointer.SetActive(false);

        controlsScreen = GameObject.Find("ControlsScreen");
        controlsScreen.SetActive(false);

        player = GameObject.Find("Player");
        chassis = GameObject.Find("Player/ChassisRotater");
        barrel = GameObject.Find("Player/ChassisRotater/BarrelRotater");
        shotOrigin = GameObject.Find("Player/ChassisRotater/BarrelRotater/Barrel/ShotOrigin");

        bodyRenderer = GameObject.Find("Player/Body").GetComponent<Renderer>();
        chassisRenderer = GameObject.Find("Player/ChassisRotater/Chassis").GetComponent<Renderer>();
        leftTreadRenderer = GameObject.Find("Player/TreadLeft").GetComponent<Renderer>();
        rightTreadRenderer = GameObject.Find("Player/TreadRight").GetComponent<Renderer>();

        lastShotTimer = Time.time;

        reset = transform.position;
        // reset.y += 1;

        moveSpeed = moveSpeed * Time.deltaTime;
        barrelSpeed = barrelSpeed * Time.deltaTime;
        rotateSpeed = rotateSpeed * Time.deltaTime;

        normalColor = bodyRenderer.material.color;
        damageColor = new Color(1, 1, 1);

        barrel.transform.localRotation = Quaternion.Euler(60, 0, 0);

        pointerSizeSpeed += Time.deltaTime;
        pointerSizeChange = new Vector3(pointerSizeSpeed, pointerSizeSpeed, pointerSizeSpeed);
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

    public void Reset()
    {
        transform.position = reset;
        transform.rotation = Quaternion.identity;
        chassis.transform.rotation = Quaternion.identity;
        lives = livesMax;
    }

    // Update is called once per frame
    void Update()
    {

        switch (state)
        {
            case State.controlDisabled:

                return;

            case State.controlEnabled:

                // newMousePos = Input.mousePosition - lastMousePos;
                // xChange = newMousePos.y - lastMousePos.y;


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

                if (Input.GetKey(KeyCode.Space))
                {
                    dialogueManager.SetSkip();
                }

                if (Input.GetKeyDown(KeyCode.F) && flipped == false)
                {
                    flipped = true;
                    fliptimer = 5;
                }

                if (Input.GetKey(KeyCode.LeftShift)) throttle = true;
                if (Input.GetKeyUp(KeyCode.LeftShift)) throttle = false;

                if (checkpointerOn)
                {
                    GameObject pointerTargetObject = missionManager.GetNextCheckpoint();
                    if (pointerTargetObject != null)
                    {
                        pointerTarget = pointerTargetObject.transform.position;
                        Vector3 targetDirection = pointerTarget - checkpointPointer.transform.position;
                        Vector3 newDirection = Vector3.RotateTowards(checkpointPointer.transform.forward, targetDirection, (2.5f * Time.deltaTime), 0.0f);
                        checkpointPointer.transform.rotation = Quaternion.LookRotation(newDirection);
                    }
                    else return;
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

                // lastMousePos = Input.mousePosition;
                break;

            case State.dead:
                gameManager.SetState(GameManager.State.dead);

                bodyRenderer.material.color = damageColor;
                chassisRenderer.material.color = damageColor;
                leftTreadRenderer.material.color = damageColor;
                rightTreadRenderer.material.color = damageColor;
                break;
        }

        if (pointerToggle)
        {
            if (checkpointerOn)
            {
                checkpointPointer.transform.localScale -= pointerSizeChange;
                if (checkpointPointer.transform.localScale.x < 0.005f)
                {
                    pointerToggle = false;
                    checkpointPointer.SetActive(false);
                    checkpointerOn = false;
                }
            }
            else if (!checkpointerOn)
            {
                if (checkpointPointer.activeSelf == false)
                {
                    checkpointPointer.SetActive(true);
                    checkpointPointer.transform.localScale = pointerMin;
                }

                checkpointPointer.transform.localScale += pointerSizeChange;
                if (checkpointPointer.transform.localScale.x >= 0.4f)
                {

                    pointerToggle = false;
                    checkpointerOn = true;
                }
            }

        }
        
    }

    public void TogglePointer()
    {
        pointerToggle = true;
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Explosion")
        {
            if (vulnerable)
            {
                PlayerDamage();
                damaged = true;
                vulnerable = false;
                lives--;

                if (lives == 0) state = State.dead;
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
            damaged = false;
            vulnerable = true;
            damageCount = damageCountReset;
        }
    }

    public int GetLives()
    {
        return lives;
    }

    public void PlayerDamage()
    {
        if (OnPlayerDamage != null)
        {
            OnPlayerDamage();
        }
    }
}
