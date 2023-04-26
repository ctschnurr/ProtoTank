using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{

    GameObject TargetA;
    GameObject TargetB;
    GameObject TargetC;
    GameObject TargetD;
    GameObject TargetE;
    GameObject TargetF;
    GameObject TargetG;
    GameObject TargetH;

    GameObject parent;

    float decay_timer = 0.0f;
    float fadeSpeed = 0.5f;

    bool destroyed = false;
    bool countMe = false;

    MissionManager manager;

    public string[] dialogueStrings;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.gameObject;
        manager = GameObject.Find("MissionManager").GetComponent<MissionManager>();

        TargetA = parent.transform.Find("TargetA").gameObject;
        TargetB = parent.transform.Find("TargetB").gameObject;
        TargetC = parent.transform.Find("TargetC").gameObject;
        TargetD = parent.transform.Find("TargetD").gameObject;
        TargetE = parent.transform.Find("TargetE").gameObject;
        TargetF = parent.transform.Find("TargetF").gameObject;
        TargetG = parent.transform.Find("TargetG").gameObject;
        TargetH = parent.transform.Find("TargetH").gameObject;

        decay_timer = decay_timer * Time.deltaTime;
        fadeSpeed = fadeSpeed * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyed)
        {
            decay_timer += 0.05f;

            if (decay_timer > 10)
            {
                Color tempcolor = TargetA.GetComponent<Renderer>().material.color;
                tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, fadeSpeed);
                
                TargetA.GetComponent<Renderer>().material.color = tempcolor;
                TargetB.GetComponent<Renderer>().material.color = tempcolor;
                TargetC.GetComponent<Renderer>().material.color = tempcolor;
                TargetD.GetComponent<Renderer>().material.color = tempcolor;
                TargetE.GetComponent<Renderer>().material.color = tempcolor;
                TargetF.GetComponent<Renderer>().material.color = tempcolor;
                TargetG.GetComponent<Renderer>().material.color = tempcolor;
                TargetH.GetComponent<Renderer>().material.color = tempcolor;

                if (TargetA.GetComponent<Renderer>().material.color.a == 0) Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Shell")
        {
            TargetA.GetComponent<Rigidbody>().isKinematic = false;
            TargetB.GetComponent<Rigidbody>().isKinematic = false;
            TargetC.GetComponent<Rigidbody>().isKinematic = false;
            TargetD.GetComponent<Rigidbody>().isKinematic = false;
            TargetE.GetComponent<Rigidbody>().isKinematic = false;
            TargetF.GetComponent<Rigidbody>().isKinematic = false;
            TargetG.GetComponent<Rigidbody>().isKinematic = false;
            TargetH.GetComponent<Rigidbody>().isKinematic = false;
            destroyed = true;
            countMe = true;
            manager.NextObjective(parent, dialogueStrings);
        }
    }

    public bool GetCountMe()
    {
        return countMe;
    }
}
