using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TargetController : Objective
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

    float decay_timer = 5f;
    float fadeSpeed = 0.5f;

    bool destroyed = false;
    bool countMe = false;

    public Component[] targetParts;
    public List<GameObject> pieces;

    public Vector3[] resetPositions;
    public Quaternion[] resetRotations;

    public Color tempcolor;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.gameObject;
        subjectObject = parent;
        missionManager = GameObject.Find("MissionManager").GetComponent<MissionManager>();
        screenManager = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();

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

        targetParts = GetComponentsInChildren<Renderer>();

        int numberOfPieces = parent.transform.childCount;
        pieces = new List<GameObject>();

        resetPositions = new Vector3[numberOfPieces];
        resetRotations = new Quaternion[numberOfPieces];

        for (int i = 0; i < numberOfPieces; i++)
        {
            Transform targetPieceTransform = parent.transform.GetChild(i);
            GameObject targetPiece = targetPieceTransform.gameObject;

            resetPositions[i] = targetPiece.transform.localPosition;
            resetRotations[i] = targetPiece.transform.localRotation;

            pieces.Add(targetPiece);
        }

        if (transform.parent != null)
        {
            if (transform.parent.gameObject.tag == "MissionGroup" || transform.parent.gameObject.tag == "ObjectiveGroup") managed = true;
        }

        if (preStrings.Length > 0) hasPreStrings = true;
        if (postStrings.Length > 0) hasPostStrings = true;

        tpShader = Shader.Find("Transparent/Diffuse");
        normalShader = Shader.Find("Standard");

        MissionManager.OnRunReset += Reset;
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyed)
        {
            decay_timer -= Time.deltaTime;

            if (decay_timer < 0)
            {
                foreach (Renderer partRenderer in targetParts)
                {
                    partRenderer.material.shader = tpShader;

                    tempcolor = partRenderer.material.color;
                    tempcolor.a = Mathf.MoveTowards(tempcolor.a, 0f, 0.005f);
                    partRenderer.material.color = tempcolor;

                    if (tempcolor.a == 0)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Shell")
        {
            foreach (GameObject piece in pieces)
            {
                piece.GetComponent<Rigidbody>().isKinematic = false;
            }
            destroyed = true;
            countMe = true;
            RunComplete();
        }
    }

    public bool GetCountMe()
    {
        return countMe;
    }

    void Reset()
    {
        if (destroyed)
        {

            for (int i = 0; i < pieces.Count; i++)
            {
                GameObject piece = pieces[i];

                piece.SetActive(true);
                piece.GetComponent<Rigidbody>().isKinematic = true;

                piece.transform.localPosition = resetPositions[i];
                piece.transform.localRotation = resetRotations[i];
            }

            foreach (Renderer partRenderer in targetParts)
            {
                tempcolor = partRenderer.material.color;
                tempcolor.a = 1f;
                partRenderer.material.color = tempcolor;

                partRenderer.material.shader = normalShader;
            }

            destroyed = false;
        }
    }
}
