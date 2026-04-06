using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_Title : MonoBehaviour
{

    [SerializeField] CurtainHandler curtainHandler_Cp;
    [SerializeField] AudioHandler audio_Cp;
    [SerializeField] string nextSceneName;

    public UIManager_Title uiManager_Cp;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init()
    {
        CurtainUp();
    }

    void CurtainUp()
    {
        curtainHandler_Cp.CurtainOpen(() =>
        {
            audio_Cp.Play();
        });
    }

    public void LoadNewScene(string sceneName = null)
    {
        if (sceneName == null)
        {
            sceneName = nextSceneName;
        }

        audio_Cp.Finish();
        curtainHandler_Cp.CurtainClose(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

}
