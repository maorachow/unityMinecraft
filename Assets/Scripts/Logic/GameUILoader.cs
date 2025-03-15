using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUILoader : MonoBehaviour
{
    
    void Start()
    {
        GameObject mobileControlUIs = Resources.Load<GameObject>("Prefabs/mobilecontrolUIs");
        GameObject inGameUIs = Resources.Load<GameObject>("Prefabs/ingameUIs");
        GameObject canvas=GameObject.Find("Canvas");
        Instantiate(mobileControlUIs, canvas.transform);
        Instantiate(inGameUIs, canvas.transform);
    }
 
}
