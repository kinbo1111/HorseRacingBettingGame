using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AudioHandler : MonoBehaviour
{

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Types
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Types

    public enum GameState_En
    {
        Nothing, Inited, Playing,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] AudioSource bgdAudioS_Cp;
    [SerializeField] AudioClip bgdAudioClip, btnAudioClip;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    float maxBgdVolume = 0.2f, btnVolume = 0.4f;
    float volumeIncDur = 0.5f, volumeDecDur = 0.3f;

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Properties
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Properties

    //-------------------------------------------------- public properties
    public GameState_En mainGameState
    {
        get { return gameStates[0]; }
        set { gameStates[0] = value; }
    }
    public float bgdVolume
    {
        get { return bgdAudioS_Cp.volume; }
        set { bgdAudioS_Cp.volume = value; }
    }

    //-------------------------------------------------- private properties
    Tween audioTween;

    #endregion

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Methods
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        AddGameStates(GameState_En.Nothing);

        // init background music
        if (bgdAudioS_Cp == null)
        {
            bgdAudioS_Cp = gameObject.AddComponent<AudioSource>();
        }
        if (bgdAudioClip != null)
        {
            bgdAudioS_Cp.clip = bgdAudioClip;
        }
        bgdAudioS_Cp.playOnAwake = false;
        bgdAudioS_Cp.loop = true;
        bgdAudioS_Cp.volume = 0f;

        if (btnAudioClip == null)
        {
            btnAudioClip = Resources.Load<AudioClip>("Audio/mouse click");
        }

        mainGameState = GameState_En.Inited;
    }

    // Start is called before the first frame update
    void Start()
    {
        // init all buttons in the scene
        List<Button> allButton_Cps = new List<Button>(FindObjectsOfType<Button>());
        for (int i = 0; i < allButton_Cps.Count; i++)
        {
            if (allButton_Cps[i].GetComponentInChildren<AudioSource>() == null)
            {
                AudioSource btnAudioS_Cp_tp = allButton_Cps[i].AddComponent<AudioSource>();
                if (btnAudioClip != null)
                {
                    btnAudioS_Cp_tp.clip = btnAudioClip;
                }
                btnAudioS_Cp_tp.playOnAwake = false;
                btnAudioS_Cp_tp.loop = false;
                btnAudioS_Cp_tp.volume = btnVolume;
                allButton_Cps[i].onClick.AddListener(() => { btnAudioS_Cp_tp.Play(); });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Manage gameStates
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region ManageGameStates

    //--------------------------------------------------
    public void AddMainGameState(GameState_En value = GameState_En.Nothing)
    {
        if (gameStates.Count == 0)
        {
            gameStates.Add(value);
        }
    }

    //--------------------------------------------------
    public void AddGameStates(params GameState_En[] values)
    {
        foreach (GameState_En value_tp in values)
        {
            gameStates.Add(value_tp);
        }
    }

    //--------------------------------------------------
    public bool ExistGameStates(params GameState_En[] values)
    {
        bool result = true;
        foreach (GameState_En value in values)
        {
            if (!gameStates.Contains(value))
            {
                result = false;
                break;
            }
        }

        return result;
    }

    //--------------------------------------------------
    public bool ExistAnyGameStates(params GameState_En[] values)
    {
        bool result = false;
        foreach (GameState_En value in values)
        {
            if (gameStates.Contains(value))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //--------------------------------------------------
    public int GetExistGameStatesCount(GameState_En value)
    {
        int result = 0;

        for (int i = 0; i < gameStates.Count; i++)
        {
            if (gameStates[i] == value)
            {
                result++;
            }
        }

        return result;
    }

    //--------------------------------------------------
    public void RemoveGameStates(params GameState_En[] values)
    {
        foreach (GameState_En value in values)
        {
            gameStates.RemoveAll(gameState_tp => gameState_tp == value);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Initialize
    //////////////////////////////////////////////////////////////////////
    #region Initialize

    void Init()
    {
        
    }

    #endregion

    public void Play()
    {
        audioTween = DOTween.To(() => bgdVolume, x => bgdVolume = x, maxBgdVolume, volumeIncDur);
        bgdAudioS_Cp.Play();
    }

    public void Pause()
    {
        if (audioTween.IsActive())
        {
            audioTween.Kill();
        }
        audioTween = DOTween.To(() => bgdVolume, x => bgdVolume = x, 0f, volumeDecDur);
        bgdAudioS_Cp.Pause();
    }

    public void Resume()
    {
        if (audioTween.IsActive())
        {
            audioTween.Kill();
        }
        audioTween = DOTween.To(() => bgdVolume, x => bgdVolume = x, maxBgdVolume, volumeIncDur);
        bgdAudioS_Cp.UnPause();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Finish()
    {
        if (audioTween.IsActive())
        {
            audioTween.Kill();
        }
        DOTween.To(() => bgdVolume, x => bgdVolume = x, 0f, volumeDecDur)
            .OnComplete(() => { bgdAudioS_Cp.Stop(); });
    }

    #endregion
}
