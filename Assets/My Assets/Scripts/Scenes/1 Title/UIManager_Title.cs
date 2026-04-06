using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager_Title : MonoBehaviour
{

    Controller_Title controller_Cp;

    // Start is called before the first frame update
    void Start()
    {
        Init();        
    }

    void Init()
    {
        controller_Cp = GameObject.FindWithTag("GameController").GetComponent<Controller_Title>();
    }

    public void OnClick_TitlePanel()
    {
        controller_Cp.LoadNewScene();
    }

}
