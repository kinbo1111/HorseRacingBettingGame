using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Lobby : MonoBehaviour
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
    [SerializeField] Image hasRacingNewspaperImage_Cp;
    [SerializeField] Sprite hasRacingSprite, notHasRacngSprite;
    [SerializeField] Text moneyText_Cp;
    [SerializeField] Text ageText_Cp, monthText_Cp;
    [SerializeField] Text totalBettingCountText_Cp, successBettingCountText_Cp;
    [SerializeField] Button bettingButton_Cp, deliveringButton_Cp;
    [SerializeField] Text minBettingMoneyText_Cp;
    /// <summary>Lobby Middle Panel (最小ベット額 etc.); hidden while billing is open so it does not show through the modal.</summary>
    [SerializeField] GameObject lobbyMiddlePanelWhileBilling_GO;
    [SerializeField] GameObject billingPanel_GO;
    [SerializeField] RectTransform openBillingPlusButton_RT;
    [SerializeField] RectTransform billingPromoButton_RT;
    [SerializeField] RectTransform actionButtonsPanel_RT;
    [SerializeField] Sprite billingCoinSprite;
    [SerializeField] Sprite billingPriceBadgeSprite;
    [SerializeField] Sprite billingPackSmallSprite;
    [SerializeField] Sprite billingPackMediumSprite;
    [SerializeField] Sprite billingPackLargeSprite;
    [SerializeField] Sprite billingPackXLargeSprite;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_Lobby controller_Cp;
    DataManager dataManager_Cp;

    static readonly string[] BillingIapButtonNames =
    {
        "IAP Buy 1000 Button",
        "IAP Buy 5000 Button",
        "IAP Buy 10000 Button",
        "IAP Buy 50000 Button",
    };
    const string BillingPromoButtonName = "Open Billing Promo Button";
    const string LobbyMoneyChipBgName = "Lobby Money Chip Bg";
    const string LobbyMoneyBillingTapName = "Lobby Money Billing Tap";

    Sprite _lobbyUiSlicedSpriteCache;

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
    public bool hasRacingNewspaper
    {
        set { hasRacingNewspaperImage_Cp.sprite = value ? hasRacingSprite : notHasRacngSprite; }
    }
    public int money
    {
        set
        {
            moneyText_Cp.text = value.ToString();
            RefreshOpenBillingPlusLayout();
        }
    }
    public int age
    {
        set { ageText_Cp.text = value.ToString(); }
    }
    public int month
    {
        set { monthText_Cp.text = value.ToString(); }
    }
    public int totalBettingCount
    {
        set { totalBettingCountText_Cp.text = value.ToString(); }
    }
    public int successBettingCount
    {
        set { successBettingCountText_Cp.text = value.ToString(); }
    }
    public int minBettingMoney
    {
        set { minBettingMoneyText_Cp.text = value.ToString(); }
    }

    //-------------------------------------------------- private properties
    GameData_St gameData { get { return dataManager_Cp.gameData; } }

    #endregion

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Methods
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    void OnDestroy()
    {
        if (IapManager.Instance != null)
        {
            IapManager.Instance.PurchaseSucceeded -= OnIapPurchaseSucceeded;
        }
    }

    void OnIapPurchaseSucceeded()
    {
        money = gameData.money;
        if (dataManager_Cp != null)
        {
            SetActiveBettingBtn(gameData.money >= dataManager_Cp.minBettingMoney);
        }
        if (billingPanel_GO != null && billingPanel_GO.activeSelf)
        {
            ApplyBillingPanelLayoutAndStyle();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RefreshResponsiveLayout();
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
        InitVariables();
        if (billingPanel_GO != null)
        {
            billingPanel_GO.SetActive(false);
            ApplyBillingPanelLayoutAndStyle();
        }

        if (IapManager.Instance != null)
        {
            IapManager.Instance.PurchaseSucceeded -= OnIapPurchaseSucceeded;
            IapManager.Instance.PurchaseSucceeded += OnIapPurchaseSucceeded;
        }

        StartCoroutine(RefreshOpenBillingPlusNextFrame());
        StartCoroutine(RefreshResponsiveLayoutNextFrame());

        EnsureOpenBillingPlusReference();
        EnsureOpenBillingPlusRaycastWorks();

        mainGameState = GameState_En.Inited;
    }

    IEnumerator RefreshOpenBillingPlusNextFrame()
    {
        yield return null;
        RefreshOpenBillingPlusLayout();
    }

    IEnumerator RefreshResponsiveLayoutNextFrame()
    {
        yield return null;
        RefreshResponsiveLayout();
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Lobby>();
        dataManager_Cp = controller_Cp.dataManager_Cp;
        if (actionButtonsPanel_RT == null && bettingButton_Cp != null)
        {
            actionButtonsPanel_RT = bettingButton_Cp.transform.parent as RectTransform;
        }

        EnsureLobbyMiddlePanelWhileBillingRef();
    }

    void EnsureLobbyMiddlePanelWhileBillingRef()
    {
        if (lobbyMiddlePanelWhileBilling_GO != null)
        {
            return;
        }

        Transform canvasPanel = transform.Find("Canvas Panel");
        if (canvasPanel == null)
        {
            return;
        }

        Transform middle = canvasPanel.Find("Middle Panel");
        if (middle != null)
        {
            lobbyMiddlePanelWhileBilling_GO = middle.gameObject;
        }
    }

    public void InitGameDataUI()
    {
        hasRacingNewspaper = gameData.hasRacingNewspaper;
        money = gameData.money;
        age = gameData.age;
        month = gameData.month;
        totalBettingCount = gameData.totalBettingCount;
        successBettingCount = gameData.successBettingCount;
    }

    void InitVariables()
    {
        minBettingMoney = dataManager_Cp.minBettingMoney;
    }

    void EnsureOpenBillingPlusReference()
    {
        if (openBillingPlusButton_RT != null)
        {
            return;
        }
        GameObject go = GameObject.Find("Open Billing Button");
        if (go != null)
        {
            openBillingPlusButton_RT = go.GetComponent<RectTransform>();
        }
    }

    void EnsureBillingPromoButtonReference()
    {
        if (billingPromoButton_RT != null)
        {
            return;
        }

        GameObject existing = GameObject.Find(BillingPromoButtonName);
        if (existing != null)
        {
            billingPromoButton_RT = existing.GetComponent<RectTransform>();
            return;
        }

        Transform topPanel = GameObject.Find("Top Panel")?.transform;
        if (topPanel == null)
        {
            return;
        }

        GameObject promoGo = new GameObject(BillingPromoButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
        billingPromoButton_RT = promoGo.GetComponent<RectTransform>();
        billingPromoButton_RT.SetParent(topPanel, false);

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(billingPromoButton_RT, false);
    }

    void AttachOpenBillingButtonToMoneyPanel(RectTransform moneyPanelRt)
    {
        if (openBillingPlusButton_RT == null || moneyPanelRt == null)
        {
            return;
        }
        if (openBillingPlusButton_RT.parent != moneyPanelRt)
        {
            openBillingPlusButton_RT.SetParent(moneyPanelRt, false);
            openBillingPlusButton_RT.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Fully transparent Image + "Cull Transparent Mesh" often drops the mesh from the raycast list,
    /// so the Button never receives clicks. The "+" Text had raycastTarget off, so clicks missed too.
    /// </summary>
    void EnsureOpenBillingPlusRaycastWorks()
    {
        if (openBillingPlusButton_RT == null)
        {
            return;
        }
        Image hitGraphic = openBillingPlusButton_RT.GetComponent<Image>();
        if (hitGraphic != null)
        {
            hitGraphic.raycastTarget = true;
            hitGraphic.canvasRenderer.cullTransparentMesh = false;
            if (hitGraphic.color.a < 0.15f)
            {
                Color c = hitGraphic.color;
                c.a = 1f;
                hitGraphic.color = c;
            }
        }
        foreach (Text t in openBillingPlusButton_RT.GetComponentsInChildren<Text>(true))
        {
            t.raycastTarget = true;
        }
    }

    void EnsureLobbyUiSlicedSpriteCached()
    {
        if (_lobbyUiSlicedSpriteCache != null)
        {
            return;
        }

        if (billingPanel_GO != null)
        {
            Transform bc = billingPanel_GO.transform.Find("Billing Content");
            Image panelImg = bc != null ? bc.GetComponent<Image>() : null;
            if (panelImg != null && panelImg.sprite != null)
            {
                _lobbyUiSlicedSpriteCache = panelImg.sprite;
            }
        }

        if (_lobbyUiSlicedSpriteCache == null && openBillingPlusButton_RT != null)
        {
            Image ob = openBillingPlusButton_RT.GetComponent<Image>();
            if (ob != null && ob.sprite != null)
            {
                _lobbyUiSlicedSpriteCache = ob.sprite;
            }
        }
    }

    void EnsureLobbyMoneyChipBackground(RectTransform moneyPanelRt)
    {
        if (moneyPanelRt == null)
        {
            return;
        }

        EnsureLobbyUiSlicedSpriteCached();

        RectTransform bgRt = moneyPanelRt.Find(LobbyMoneyChipBgName) as RectTransform;
        if (bgRt == null)
        {
            GameObject go = new GameObject(LobbyMoneyChipBgName, typeof(RectTransform), typeof(Image));
            bgRt = go.GetComponent<RectTransform>();
            bgRt.SetParent(moneyPanelRt, false);
        }

        bgRt.SetAsFirstSibling();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.pivot = new Vector2(0.5f, 0.5f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgRt.localScale = Vector3.one;

        Image bgImg = bgRt.GetComponent<Image>();
        bgImg.sprite = _lobbyUiSlicedSpriteCache;
        bgImg.type = bgImg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        bgImg.color = new Color(0.06f, 0.1f, 0.18f, 0.93f);
        bgImg.raycastTarget = false;
        bgImg.preserveAspect = false;

        Outline outline = bgRt.GetComponent<Outline>();
        if (outline == null)
        {
            outline = bgRt.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0.38f, 0.78f, 1f, 0.42f);
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;

        Shadow shadow = bgRt.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = bgRt.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(0f, -4f);
        shadow.useGraphicAlpha = true;
    }

    /// <summary>
    /// Coin + amount had raycast off, so taps missed; this strip opens billing like the + control.
    /// </summary>
    void EnsureOpenBillingMoneyTapArea(RectTransform moneyPanelRt, float widthPx, float chipHeightPx)
    {
        if (moneyPanelRt == null || widthPx < 1f)
        {
            return;
        }

        EnsureLobbyUiSlicedSpriteCached();

        RectTransform tapRt = moneyPanelRt.Find(LobbyMoneyBillingTapName) as RectTransform;
        if (tapRt == null)
        {
            GameObject go = new GameObject(LobbyMoneyBillingTapName, typeof(RectTransform), typeof(Image), typeof(Button));
            tapRt = go.GetComponent<RectTransform>();
            tapRt.SetParent(moneyPanelRt, false);
        }

        int bgIndex = -1;
        for (int i = 0; i < moneyPanelRt.childCount; i++)
        {
            if (moneyPanelRt.GetChild(i).name == LobbyMoneyChipBgName)
            {
                bgIndex = i;
                break;
            }
        }
        tapRt.SetSiblingIndex(bgIndex >= 0 ? bgIndex + 1 : 0);

        tapRt.anchorMin = new Vector2(0f, 0.5f);
        tapRt.anchorMax = new Vector2(0f, 0.5f);
        tapRt.pivot = new Vector2(0f, 0.5f);
        tapRt.sizeDelta = new Vector2(widthPx, chipHeightPx);
        tapRt.anchoredPosition = Vector2.zero;
        tapRt.localScale = Vector3.one;

        Image tapImg = tapRt.GetComponent<Image>();
        tapImg.sprite = _lobbyUiSlicedSpriteCache;
        tapImg.type = tapImg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        tapImg.color = new Color(1f, 1f, 1f, 0.02f);
        tapImg.raycastTarget = true;
        tapImg.canvasRenderer.cullTransparentMesh = false;

        Button tapBtn = tapRt.GetComponent<Button>();
        tapBtn.transition = Selectable.Transition.None;
        tapBtn.targetGraphic = tapImg;
        tapBtn.onClick.RemoveAllListeners();
        tapBtn.onClick.AddListener(OnClick_OpenBillingPanelBtn);
    }

    static void EnsureBillingPlusDivider(RectTransform plusButtonRt)
    {
        if (plusButtonRt == null)
        {
            return;
        }

        const string dividerName = "Lobby Plus Divider";
        RectTransform divRt = plusButtonRt.Find(dividerName) as RectTransform;
        if (divRt == null)
        {
            GameObject divGo = new GameObject(dividerName, typeof(RectTransform), typeof(Image));
            divRt = divGo.GetComponent<RectTransform>();
            divRt.SetParent(plusButtonRt, false);
        }

        divRt.SetAsFirstSibling();
        divRt.anchorMin = new Vector2(0f, 0.12f);
        divRt.anchorMax = new Vector2(0f, 0.88f);
        divRt.pivot = new Vector2(0f, 0.5f);
        divRt.sizeDelta = new Vector2(2f, 0f);
        divRt.anchoredPosition = Vector2.zero;
        divRt.localScale = Vector3.one;

        Image divImg = divRt.GetComponent<Image>();
        divImg.sprite = null;
        divImg.color = new Color(1f, 1f, 1f, 0.22f);
        divImg.raycastTarget = false;
    }

    void StyleLobbyMoneyTextForChip()
    {
        if (moneyText_Cp == null)
        {
            return;
        }

        moneyText_Cp.raycastTarget = false;
        Outline o = moneyText_Cp.GetComponent<Outline>();
        if (o == null)
        {
            o = moneyText_Cp.gameObject.AddComponent<Outline>();
        }
        o.effectColor = new Color(0f, 0f, 0f, 0.55f);
        o.effectDistance = new Vector2(1.2f, -1.2f);
        o.useGraphicAlpha = true;
        moneyText_Cp.fontSize = Mathf.Max(moneyText_Cp.fontSize, 48);
    }

    static float MeasureMoneyTextWidthPixels(Text text, RectTransform textRt)
    {
        float h = Mathf.Max(1f, textRt.rect.height);
        TextGenerationSettings gen = text.GetGenerationSettings(new Vector2(8192f, h));
        gen.horizontalOverflow = HorizontalWrapMode.Overflow;
        gen.verticalOverflow = VerticalWrapMode.Overflow;
        float ppu = Mathf.Max(text.pixelsPerUnit, 0.0001f);
        return text.cachedTextGeneratorForLayout.GetPreferredWidth(text.text, gen) / ppu;
    }

    static float CapMoneyTextWidthEstimate(Text text, float measured)
    {
        if (text == null || string.IsNullOrEmpty(text.text))
        {
            return measured;
        }
        int len = text.text.Length;
        float font = Mathf.Max(1f, text.fontSize);
        float generous = font * Mathf.Max(len, 4) * 1.35f;
        generous = Mathf.Min(generous, 900f);
        return Mathf.Min(measured, generous);
    }

    void RefreshOpenBillingPlusLayout()
    {
        EnsureOpenBillingPlusReference();
        EnsureBillingPromoButtonReference();
        if (openBillingPlusButton_RT == null || moneyText_Cp == null)
        {
            return;
        }

        openBillingPlusButton_RT.gameObject.SetActive(true);

        RectTransform textRt = moneyText_Cp.rectTransform;
        RectTransform moneyPanelRt = textRt.parent as RectTransform;
        AttachOpenBillingButtonToMoneyPanel(moneyPanelRt);
        if (moneyPanelRt == null)
        {
            return;
        }

        EnsureLobbyUiSlicedSpriteCached();
        EnsureLobbyMoneyChipBackground(moneyPanelRt);
        StyleLobbyMoneyTextForChip();

        RectTransform coinRt = moneyPanelRt.Find("Money Image") as RectTransform;
        Image coinImg = coinRt != null ? coinRt.GetComponent<Image>() : null;
        if (coinImg != null)
        {
            coinImg.raycastTarget = false;
            coinImg.preserveAspect = true;
        }

        const float chipHeight = 86f;
        const float padLeft = 18f;
        const float padRight = 16f;
        const float coinSize = 56f;
        const float gapAfterCoin = 12f;
        const float gapBeforePlus = 14f;
        const float plusWidth = 78f;
        const float plusHeight = 72f;

        openBillingPlusButton_RT.anchorMin = new Vector2(0f, 0.5f);
        openBillingPlusButton_RT.anchorMax = new Vector2(0f, 0.5f);
        openBillingPlusButton_RT.pivot = new Vector2(0f, 0.5f);
        openBillingPlusButton_RT.sizeDelta = new Vector2(plusWidth, plusHeight);

        if (coinRt != null)
        {
            coinRt.anchorMin = new Vector2(0f, 0.5f);
            coinRt.anchorMax = new Vector2(0f, 0.5f);
            coinRt.pivot = new Vector2(0.5f, 0.5f);
            coinRt.sizeDelta = new Vector2(coinSize, coinSize);
            coinRt.anchoredPosition = new Vector2(padLeft + coinSize * 0.5f, 0f);
        }

        textRt.anchorMin = new Vector2(0f, 0.5f);
        textRt.anchorMax = new Vector2(0f, 0.5f);
        textRt.pivot = new Vector2(0f, 0.5f);
        textRt.anchoredPosition = new Vector2(padLeft + coinSize + gapAfterCoin, 0f);

        Canvas.ForceUpdateCanvases();
        float textWidth = MeasureMoneyTextWidthPixels(moneyText_Cp, textRt);
        textWidth = CapMoneyTextWidthEstimate(moneyText_Cp, textWidth);
        if (!float.IsFinite(textWidth) || textWidth < 1f)
        {
            textWidth = Mathf.Max(72f, moneyText_Cp.preferredWidth);
        }
        textWidth = Mathf.Max(textWidth, 68f);
        textRt.sizeDelta = new Vector2(textWidth + 8f, Mathf.Max(58f, textRt.sizeDelta.y));

        float plusLeft = padLeft + coinSize + gapAfterCoin + textWidth + gapBeforePlus;
        openBillingPlusButton_RT.anchoredPosition = new Vector2(plusLeft, 0f);

        float needW = plusLeft + plusWidth + padRight;
        moneyPanelRt.sizeDelta = new Vector2(needW, chipHeight);

        EnsureOpenBillingMoneyTapArea(moneyPanelRt, plusLeft, chipHeight);
        StyleOpenBillingEntryButton();
        StyleBillingPromoButton();
        EnsureBillingPlusDivider(openBillingPlusButton_RT);
        EnsureOpenBillingPlusRaycastWorks();
    }

    void StyleOpenBillingEntryButton()
    {
        if (openBillingPlusButton_RT == null)
        {
            return;
        }

        EnsureLobbyUiSlicedSpriteCached();

        Image bg = openBillingPlusButton_RT.GetComponent<Image>();
        if (bg != null)
        {
            bg.sprite = _lobbyUiSlicedSpriteCache;
            bg.type = bg.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            bg.preserveAspect = false;
            bg.color = new Color(0.14f, 0.38f, 0.72f, 0.98f);
            // Widen touch target on phones without changing the visible rect (Material Design ~48dp).
            bg.raycastPadding = new Vector4(16f, 14f, 16f, 14f);
        }

        Button btn = openBillingPlusButton_RT.GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.94f, 0.97f, 1f, 1f);
            cb.pressedColor = new Color(0.82f, 0.9f, 1f, 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
        }

        Outline accentOutline = openBillingPlusButton_RT.GetComponent<Outline>();
        if (accentOutline == null)
        {
            accentOutline = openBillingPlusButton_RT.gameObject.AddComponent<Outline>();
        }
        accentOutline.effectColor = new Color(1f, 0.92f, 0.45f, 0.35f);
        accentOutline.effectDistance = new Vector2(1f, -1f);
        accentOutline.useGraphicAlpha = true;

        Text label = openBillingPlusButton_RT.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.text = "+";
            label.fontSize = 46;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.98f, 0.88f, 1f);
            RectTransform labelRt = label.rectTransform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.offsetMin = new Vector2(5f, 0f);
            labelRt.offsetMax = Vector2.zero;

            Outline labelOl = label.GetComponent<Outline>();
            if (labelOl == null)
            {
                labelOl = label.gameObject.AddComponent<Outline>();
            }
            labelOl.effectColor = new Color(0f, 0f, 0f, 0.45f);
            labelOl.effectDistance = new Vector2(1f, -1f);
            labelOl.useGraphicAlpha = true;
        }
    }

    void StyleBillingPromoButton()
    {
        if (billingPromoButton_RT == null)
        {
            return;
        }

        // Hide the extra lobby title banner; user requested title only inside billing page.
        billingPromoButton_RT.gameObject.SetActive(false);
        return;

        billingPromoButton_RT.anchorMin = new Vector2(0f, 1f);
        billingPromoButton_RT.anchorMax = new Vector2(0f, 1f);
        billingPromoButton_RT.pivot = new Vector2(0f, 1f);
        billingPromoButton_RT.anchoredPosition = new Vector2(12f, -66f);
        billingPromoButton_RT.sizeDelta = new Vector2(430f, 44f);
        billingPromoButton_RT.SetAsLastSibling();

        Image bg = billingPromoButton_RT.GetComponent<Image>();
        if (bg != null)
        {
            bg.sprite = null;
            bg.type = Image.Type.Simple;
            bg.color = new Color(0.1f, 0.55f, 0.95f, 0.92f);
            bg.raycastTarget = true;
        }

        Button btn = billingPromoButton_RT.GetComponent<Button>();
        if (btn != null)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.88f, 0.95f, 1f, 1f);
            cb.pressedColor = new Color(0.78f, 0.88f, 1f, 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;
            btn.targetGraphic = bg;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick_OpenBillingPanelBtn);
        }

        Text label = billingPromoButton_RT.GetComponentInChildren<Text>(true);
        if (label == null)
        {
            return;
        }
        label.text = "有料アイテムゲットで的中率アップ";
        label.font = moneyText_Cp != null && moneyText_Cp.font != null
            ? moneyText_Cp.font
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 23;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = true;
        RectTransform labelRt = label.rectTransform;
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
    }

    void RefreshResponsiveLayout()
    {
        RefreshActionButtonsLayout();
        RefreshOpenBillingPlusLayout();
    }

    void RefreshActionButtonsLayout()
    {
        if (actionButtonsPanel_RT == null)
        {
            if (bettingButton_Cp != null)
            {
                actionButtonsPanel_RT = bettingButton_Cp.transform.parent as RectTransform;
            }
            if (actionButtonsPanel_RT == null)
            {
                return;
            }
        }

        // Place action buttons manually so they always appear as one column on screen.
        HorizontalLayoutGroup horizontalLayout = actionButtonsPanel_RT.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null) horizontalLayout.enabled = false;
        VerticalLayoutGroup verticalLayout = actionButtonsPanel_RT.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout != null) verticalLayout.enabled = false;

        RectTransform parentRt = actionButtonsPanel_RT.parent as RectTransform;
        if (parentRt == null)
        {
            return;
        }

        List<RectTransform> buttonRects = new List<RectTransform>();
        for (int i = 0; i < actionButtonsPanel_RT.childCount; i++)
        {
            RectTransform child = actionButtonsPanel_RT.GetChild(i) as RectTransform;
            if (child != null && child.gameObject.activeSelf)
            {
                buttonRects.Add(child);
            }
        }
        if (buttonRects.Count == 0)
        {
            return;
        }

        const float edgeMargin = 24f;
        const float minButtonWidth = 170f;
        const float fallbackButtonWidth = 280f;
        const float fallbackButtonHeight = 80f;
        const float spacing = 16f;

        float requiredHeight = 0f;
        float maxButtonWidth = 0f;
        float maxButtonHeight = fallbackButtonHeight;
        for (int i = 0; i < buttonRects.Count; i++)
        {
            float preferred = LayoutUtility.GetPreferredSize(buttonRects[i], 0);
            float measuredWidth = Mathf.Max(
                preferred,
                buttonRects[i].rect.width,
                buttonRects[i].sizeDelta.x,
                fallbackButtonWidth);
            float measuredHeight = Mathf.Max(
                LayoutUtility.GetPreferredSize(buttonRects[i], 1),
                buttonRects[i].rect.height,
                buttonRects[i].sizeDelta.y,
                fallbackButtonHeight);
            maxButtonWidth = Mathf.Max(maxButtonWidth, measuredWidth);
            maxButtonHeight = Mathf.Max(maxButtonHeight, measuredHeight);
            requiredHeight += measuredHeight;
            if (i < buttonRects.Count - 1)
            {
                requiredHeight += spacing;
            }
        }

        float availableWidth = Mathf.Max(0f, parentRt.rect.width - edgeMargin * 2f);
        if (availableWidth <= 0f)
        {
            return;
        }

        float buttonWidth = Mathf.Clamp(maxButtonWidth, minButtonWidth, availableWidth);
        float buttonHeight = maxButtonHeight;
        float targetWidth = buttonWidth;
        float targetHeight = buttonRects.Count * buttonHeight + (buttonRects.Count - 1) * spacing;
        actionButtonsPanel_RT.anchorMin = new Vector2(1f, 0f);
        actionButtonsPanel_RT.anchorMax = new Vector2(1f, 0f);
        actionButtonsPanel_RT.pivot = new Vector2(1f, 0f);
        actionButtonsPanel_RT.anchoredPosition = new Vector2(-edgeMargin, edgeMargin);
        actionButtonsPanel_RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        actionButtonsPanel_RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        float y = targetHeight - buttonHeight;
        for (int i = 0; i < buttonRects.Count; i++)
        {
            RectTransform rt = buttonRects[i];
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonHeight);

            LayoutElement element = buttonRects[i].GetComponent<LayoutElement>();
            if (element == null)
            {
                element = buttonRects[i].gameObject.AddComponent<LayoutElement>();
            }
            element.minWidth = buttonWidth;
            element.preferredWidth = buttonWidth;
            element.flexibleWidth = 0f;
            element.minHeight = buttonHeight;
            element.preferredHeight = buttonHeight;
            element.flexibleHeight = 0f;

            y -= buttonHeight + spacing;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(actionButtonsPanel_RT);
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        RefreshResponsiveLayout();
    }

    /// <summary>
    /// Forces billing UI layout/colors at runtime so updates apply even if the Lobby scene
    /// on disk was not saved or an older version is loaded in the Editor.
    /// </summary>
    void ApplyBillingPanelLayoutAndStyle()
    {
        if (billingPanel_GO == null)
        {
            return;
        }
        ResolveBillingSpriteFallbacks();

        Transform root = billingPanel_GO.transform;
        foreach (Outline o in root.GetComponentsInChildren<Outline>(true))
        {
            o.enabled = false;
        }

        Image backdrop = root.Find("Billing Backdrop")?.GetComponent<Image>();
        if (backdrop != null)
        {
            // Opaque enough that lobby UI (e.g. Middle Panel) does not bleed through.
            backdrop.color = new Color(0.04f, 0.06f, 0.1f, 0.9f);
        }

        RectTransform content = root.Find("Billing Content") as RectTransform;
        if (content == null)
        {
            return;
        }

        NormalizeBillingPurchaseButtons(content);

        content.SetAsLastSibling();
        content.anchorMin = new Vector2(0.5f, 0.5f);
        content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.anchoredPosition = new Vector2(0f, -16f);
        content.sizeDelta = new Vector2(1180f, 620f);
        Image panelBg = content.GetComponent<Image>();
        if (panelBg != null)
        {
            panelBg.color = new Color(0.72f, 0.92f, 0.98f, 0.97f);
            if (panelBg.sprite != null)
            {
                panelBg.type = Image.Type.Sliced;
            }
        }

        Shadow panelShadow = content.GetComponent<Shadow>();
        if (panelShadow == null)
        {
            panelShadow = content.gameObject.AddComponent<Shadow>();
        }
        panelShadow.effectColor = new Color(0.04f, 0.08f, 0.18f, 0.28f);
        panelShadow.effectDistance = new Vector2(0f, -8f);
        panelShadow.useGraphicAlpha = true;

        RectTransform titleRt = content.Find("Billing Title Text") as RectTransform;
        if (titleRt != null)
        {
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -30f);
            float titleWidth = Mathf.Max(280f, content.sizeDelta.x - 56f);
            titleRt.sizeDelta = new Vector2(titleWidth, 52f);
        }

        Text titleTxt = content.Find("Billing Title Text")?.GetComponent<Text>();
        if (titleTxt != null)
        {
            titleTxt.gameObject.SetActive(true);
            titleTxt.text = "有料アイテムゲットで的中率アップ";
            titleTxt.color = Color.white;
            titleTxt.fontSize = 34;
            titleTxt.fontStyle = FontStyle.Normal;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleTxt.verticalOverflow = VerticalWrapMode.Overflow;
            titleTxt.raycastTarget = false;
        }

        const float moneyRowCenterY = -96f;
        RectTransform hintRt = content.Find("Billing Hint Text") as RectTransform;
        if (hintRt != null)
        {
            hintRt.anchorMin = new Vector2(0f, 1f);
            hintRt.anchorMax = new Vector2(0f, 1f);
            hintRt.pivot = new Vector2(0f, 0.5f);
            float coinSlot = 52f;
            float gap = 10f;
            hintRt.anchoredPosition = new Vector2(40f + coinSlot + gap, moneyRowCenterY);
            hintRt.sizeDelta = new Vector2(420f, 56f);
        }

        Text hintTxt = content.Find("Billing Hint Text")?.GetComponent<Text>();
        if (hintTxt != null)
        {
            hintTxt.gameObject.SetActive(true);
            hintTxt.text = gameData.money.ToString("N0", CultureInfo.InvariantCulture);
            hintTxt.fontSize = 40;
            hintTxt.fontStyle = FontStyle.Bold;
            hintTxt.alignment = TextAnchor.MiddleLeft;
            hintTxt.color = new Color(0.99f, 0.9f, 0.16f, 1f);
            hintTxt.raycastTarget = false;
        }

        EnsureBillingHeaderCoin(content, moneyRowCenterY);

        const float cellW = 640f;
        const float cellH = 78f;
        const float gapY = 18f;
        const float listTopY = -200f;
        for (int i = 0; i < BillingIapButtonNames.Length; i++)
        {
            float y = listTopY - i * (cellH + gapY);
            LayoutBillingTopBar(content, BillingIapButtonNames[i], new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(-12f, y), new Vector2(cellW, cellH));
            ApplyBillingTierVisuals(content, i, IapManager.Instance);
        }

        RectTransform closeRt = content.Find("Billing Close Button") as RectTransform;
        if (closeRt != null)
        {
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-22f, -18f);
            closeRt.sizeDelta = new Vector2(42f, 42f);
            StyleBillingImageButton(closeRt, new Color(0.82f, 0.24f, 0.34f, 1f),
                new Color(0.9f, 0.32f, 0.42f, 1f), new Color(0.72f, 0.16f, 0.26f, 1f));
            TrySetBillingButtonSliced(closeRt);
            Text closeLabel = closeRt.GetComponentInChildren<Text>(true);
            if (closeLabel != null)
            {
                closeLabel.text = "\u00D7";
                closeLabel.fontSize = 28;
                closeLabel.fontStyle = FontStyle.Bold;
                closeLabel.color = Color.white;
                closeLabel.alignment = TextAnchor.MiddleCenter;
            }
        }

        RefreshBillingPackButtonLabels();

        ReorderBillingContentForCorrectOverlay(content);

        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// IAP row buttons use an opaque Image; they must render below the title and balance row.
    /// </summary>
    static void ReorderBillingContentForCorrectOverlay(RectTransform content)
    {
        if (content == null)
        {
            return;
        }

        int idx = 0;
        for (int i = 0; i < BillingIapButtonNames.Length; i++)
        {
            Transform row = content.Find(BillingIapButtonNames[i]);
            if (row != null)
            {
                row.SetSiblingIndex(idx++);
            }
        }

        Transform title = content.Find("Billing Title Text");
        if (title != null)
        {
            title.SetSiblingIndex(idx++);
        }

        Transform coin = content.Find("Billing Header Coin");
        if (coin != null)
        {
            coin.SetSiblingIndex(idx++);
        }

        Transform hint = content.Find("Billing Hint Text");
        if (hint != null)
        {
            hint.SetSiblingIndex(idx++);
        }

        Transform close = content.Find("Billing Close Button");
        if (close != null)
        {
            close.SetSiblingIndex(idx++);
        }
    }

    void NormalizeBillingPurchaseButtons(RectTransform content)
    {
        if (content == null)
        {
            return;
        }

        List<RectTransform> purchaseButtons = new List<RectTransform>();
        RectTransform template = null;

        foreach (Transform child in content)
        {
            if (child == null || child.name == "Billing Title Text" || child.name == "Billing Hint Text" ||
                child.name == "Billing Close Button" || child.name == "Billing Header Coin")
            {
                continue;
            }

            RectTransform childRt = child as RectTransform;
            Button childBtn = child.GetComponent<Button>();
            if (childRt == null || childBtn == null)
            {
                continue;
            }

            purchaseButtons.Add(childRt);
            if (template == null)
            {
                template = childRt;
            }
        }

        if (template == null)
        {
            return;
        }

        while (purchaseButtons.Count < BillingIapButtonNames.Length)
        {
            GameObject cloneGo = Instantiate(template.gameObject, content);
            cloneGo.SetActive(true);
            RectTransform cloneRt = cloneGo.GetComponent<RectTransform>();
            cloneRt.localScale = Vector3.one;
            purchaseButtons.Add(cloneRt);
        }

        for (int i = 0; i < purchaseButtons.Count; i++)
        {
            RectTransform rt = purchaseButtons[i];
            bool shouldUse = i < BillingIapButtonNames.Length;
            rt.gameObject.SetActive(shouldUse);
            if (!shouldUse)
            {
                continue;
            }

            rt.name = BillingIapButtonNames[i];

            Button btn = rt.GetComponent<Button>();
            if (btn != null)
            {
                int catalogIndex = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClick_IapBuyPackByIndex(catalogIndex));
            }

            Text label = rt.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = string.Empty;
                label.supportRichText = true;
            }
        }
    }

    void ApplyBillingTierVisuals(Transform content, int tierIndex, IapManager iap)
    {
        RectTransform rt = content.Find(BillingIapButtonNames[tierIndex]) as RectTransform;
        if (rt == null)
        {
            return;
        }

        StyleBillingImageButton(rt, new Color(1f, 1f, 1f, 0.02f), new Color(1f, 1f, 1f, 0.06f),
            new Color(1f, 1f, 1f, 0.1f));
        TrySetBillingButtonSliced(rt);
        BuildBillingTierRow(rt, tierIndex, iap);
    }

    static void TrySetBillingButtonSliced(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }
        Image img = rt.GetComponent<Image>();
        if (img != null && img.sprite != null)
        {
            img.type = Image.Type.Sliced;
        }
    }

    void RefreshBillingPackButtonLabels()
    {
        if (billingPanel_GO == null)
        {
            return;
        }
        Transform content = billingPanel_GO.transform.Find("Billing Content");
        if (content == null)
        {
            return;
        }
        IapManager iap = IapManager.Instance;
        for (int i = 0; i < BillingIapButtonNames.Length; i++)
        {
            ApplyBillingTierVisuals(content, i, iap);
        }
    }

    static void SetPackButtonLabel(Transform buttonRoot, int catalogIndex, IapManager iap)
    {
        if (buttonRoot == null)
        {
            return;
        }
        Text label = buttonRoot.GetComponentInChildren<Text>(true);
        if (label == null)
        {
            return;
        }
        int grantedAmount = iap != null ? iap.GetGrantedGameYenForCatalogIndex(catalogIndex) : 0;
        string priceStr = iap != null ? iap.GetLocalizedPriceString(catalogIndex) : string.Empty;
        if (string.IsNullOrEmpty(priceStr) && iap != null)
        {
            priceStr = iap.GetFallbackDisplayPrice(catalogIndex);
        }
        string grantStr = grantedAmount > 0 ? grantedAmount.ToString("N0", CultureInfo.InvariantCulture) : "\u2014";
        if (string.IsNullOrEmpty(priceStr))
        {
            priceStr = "\u2014";
        }
        label.supportRichText = true;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "<size=26><b>+" + grantStr + "</b></size>\n<size=20><color=#dfe8ff>" + priceStr +
            "</color></size>";
    }

    Sprite GetBillingPackSprite(int catalogIndex)
    {
        switch (catalogIndex)
        {
            case 0: return billingPackSmallSprite;
            case 1: return billingPackMediumSprite;
            case 2: return billingPackLargeSprite;
            case 3: return billingPackXLargeSprite;
            default: return null;
        }
    }

    string GetBillingDisplayPrice(IapManager iap, int catalogIndex)
    {
        if (iap == null)
        {
            return string.Empty;
        }

        string fallback = iap.GetFallbackDisplayPrice(catalogIndex);
        string localized = iap.GetLocalizedPriceString(catalogIndex);

        if (Application.isEditor && !string.IsNullOrEmpty(fallback))
        {
            return fallback;
        }

        if (string.IsNullOrEmpty(localized))
        {
            return fallback;
        }

        // Fake store metadata often returns $0.01 in the Editor / test mode.
        if (!string.IsNullOrEmpty(fallback) && localized.Contains("0.01"))
        {
            return fallback;
        }

        return localized;
    }

    void ResolveBillingSpriteFallbacks()
    {
        if (billingCoinSprite == null)
        {
            Image moneyIcon = GameObject.Find("Money Image")?.GetComponent<Image>();
            if (moneyIcon != null)
            {
                billingCoinSprite = moneyIcon.sprite;
            }
        }

        if (billingPackSmallSprite == null) billingPackSmallSprite = billingCoinSprite;
        if (billingPackMediumSprite == null) billingPackMediumSprite = billingPackSmallSprite;
        if (billingPackLargeSprite == null) billingPackLargeSprite = billingPackMediumSprite;
        if (billingPackXLargeSprite == null) billingPackXLargeSprite = billingPackLargeSprite;
    }

    void EnsureBillingHeaderCoin(RectTransform content, float moneyRowCenterYFromTop)
    {
        RectTransform iconRt = content.Find("Billing Header Coin") as RectTransform;
        if (iconRt == null)
        {
            GameObject iconGo = new GameObject("Billing Header Coin", typeof(RectTransform), typeof(Image));
            iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.SetParent(content, false);
        }

        iconRt.anchorMin = new Vector2(0f, 1f);
        iconRt.anchorMax = new Vector2(0f, 1f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        float coinCenterX = 40f + 26f;
        iconRt.anchoredPosition = new Vector2(coinCenterX, moneyRowCenterYFromTop);
        iconRt.sizeDelta = new Vector2(52f, 52f);

        Image iconImg = iconRt.GetComponent<Image>();
        iconImg.sprite = billingCoinSprite;
        iconImg.color = billingCoinSprite != null ? Color.white : new Color(1f, 0.86f, 0.12f, 1f);
        iconImg.preserveAspect = true;
        iconImg.type = Image.Type.Simple;
        if (billingCoinSprite == null)
        {
            iconImg.type = Image.Type.Sliced;
        }

        Transform labelTr = iconRt.Find("Label");
        if (labelTr != null)
        {
            labelTr.gameObject.SetActive(false);
        }
    }

    void BuildBillingTierRow(RectTransform rt, int catalogIndex, IapManager iap)
    {
        if (rt == null)
        {
            return;
        }

        Image rootImg = rt.GetComponent<Image>();
        if (rootImg != null)
        {
            rootImg.color = Color.white;
            if (rootImg.sprite != null)
            {
                rootImg.type = Image.Type.Sliced;
            }
        }

        RectTransform priceBg = EnsureChildRect(rt, "PriceBg");
        Image priceBgImg = EnsureImage(priceBg, Color.white);
        priceBg.anchorMin = new Vector2(1f, 0.5f);
        priceBg.anchorMax = new Vector2(1f, 0.5f);
        priceBg.pivot = new Vector2(1f, 0.5f);
        priceBg.anchoredPosition = new Vector2(-12f, 0f);
        priceBg.sizeDelta = new Vector2(176f, 46f);

        priceBgImg.sprite = billingPriceBadgeSprite;
        if (priceBgImg.sprite != null)
        {
            priceBgImg.color = Color.white;
            priceBgImg.preserveAspect = false;
            priceBgImg.type = Image.Type.Sliced;
        }
        else
        {
            priceBgImg.color = new Color(0.98f, 0.74f, 0.26f, 1f);
            priceBgImg.type = Image.Type.Simple;
        }

        RectTransform iconRt = EnsureChildRect(rt, "PackIcon");
        Image iconImg = EnsureImage(iconRt, Color.white);
        iconRt.anchorMin = new Vector2(0f, 0.5f);
        iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.anchoredPosition = new Vector2(34f, -3f);
        iconRt.sizeDelta = new Vector2(54f, 54f);
        iconImg.sprite = GetBillingPackSprite(catalogIndex);
        if (iconImg.sprite != null)
        {
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.type = Image.Type.Simple;
        }
        else
        {
            iconImg.color = new Color(0.98f, 0.84f, 0.18f, 1f);
            iconImg.type = Image.Type.Sliced;
        }

        Transform iconTextTr = iconRt.Find("IconText");
        if (iconTextTr != null && iconImg.sprite != null)
        {
            iconTextTr.gameObject.SetActive(false);
        }
        else
        {
            Text iconText = EnsureText(iconRt, "IconText");
            iconText.text = "C";
            iconText.fontSize = 20;
            iconText.fontStyle = FontStyle.Bold;
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.color = new Color(0.82f, 0.46f, 0.04f, 1f);
        }

        Text amountText = rt.GetComponentInChildren<Text>(true);
        if (amountText == null)
        {
            amountText = EnsureText(rt, "Text");
        }
        amountText.rectTransform.anchorMin = new Vector2(0f, 0f);
        amountText.rectTransform.anchorMax = new Vector2(0f, 1f);
        amountText.rectTransform.pivot = new Vector2(0f, 0.5f);
        amountText.rectTransform.anchoredPosition = new Vector2(76f, -3f);
        amountText.rectTransform.sizeDelta = new Vector2(184f, 56f);
        amountText.alignment = TextAnchor.MiddleLeft;
        amountText.fontSize = 24;
        amountText.fontStyle = FontStyle.Bold;
        amountText.color = Color.white;
        amountText.horizontalOverflow = HorizontalWrapMode.Overflow;
        amountText.verticalOverflow = VerticalWrapMode.Overflow;
        amountText.raycastTarget = false;

        Text priceText = EnsureText(priceBg, "PriceText");
        priceText.rectTransform.anchorMin = Vector2.zero;
        priceText.rectTransform.anchorMax = Vector2.one;
        priceText.rectTransform.offsetMin = Vector2.zero;
        priceText.rectTransform.offsetMax = Vector2.zero;
        priceText.alignment = TextAnchor.MiddleCenter;
        priceText.fontSize = 22;
        priceText.fontStyle = FontStyle.Bold;
        priceText.color = priceBgImg.sprite != null ? Color.white : new Color(0.25f, 0.2f, 0.08f, 1f);

        int grantedAmount = iap != null ? iap.GetGrantedGameYenForCatalogIndex(catalogIndex) : 0;
        string priceStr = GetBillingDisplayPrice(iap, catalogIndex);

        amountText.text = "+" + (grantedAmount > 0 ? grantedAmount.ToString("N0", CultureInfo.InvariantCulture) : "---");
        priceText.text = string.IsNullOrEmpty(priceStr) ? "---" : priceStr;
    }

    static RectTransform EnsureChildRect(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child as RectTransform;
        }

        GameObject go = new GameObject(childName, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    static Image EnsureImage(RectTransform rt, Color color)
    {
        Image img = rt.GetComponent<Image>();
        if (img == null)
        {
            img = rt.gameObject.AddComponent<Image>();
        }
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static Text EnsureText(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        Text text;
        if (child == null)
        {
            GameObject go = new GameObject(childName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            text = go.GetComponent<Text>();
        }
        else
        {
            text = child.GetComponent<Text>();
        }

        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        text.raycastTarget = false;
        return text;
    }

    static void StyleBillingImageButton(RectTransform rt, Color normal, Color highlighted, Color pressed)
    {
        if (rt == null)
        {
            return;
        }
        Image img = rt.GetComponent<Image>();
        if (img != null)
        {
            img.color = Color.white;
        }
        Button btn = rt.GetComponent<Button>();
        if (btn != null)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = normal;
            cb.highlightedColor = highlighted;
            cb.pressedColor = pressed;
            cb.selectedColor = highlighted;
            cb.disabledColor = new Color(0.85f, 0.85f, 0.88f, 0.45f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;
        }
    }

    static void StyleBillingPackButtonTexts(Transform buttonRoot, Color titleColor, int titleSize)
    {
        if (buttonRoot == null)
        {
            return;
        }
        Text label = buttonRoot.GetComponentInChildren<Text>(true);
        if (label == null)
        {
            return;
        }
        label.fontSize = titleSize;
        label.color = titleColor;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.lineSpacing = 1.05f;
    }

    static void LayoutBillingTopBar(Transform content, string childName, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (content == null)
        {
            return;
        }
        RectTransform rt = content.Find(childName) as RectTransform;
        if (rt == null)
        {
            return;
        }
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
    }

    #endregion

    public void SetActiveBettingBtn(bool active)
    {
        bettingButton_Cp.interactable = active;
    }

    public void SetActiveDeliveringBtn(bool active)
    {
        deliveringButton_Cp.interactable = active;
    }

    //////////////////////////////////////////////////////////////////////
    /// Callback from UI
    //////////////////////////////////////////////////////////////////////
    #region Callback from UI

    public void OnClick_BettingBtn()
    {
        controller_Cp.OnClick_BettingBtn();
    }

    public void OnClick_DeliveringBtn()
    {
        controller_Cp.OnClick_DeliveringBtn();
    }

    public void OnClick_EscapeBtn()
    {
        controller_Cp.Escape();
    }

    public void OnClick_OpenBillingPanelBtn()
    {
        if (billingPanel_GO != null)
        {
            EnsureLobbyMiddlePanelWhileBillingRef();
            if (lobbyMiddlePanelWhileBilling_GO != null)
            {
                lobbyMiddlePanelWhileBilling_GO.SetActive(false);
            }

            ApplyBillingPanelLayoutAndStyle();
            billingPanel_GO.SetActive(true);
        }
    }

    public void OnClick_CloseBillingPanelBtn()
    {
        if (billingPanel_GO != null)
        {
            billingPanel_GO.SetActive(false);
        }

        if (lobbyMiddlePanelWhileBilling_GO != null)
        {
            lobbyMiddlePanelWhileBilling_GO.SetActive(true);
        }
    }

    public void OnClick_IapBuyPack0()
    {
        OnClick_IapBuyPackByIndex(0);
    }

    public void OnClick_IapBuyPack1()
    {
        OnClick_IapBuyPackByIndex(1);
    }

    public void OnClick_IapBuyPack2()
    {
        OnClick_IapBuyPackByIndex(2);
    }

    public void OnClick_IapBuyPack3()
    {
        OnClick_IapBuyPackByIndex(3);
    }

    void OnClick_IapBuyPackByIndex(int index)
    {
        IapManager.Instance?.BuyProductByCatalogIndex(index);
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion

}
