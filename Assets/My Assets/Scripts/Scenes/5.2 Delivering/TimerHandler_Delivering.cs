using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerHandler_Delivering : MonoBehaviour
{

    [SerializeField] List<int> maxTimes = new List<int>();

    [ReadOnly] public int restTime;
    [ReadOnly] public UnityEvent timerEndEvent;

    Controller_Delivering controller_Cp;
    UIManager_Delivering ui_Cp;

    int maxTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init()
    {
        SetComponents();
        InitVariables();
        InitUI();
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        ui_Cp = controller_Cp.ui_Cp;
    }

    void InitVariables()
    {
        maxTime = maxTimes[controller_Cp.gLevelHandler_Cp.gLevelId];
        restTime = maxTime;
    }

    void InitUI()
    {
        ui_Cp.SetTime(restTime);
    }

    public void PlayTimer()
    {
        StartCoroutine(Corou_PlayTimer());
    }

    IEnumerator Corou_PlayTimer()
    {
        do
        {
            ui_Cp.SetTime(restTime);
            yield return new WaitForSeconds(1f);
            restTime--;
            ui_Cp.SetTime(restTime);
        } while (restTime > 0);   
        
        timerEndEvent.Invoke();
    }

}
