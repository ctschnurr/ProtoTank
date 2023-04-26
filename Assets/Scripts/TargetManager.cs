using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetManager : MonoBehaviour
{

    GameObject parent;
    GameObject counterObject;
    TextMeshProUGUI counterText;

    int numberOfTargets;
    int remainingTargets;

    private static List<GameObject> targets = new List<GameObject>();
    private static List<GameObject> removeList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.gameObject;
        numberOfTargets = parent.transform.childCount;

        for (int i = 0; i < numberOfTargets; i++)
        {
            Transform subject = parent.transform.GetChild(i);
            GameObject subjectGO = subject.gameObject;
            targets.Add(subjectGO);
        }

        remainingTargets = targets.Count;

        counterObject = GameObject.Find("HUD/counter");
        counterText = counterObject.GetComponent<TextMeshProUGUI>();
        counterText.text = "Remaining Targets: " + remainingTargets;
        counterText.color = new Color32(0, 0, 0, 255);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject target in targets)
        {
            TargetController targetTC = target.GetComponent<TargetController>();
            bool check = targetTC.GetCountMe();

            if(check == true)
            {
                removeList.Add(target);
            }
        }

        if (removeList.Count > 0) DoRemoveList();

        removeList.Clear();
        remainingTargets = targets.Count;

        if (remainingTargets == 0) counterText.text = "Remaining Targets: " + remainingTargets + "! Excellent work, soldier!";
        else counterText.text = "Remaining Targets: " + remainingTargets;
    }

    void DoRemoveList()
    {
        foreach (GameObject remove in removeList)
        {
            targets.Remove(remove);
        }
    }
}
