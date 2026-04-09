using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class UIManager_Ending : MonoBehaviour
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
    [SerializeField] Image resultImage_Cp;
    [SerializeField] Text resultText_Cp;
    [SerializeField] Text totalBettingCountText_Cp, bettingSuccCountText_Cp;
    // TextMeshPro support (optional)
    [SerializeField] TMP_Text resultText_TMP_Cp;
    [SerializeField] TMP_Text totalBettingCount_TMP_Cp, bettingSuccCount_TMP_Cp;
    [SerializeField] TMP_Text finalMoneyText_TMP_Cp;
    [SerializeField] Text finalMoneyText_Cp;
    [SerializeField] VideoPlayer videoPlayer_Cp;
    [Header("Result Text Visual Effects")]
    [SerializeField] bool applyResultTextEffects = true;
    [SerializeField] Color resultTextColor = Color.white;
    [SerializeField] bool useOutline = true;
    [SerializeField] Color outlineColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] Vector2 outlineDistance = new Vector2(2f, -2f);
    [SerializeField] bool useShadow = true;
    [SerializeField] Color shadowColor = new Color(0f, 0f, 0f, 0.9f);
    [SerializeField] Vector2 shadowDistance = new Vector2(4f, -4f);
    [Header("Result Text Animation")]
    [SerializeField] bool useFadeInAnimation = true;
    [SerializeField] float fadeInDelay = 0.1f;
    [SerializeField] float fadeInDuration = 0.45f;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_Ending controller_Cp;
    Coroutine resultTextFadeCoroutine_Cp;

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
    public Sprite resultSprite
    {
        set { if (resultImage_Cp != null) resultImage_Cp.sprite = value; }
    }
    public string resultText
    {
        set {
            if (resultText_Cp != null) resultText_Cp.text = value;
            if (resultText_TMP_Cp != null) resultText_TMP_Cp.text = value;
            PlayResultTextFadeIn();
        }
    }
    public int totalBettingCount
    {
        set {
            if (totalBettingCountText_Cp != null) totalBettingCountText_Cp.text = value.ToString() + "回";
            if (totalBettingCount_TMP_Cp != null) totalBettingCount_TMP_Cp.text = value.ToString() + "回";
        }
    }
    public int bettingSuccCountText
    {
        set {
            if (bettingSuccCountText_Cp != null) bettingSuccCountText_Cp.text = value.ToString() + "回";
            if (bettingSuccCount_TMP_Cp != null) bettingSuccCount_TMP_Cp.text = value.ToString() + "回";
        }
    }

    //-------------------------------------------------- private properties

    #endregion

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Methods
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    void Start()
    {

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

    public void Init()
    {
        AddGameStates(GameState_En.Nothing);

        SetComponents();
        ApplyResultTextVisualEffects();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Ending>();
    }

    // Improve readability on top of video backgrounds.
    void ApplyResultTextVisualEffects()
    {
        if (!applyResultTextEffects) return;

        if (resultText_Cp != null)
        {
            resultText_Cp.color = resultTextColor;

            if (useOutline)
            {
                Outline outlineCp = resultText_Cp.GetComponent<Outline>();
                if (outlineCp == null) outlineCp = resultText_Cp.gameObject.AddComponent<Outline>();
                outlineCp.effectColor = outlineColor;
                outlineCp.effectDistance = outlineDistance;
            }

            if (useShadow)
            {
                Shadow shadowCp = resultText_Cp.GetComponent<Shadow>();
                if (shadowCp == null) shadowCp = resultText_Cp.gameObject.AddComponent<Shadow>();
                shadowCp.effectColor = shadowColor;
                shadowCp.effectDistance = shadowDistance;
            }
        }

        if (resultText_TMP_Cp != null)
        {
            resultText_TMP_Cp.color = resultTextColor;
            resultText_TMP_Cp.fontStyle = FontStyles.Bold;
        }
    }

    void PlayResultTextFadeIn()
    {
        if (!useFadeInAnimation)
        {
            SetResultTextAlpha(1f);
            return;
        }

        if (resultTextFadeCoroutine_Cp != null)
        {
            StopCoroutine(resultTextFadeCoroutine_Cp);
        }
        resultTextFadeCoroutine_Cp = StartCoroutine(Coroutine_ResultTextFadeIn());
    }

    IEnumerator Coroutine_ResultTextFadeIn()
    {
        SetResultTextAlpha(0f);

        if (fadeInDelay > 0f)
        {
            yield return new WaitForSeconds(fadeInDelay);
        }

        float duration = Mathf.Max(0.01f, fadeInDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetResultTextAlpha(t);
            yield return null;
        }

        SetResultTextAlpha(1f);
        resultTextFadeCoroutine_Cp = null;
    }

    void SetResultTextAlpha(float alpha)
    {
        if (resultText_Cp != null)
        {
            Color c = resultTextColor;
            c.a = alpha;
            resultText_Cp.color = c;
        }

        if (resultText_TMP_Cp != null)
        {
            Color c = resultTextColor;
            c.a = alpha;
            resultText_TMP_Cp.color = c;
        }
    }

    #endregion

    public void OnClick_EscapeBtn()
    {
        controller_Cp.Escape();
    }

    // Set final money display
    public void SetFinalMoney(int money)
    {
        string txt = money.ToString("N0") + "円";
        if (finalMoneyText_Cp != null)
        {
            finalMoneyText_Cp.text = txt;
        }
        if (finalMoneyText_TMP_Cp != null)
        {
            finalMoneyText_TMP_Cp.text = txt;
        }
    }

    // Play a VideoClip in the assigned VideoPlayer
    public void PlayVideo(VideoClip clip)
    {
        if (videoPlayer_Cp == null || clip == null) return;

        videoPlayer_Cp.Stop();
        videoPlayer_Cp.clip = clip;
        videoPlayer_Cp.isLooping = false;
        videoPlayer_Cp.Play();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
