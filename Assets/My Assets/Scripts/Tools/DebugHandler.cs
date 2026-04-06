using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugHandler : MonoBehaviour
{

    [SerializeField]
    GameObject debugPanel_GO;

    [SerializeField]
    Text debugText_Cp;

    [SerializeField]
    bool enableDebug = true;

    public static DebugHandler instance;

    private void Awake()
    {
        instance = this;

        debugPanel_GO.SetActive(enableDebug);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AddDebugInfo(string text)
    {
        if (!enableDebug)
        {
            return;
        }

        debugText_Cp.text = debugText_Cp.text + (string.IsNullOrEmpty(debugText_Cp.text) ? string.Empty : '\n') + text;
    }

}
